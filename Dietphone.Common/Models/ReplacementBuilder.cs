using System.Collections.Generic;
using System.Linq;
using System;

namespace Dietphone.Models
{
    public interface ReplacementBuilder
    {
        Replacements GetReplacementsFor(IList<MealItem> normalizedItems, IList<Pattern> usingPatterns);
    }

    public class ReplacementBuilderImpl : ReplacementBuilder
    {
        private const byte MAX_NOT_REPLACED_PERCENT_OF_ENERGY = 5;

        public Replacements GetReplacementsFor(IList<MealItem> normalizedItems, IList<Pattern> usingPatterns)
        {
            CheckPatterns(usingPatterns);
            var replacements = new List<Replacement>();
            foreach (var patternsFor in usingPatterns.GroupBy(p => p.For))
            {
                var top = patternsFor.OrderByDescending(p => p.RightnessPoints).First();
                var replacement = new Replacement { Pattern = top };
                replacements.Add(replacement);
            }
            return new Replacements { Items = replacements, Complete = GetComplete(normalizedItems, replacements) };
        }

        private void CheckPatterns(IList<Pattern> patterns)
        {
            foreach (var pattern in patterns)
                if (pattern.For == null)
                    throw new ArgumentException("Pattern.For cannot be null.");
        }

        private bool GetComplete(IList<MealItem> normalizedItems, List<Replacement> replacements)
        {
            int energySum = normalizedItems
                .Sum(item => item.Energy);
            float replacementEnergySum = replacements
                .Sum(replacement => replacement.Pattern.Match.Energy * replacement.Pattern.Factor);
            float notReplacedEnergy = Math.Abs(energySum - replacementEnergySum);
            double notReplacedEnergyPercent = notReplacedEnergy / energySum * 100;
            return notReplacedEnergyPercent <= MAX_NOT_REPLACED_PERCENT_OF_ENERGY;
        }
    }
}
