using System.Collections.Generic;
using System.Linq;
using System;

namespace Dietphone.Models
{
    public interface ReplacementBuilder
    {
        Replacement GetReplacementFor(IList<MealItem> normalizedItems, IList<Pattern> usingPatterns);
    }

    public class ReplacementBuilderImpl : ReplacementBuilder
    {
        private const byte MAX_NOT_REPLACED_PERCENT_OF_ENERGY = 5;

        public Replacement GetReplacementFor(IList<MealItem> normalizedItems, IList<Pattern> usingPatterns)
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
                IsComplete = IsComplete(normalizedItems, replacementItems)
            };
        }

        private void CheckPatterns(IList<Pattern> patterns)
        {
            foreach (var pattern in patterns)
                if (pattern.For == null)
                    throw new ArgumentException("Pattern.For cannot be null.");
        }

        private bool IsComplete(IList<MealItem> normalizedItems, List<ReplacementItem> replacementItems)
        {
            int energySum = normalizedItems
                .Sum(item => item.Energy);
            float replacementEnergySum = replacementItems
                .Sum(replacement => replacement.Pattern.Match.Energy * replacement.Pattern.Factor);
            float notReplacedEnergy = Math.Abs(energySum - replacementEnergySum);
            double notReplacedEnergyPercent = notReplacedEnergy / energySum * 100;
            return notReplacedEnergyPercent <= MAX_NOT_REPLACED_PERCENT_OF_ENERGY;
        }
    }
}
