using System;
using System.Collections.Generic;
using System.Linq;

namespace Dietphone.Models
{
    public interface SugarEstimator
    {
        IList<Sugar> GetEstimatedSugarsAfter(Meal meal, IList<ReplacementItem> usingReplacementItems);
    }

    public class SugarEstimatorImpl : SugarEstimator
    {
        public IList<Sugar> GetEstimatedSugarsAfter(Meal meal, IList<ReplacementItem> usingReplacementItems)
        {
            throw new NotImplementedException();
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
        public void Weigth(Meal meal, List<CollectedSugar> collectedSugars)
        {
            foreach (var collected in collectedSugars)
            {
                var pattern = collected.Source.Pattern;
                var mealItemsPercentOfEnergyInMeal
                    = pattern.For.PercentOfEnergyInMeal(meal);
                var replacementMealItemsPercentOfEnergyInReplacementMeal
                    = pattern.Match.PercentOfEnergyInMeal(pattern.From);
                collected.Weight 
                    = mealItemsPercentOfEnergyInMeal * replacementMealItemsPercentOfEnergyInReplacementMeal;
            }
        }
    }

    public class CollectedSugar
    {
        public Sugar Collected { get; set; }
        public Sugar Related { get; set; }
        public int Weight { get; set; }
        public ReplacementItem Source { get; set; }
    }
}
