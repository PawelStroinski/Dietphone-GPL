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

        [TestCase("1 100g 2 200g", "1 100g 2 200g", 1, 1)]
        [TestCase("1 100g", "1 50g", 2)]
        [TestCase("1 100g", "1 0g", 0)]
        [TestCase("1 0g", "1 100g", 0)]
        [TestCase("1 100g 2 200g", "1 10g 2 400g", 10, 0.5)]
        public void CalculatesPatternFactor(string meal, string patternMeal, params double[] expected)
        {
            var meal_ = AddMeal("12:00 " + meal);
            var patternMeal_ = AddMeal("12:00 " + patternMeal);
            var patterns = new List<Pattern>();
            for (int i = 0; i < expected.Count(); i++)
            {
                var pattern = new Pattern { From = patternMeal_, Match = patternMeal_.Items[i], For = meal_.Items[i] };
                patterns.Add(pattern);
            }
            var sut = new ReplacementBuilderImpl();
            var replacements = sut.GetReplacementsFor(meal_.Items, patterns).Items;
            Assert.AreEqual(expected, replacements.Select(r => r.PatternFactor));
        }
    }
}
