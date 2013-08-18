using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Globalization;

namespace Dietphone.Models.Tests
{
    public class ReplacementBuilderTests : ModelBasedTests
    {
        private void AddSomeMoreProductsForUseInTest()
        {
            Product product;
            product = AddProduct(7, energy: 200, carbs: 50, protein: 0, fat: 0);
            Assert.AreEqual(5, product.CuPer100g);
            Assert.AreEqual(0, product.FpuPer100g);
            Assert.AreEqual(product.EnergyPer100g, product.CalculatedEnergyPer100g);
            product = AddProduct(8, energy: 498, carbs: 0, protein: 12, fat: 50);
            Assert.AreEqual(0, product.CuPer100g);
            Assert.AreEqual(5, product.FpuPer100g);
            Assert.AreEqual(product.EnergyPer100g, product.CalculatedEnergyPer100g);
            product = AddProduct(9, energy: 690, carbs: 10, protein: 50, fat: 50);
            Assert.AreEqual(1, product.CuPer100g);
            Assert.AreEqual(6.5, product.FpuPer100g);
            Assert.AreEqual(product.EnergyPer100g, product.CalculatedEnergyPer100g);
        }

        [Test]
        public void ChoosesPatternsWithMostRightnessPoints()
        {
            var meal = AddMeal("12:00 1 100g 2 200g");
            var pattern1 = new Pattern { RightnessPoints = 1 };
            var pattern2 = new Pattern { RightnessPoints = 10 };
            var pattern3 = new Pattern { RightnessPoints = 5 };
            var pattern4 = new Pattern { RightnessPoints = 5 };
            var patterns = new Pattern[] { pattern1, pattern2, pattern3, pattern4 };
            foreach (var pattern in patterns)
            {
                pattern.From = meal;
                pattern.Match = meal.Items[0];
                pattern.For = meal.Items[0];
                pattern.Insulin = new Insulin();
            }
            pattern4.For = meal.Items[1];
            var sut = new ReplacementBuilderImpl();
            var replacementItems = sut.GetReplacementFor(meal, patterns).Items;
            Assert.AreEqual(new Pattern[] { pattern2, pattern4 }, replacementItems.Select(r => r.Pattern));
        }

        [Test]
        public void IfCalledWithForEqualToNullThenThrowsException()
        {
            var meal = AddMeal("12:00 1 100g");
            var patterns = new List<Pattern> { new Pattern { Match = meal.Items[0] } };
            var sut = new ReplacementBuilderImpl();
            var exception = Assert.Throws<ArgumentException>(()
                => sut.GetReplacementFor(meal, patterns));
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
                pattern.Insulin = new Insulin();
                patterns.Add(pattern);
            }
            var sut = new ReplacementBuilderImpl(new ReplacementBuilderImpl.IsComplete());
            var replacement = sut.GetReplacementFor(meal, patterns);
            Assert.AreEqual(expectedComplete, replacement.IsComplete);
        }

        [TestCase("1", "1 100g ; 1 * 1")]
        [TestCase("0.5", "1 100g 1 100g ; 1 * 1")]
        [TestCase("0", "0 100g ; 0 * 1")]
        [TestCase("1", "1 100g 2 200g 3 300g ; 3 * 2")]
        [TestCase("1.4", "1 100g 2 200g ; 2 * 1", "1 100g 2 200g ; 1 * 2")]
        [TestCase("0 1 1", "8 100g ; 0 1 1 * 1")]
        [TestCase("0 1 1.5", "8 100g 8 100g ; 0 2 3 * 1")]
        [TestCase("0 1 1", "8 100g 8 200g 8 300g ; 0 3 3 * 2")]
        [TestCase("0 1.4 2.6", "8 100g 8 200g ; 0 2 4 * 1", "8 100g 8 200g ; 0 1 2 * 2")]
        [TestCase("2.5 2.5 3", "7 100g ; 2.5 * 1", "8 100g ; 0 2.5 3 * 1")]
        [TestCase("3.5", "7 150g 8 100g 9 100g ; 2 2 3 * 2")]
        [TestCase("0 2.1 3.2", "8 150g 9 100g 7 100g ; 2 2 3 * 2")]
        [TestCase("0.9 4.0 5.3", "9 150g 7 100g 8 100g ; 2 3 4 * 2")]
        [TestCase("2 1.8 2.6", "8 100g 9 100g ; 2 3 4 * 0.5", "9 100g 8 100g ; 2 2 3 * 1")]
        public void CalculatesInsulinTotal(string expectedInsulin, params string[] mealInsulinAndFactor)
        {
            AddSomeMoreProductsForUseInTest();
            var patterns = new List<Pattern>();
            foreach (var item in mealInsulinAndFactor)
            {
                var splet = item.Split(';', '*').Select(s => s.Trim()).ToList();
                var meal = AddMeal("12:00 " + splet[0]);
                patterns.Add(new Pattern
                {
                    Match = meal.Items[0], // In this test we pretend that always the first item is used in pattern
                    From = meal,
                    Insulin = AddInsulin("12:00 " + splet[1]),
                    Factor = float.Parse(splet[2], new CultureInfo("en")),
                    For = new MealItem { ProductId = Guid.NewGuid() }
                });
            }
            var sut = new ReplacementBuilderImpl(new ReplacementBuilderImpl.InsulinTotal());
            var replacement = sut.GetReplacementFor(new Meal(), patterns);
            var expected = AddInsulin("12:00 " + expectedInsulin);
            var actual = replacement.InsulinTotal;
            Assert.AreEqual(expected.NormalBolus, actual.NormalBolus);
            Assert.AreEqual(expected.SquareWaveBolus, actual.SquareWaveBolus);
            Assert.AreEqual(expected.SquareWaveBolusHours, actual.SquareWaveBolusHours);
        }
    }
}
