using System;
using System.Collections.Generic;
using System.Linq;

namespace Dietphone.Models
{
    public interface SugarEstimator
    {
        IList<Sugar> GetEstimatedSugarsAfter(Meal meal, Sugar currentBefore,
            IList<ReplacementItem> usingReplacementItems);
    }

    public interface CuFpuSugarWeighter
    {
        float WeigthCu(Meal meal, CollectedSugar collectedSugar);
        float WeigthFpu(Meal meal, CollectedSugar collectedSugar);
    }

    public class SugarEstimatorImpl : SugarEstimator
    {
        private readonly SugarCollector sugarCollector = new SugarCollector();
        private readonly SugarRelator sugarRelator = new SugarRelator();
        private readonly SugarWeighter sugarWeighter;
        private readonly SugarAggregator sugarAggregator = new SugarAggregator();

        public SugarEstimatorImpl(Factories factories)
        {
            sugarWeighter = new SugarWeighter(new CuFpuSugarWeighterImpl(factories.Settings));
        }

        public IList<Sugar> GetEstimatedSugarsAfter(Meal meal, Sugar currentBefore,
            IList<ReplacementItem> usingReplacementItems)
        {
            var collectedByHour = sugarCollector.CollectByHour(meal, usingReplacementItems);
            var collectedSugars = collectedByHour.Values.SelectMany(values => values).ToList();
            sugarRelator.Relate(currentBefore, collectedSugars);
            sugarWeighter.Weight(meal, collectedSugars);
            var result = sugarAggregator.Aggregate(collectedByHour);
            return result.Keys.ToList();
        }
    }

    public class SugarCollector
    {
        private Meal meal;

        public Dictionary<TimeSpan, List<CollectedSugar>> CollectByHour(Meal meal,
            IList<ReplacementItem> replacementItems)
        {
            this.meal = meal;
            IEnumerable<Tuple<Sugar, ReplacementItem>> sugarTuples = replacementItems
                .SelectMany(replacementItem => replacementItem.Pattern.After,
                    (replacementItem, sugar) => new Tuple<Sugar, ReplacementItem>(sugar, replacementItem));
            var collectedSugars = sugarTuples.Select(tuple =>
                new CollectedSugar { Collected = GetSugarCopyWithRelativeTime(tuple), Source = tuple.Item2 });
            var groupped = collectedSugars
                .GroupBy(collectedSugar => new TimeSpan(collectedSugar.Collected.DateTime.Hour, 0, 0));
            return groupped.ToDictionary(groupping => groupping.Key + TimeSpan.FromMinutes(Math.Round(
                    groupping.Average(collectedSugar => collectedSugar.Collected.DateTime.Minute))),
                groupping => groupping.ToList());
        }

        private Sugar GetSugarCopyWithRelativeTime(Tuple<Sugar, ReplacementItem> sugarTuple)
        {
            var sugarMeal = sugarTuple.Item2.Pattern.From;
            var sugarTime = new TimeSpan(sugarTuple.Item1.DateTime.Ticks - sugarMeal.DateTime.Ticks);
            return new Sugar { DateTime = meal.DateTime + sugarTime, BloodSugar = sugarTuple.Item1.BloodSugar };
        }
    }

    public class SugarRelator
    {
        public void Relate(Sugar currentBefore, List<CollectedSugar> collectedSugars)
        {
            foreach (var collected in collectedSugars)
                collected.Related = new Sugar
                {
                    DateTime = collected.Collected.DateTime,
                    BloodSugar = currentBefore.BloodSugar
                        + (collected.Collected.BloodSugar - collected.Source.Pattern.Before.BloodSugar)
                        * collected.Source.Pattern.Factor
                };
        }
    }

    public class SugarWeighter
    {
        private readonly CuFpuSugarWeighter cuFpuSugarWeighter;

        public SugarWeighter(CuFpuSugarWeighter cuFpuSugarWeighter)
        {
            this.cuFpuSugarWeighter = cuFpuSugarWeighter;
        }

        public void Weight(Meal meal, List<CollectedSugar> collectedSugars)
        {
            foreach (var collected in collectedSugars)
            {
                var pattern = collected.Source.Pattern;
                var cuToFpuRatio = meal.CuToFpuRatio * pattern.From.CuToFpuRatio;
                var fpuToCuRatio = meal.FpuToCuRatio * pattern.From.FpuToCuRatio;
                var weight
                    = cuFpuSugarWeighter.WeigthCu(meal, collected) * cuToFpuRatio
                    + cuFpuSugarWeighter.WeigthFpu(meal, collected) * fpuToCuRatio;
                collected.Weight = (float)weight;
            }
        }
    }

    public class CuFpuSugarWeighterImpl : CuFpuSugarWeighter
    {
        private readonly Settings settings;
        private static readonly TimeSpan MAX_SMOOTHING = TimeSpan.FromHours(1);

        public CuFpuSugarWeighterImpl(Settings settings)
        {
            this.settings = settings;
        }

        public float WeigthCu(Meal meal, CollectedSugar collectedSugar)
        {
            var pattern = collectedSugar.Source.Pattern;
            var mealItemsPercentOfCuInMeal
                = pattern.For.PercentOfCuInMeal(meal);
            var replacementMealItemsPercentOfCuInReplacementMeal
                = pattern.Match.PercentOfCuInMeal(pattern.From);
            var timeSpan = collectedSugar.Related.DateTime - meal.DateTime;
            var margin = TimeSpan.FromHours(settings.CuSugarsHoursToExcludingPlusOneSmoothing);
            var pastMargin = timeSpan - margin;
            var time = CalculateTime(pastMargin);
            return mealItemsPercentOfCuInMeal * replacementMealItemsPercentOfCuInReplacementMeal * time;
        }

        public float WeigthFpu(Meal meal, CollectedSugar collectedSugar)
        {
            var pattern = collectedSugar.Source.Pattern;
            var mealItemsPercentOfFpuInMeal
                = pattern.For.PercentOfFpuInMeal(meal);
            var replacementMealItemsPercentOfFpuInReplacementMeal
                = pattern.Match.PercentOfFpuInMeal(pattern.From);
            var timeSpan = collectedSugar.Related.DateTime - meal.DateTime;
            var margin = TimeSpan.FromHours(settings.FpuSugarsHoursFromExcludingMinusOneSmoothing);
            var toMargin = margin - timeSpan;
            var time = CalculateTime(toMargin);
            return mealItemsPercentOfFpuInMeal * replacementMealItemsPercentOfFpuInReplacementMeal * time;
        }

        private float CalculateTime(TimeSpan overMargin)
        {
            if (overMargin > TimeSpan.Zero)
                if (overMargin < MAX_SMOOTHING)
                {
                    var smoothing = overMargin.TotalHours / MAX_SMOOTHING.TotalHours;
                    return (float)(1 - smoothing);
                }
                else
                    return 0;
            return 1f;
        }
    }

    public class SugarAggregator
    {
        public Dictionary<Sugar, List<CollectedSugar>> Aggregate(
            Dictionary<TimeSpan, List<CollectedSugar>> collectedByHour)
        {
            return collectedByHour
                .Where(kvp => kvp.Value.Sum(collected => collected.Weight) != 0)
                .ToDictionary(kvp => new Sugar
                {
                    BloodSugar = (float)Math.Round(
                        kvp.Value.Sum(collected => collected.Related.BloodSugar * collected.Weight)
                        / kvp.Value.Sum(collected => collected.Weight), 1),
                    DateTime = kvp.Value.First().Related.DateTime.Date + kvp.Key
                }, kvp => kvp.Value);
        }
    }

    public class CollectedSugar
    {
        public Sugar Collected { get; set; }
        public Sugar Related { get; set; }
        public float Weight { get; set; }
        public ReplacementItem Source { get; set; }
    }
}
