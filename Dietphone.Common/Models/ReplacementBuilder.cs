using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dietphone.Models
{
    public interface ReplacementBuilder
    {
        IList<Replacement> GetReplacementsFor(IList<MealItem> normalizedItems, IList<Pattern> usingPatterns);
    }

    public class ReplacementBuilderImpl : ReplacementBuilder
    {
        private readonly Factories factories;

        public ReplacementBuilderImpl(Factories factories)
        {
            this.factories = factories;
        }

        public IList<Replacement> GetReplacementsFor(IList<MealItem> normalizedItems, IList<Pattern> usingPatterns)
        {
            var replacements = new List<Replacement>();
            foreach (var patternsFor in usingPatterns.GroupBy(p => p.For))
            {
                var top = patternsFor.OrderByDescending(p => p.RightnessPoints).First();
                var replacement = new Replacement { Pattern = top };
                replacements.Add(replacement);
            }
            return replacements;
        }
    }
}
