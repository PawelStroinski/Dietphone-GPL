﻿using System.Collections.Generic;
using System.Linq;

namespace Dietphone.Models
{
    public interface ReplacementBuilder
    {
        Replacements GetReplacementsFor(IList<MealItem> normalizedItems, IList<Pattern> usingPatterns);
    }

    public class ReplacementBuilderImpl : ReplacementBuilder
    {
        public Replacements GetReplacementsFor(IList<MealItem> normalizedItems, IList<Pattern> usingPatterns)
        {
            var replacements = new List<Replacement>();
            foreach (var patternsFor in usingPatterns.GroupBy(p => p.For))
            {
                var top = patternsFor.OrderByDescending(p => p.RightnessPoints).First();
                var replacement = new Replacement { Pattern = top };
                replacements.Add(replacement);
            }
            return new Replacements { Items = replacements };
        }
    }
}