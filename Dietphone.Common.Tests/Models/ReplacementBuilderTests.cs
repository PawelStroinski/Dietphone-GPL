using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Dietphone.Models.Tests
{
    public class ReplacementBuilderTests : ModelBasedTests
    {
        [Test]
        public void ChoosesPatternsWithMostRightnessPoints()
        {
            var meal = AddMeal("12:00 1 100g");
            var pattern1 = new Pattern { RightnessPoints = 1 };
            var pattern2 = new Pattern { RightnessPoints = 10 };
            var pattern3 = new Pattern { RightnessPoints = 5 };
            var patterns = new List<Pattern> { pattern1, pattern2, pattern3 };
            foreach (var pattern in patterns)
            {
                pattern.From = meal;
                pattern.Match = meal.Items[0];
                pattern.For = meal.Items[0];
            }
            var sut = new ReplacementBuilderImpl();
            var replacements = sut.GetReplacementsFor(meal.Items, patterns).Items;
            Assert.AreSame(pattern2, replacements.Single().Pattern);
        }

        public void ReturnsReplacementsAsCompleteWhenAlmostAllEnergyIsReplaced()
        {
        }
    }
}
