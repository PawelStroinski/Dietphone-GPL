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

        [Test]
        public void IfCalledWitForEqualNullThenThrowsException()
        {
            var meal = AddMeal("12:00 1 100g");
            var patterns = new List<Pattern> { new Pattern { Match = meal.Items[0] } };
            var sut = new ReplacementBuilderImpl();
            var exception = Assert.Throws<ArgumentException>(()
                => sut.GetReplacementsFor(meal.Items, patterns));
            Assert.AreEqual("Pattern.For cannot be null.", exception.Message);
        }

        [TestCase("1 100g 2 200g 3 50g", true, 1)]
        [TestCase("1 100g 2 200g 3 33g", true, 1)]
        [TestCase("1 100g 2 200g 3 32g", false, 1)]
        [TestCase("1 107g 2 200g 3 26g", true, 1)]
        [TestCase("1 107g 2 199g 3 26g", false, 1)]
        [TestCase("1 10g 2 20g 3 5g", true, 10)]
        [TestCase("1 10g 2 24g", true, 10)]
        [TestCase("1 10g 2 23g", false, 10)]
        [TestCase("1 100g 2 200g 3 67g", true, 1)]
        [TestCase("1 100g 2 200g 3 68g", false, 1)]
        public void ReturnsReplacementsAsCompleteWhenAtLeast95PercentOfEnergyIsReplaced(string found,
            bool expectedComplete, int factor)
        {
            var meal = AddMeal("12:00 1 100g 2 200g 3 50g");
            var foundMeal = AddMeal("10:00 " + found);
            var patterns = new List<Pattern>();
            for (int i = 0; i < foundMeal.Items.Count; i++)
            {
                var pattern = new Pattern();
                pattern.Match = foundMeal.Items[i];
                pattern.From = foundMeal;
                pattern.Factor = factor;
                pattern.For = meal.Items[i];
                patterns.Add(pattern);
            }
            var sut = new ReplacementBuilderImpl();
            var replacements = sut.GetReplacementsFor(meal.Items, patterns);
            Assert.AreEqual(expectedComplete, replacements.Complete);
        }
    }
}
