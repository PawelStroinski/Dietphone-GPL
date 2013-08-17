using System.Collections.Generic;
using System.Linq;
using System;

namespace Dietphone.Models
{
    public interface ReplacementBuilder
    {
        Replacement GetReplacementFor(Meal meal, IList<Pattern> usingPatterns);
    }

    public class ReplacementBuilderImpl : ReplacementBuilder
    {
        private const byte MAX_NOT_REPLACED_PERCENT_OF_ENERGY = 5;

        public Replacement GetReplacementFor(Meal meal, IList<Pattern> usingPatterns)
        {
            CheckPatterns(usingPatterns);
            var replacementItems = new List<ReplacementItem>();
            foreach (var patternsFor in usingPatterns.GroupBy(p => p.For))
            {
                var top = patternsFor.OrderByDescending(p => p.RightnessPoints).First();
                var replacementItem = new ReplacementItem { Pattern = top };
                replacementItems.Add(replacementItem);
            }
            return new Replacement
            {
                Items = replacementItems,
                IsComplete = IsComplete(meal, replacementItems),
                InsulinTotal = InsulinTotal(replacementItems)
            };
        }

        private void CheckPatterns(IList<Pattern> patterns)
        {
            foreach (var pattern in patterns)
                if (pattern.For == null)
                    throw new ArgumentException("Pattern.For cannot be null.");
        }

        private bool IsComplete(Meal meal, List<ReplacementItem> replacementItems)
        {
            short energySum = meal.Energy;
            float replacementEnergySum = replacementItems
                .Sum(replacement => replacement.Pattern.Match.Energy * replacement.Pattern.Factor);
            float notReplacedEnergy = Math.Abs(energySum - replacementEnergySum);
            double notReplacedEnergyPercent = notReplacedEnergy / energySum * 100;
            return notReplacedEnergyPercent <= MAX_NOT_REPLACED_PERCENT_OF_ENERGY;
        }

        private Insulin InsulinTotal(List<ReplacementItem> replacementItems)
        {
            var insulinTotal = new Insulin();
            var patterns = replacementItems.Select(item => item.Pattern);
            foreach (var pattern in patterns)
            {
                var meal = pattern.From;
                var totalCuInMeal = meal.Items.Sum(item => item.Cu);
                var percentOfCu = totalCuInMeal == 0 ? 0 : pattern.Match.Cu / totalCuInMeal;
                insulinTotal.NormalBolus
                    += (float)Math.Round(percentOfCu * pattern.Insulin.NormalBolus * pattern.Factor, 1);
                var totalFpuInMeal = meal.Items.Sum(item => item.Fpu);
                var percentOfFpu = totalFpuInMeal == 0 ? 0 : pattern.Match.Fpu / totalFpuInMeal;
                insulinTotal.SquareWaveBolus
                    += (float)Math.Round(percentOfFpu * pattern.Insulin.SquareWaveBolus * pattern.Factor, 1);
                insulinTotal.SquareWaveBolusHours
                    += (float)Math.Round(percentOfFpu * pattern.Insulin.SquareWaveBolusHours * pattern.Factor, 1);
            }
            return insulinTotal;
        }
    }
}
