using System;
using System.Linq;
using NUnit.Framework;

namespace Dietphone.Models.Tests
{
    public class PatternBuilderTests : ModelBasedTests
    {
        [SetUp]
        public void Initialize()
        {
            factories.Settings.SugarsAfterInsulinHours = 4;
        }

        private PatternBuilderImpl CreateSut(params PatternBuilderImpl.IAction[] actions)
        {
            return new PatternBuilderImpl(factories, actions);
        }

        [Test]
        public void IfNoMatchingItemThenReturnsEmpty()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            AddMealInsulinAndSugars("10:00 2 100g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(0, patterns.Count());
        }

        [Test]
        public void IfMatchingItemThenReturnsIt()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            var mealToFind = AddMealInsulinAndSugars("10:00 1 100g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreSame(mealToFind.Items[0], patterns.Single().Match);
            Assert.AreSame(mealToFind, patterns.Single().From);
        }

        [Test]
        public void ReturnsMatchesForEveryItemInMeal()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g 2 200g");
            var mealToFind = AddMealInsulinAndSugars("10:00 1 100g 2 200g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(2, patterns.Count);
            Assert.AreSame(mealToFind.Items[0], patterns[0].Match);
            Assert.AreSame(mealToFind.Items[1], patterns[1].Match);
            Assert.AreSame(mealToFind, patterns[0].From);
            Assert.AreSame(mealToFind, patterns[1].From);
        }

        [Test]
        public void IfItemHasDifferentUnitThenDoesntReturnIt()
        {
            var product = AddProduct(55, energy: 100, carbs: 100, protein: 100, fat: 100);
            product.ServingSizeValue = 100;
            product.EnergyPerServing = 100;
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 55 100g");
            AddMealInsulinAndSugars("10:00 55 1serving", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(0, patterns.Count());
        }

        [Test]
        public void NormalizesMealtems()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g 1 100g");
            var mealToFind = AddMealInsulinAndSugars("10:00 1 100g 1 100g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(1, patterns.Count);
            Assert.AreEqual(mealToFind.Items[0].ProductId, patterns.Single().Match.ProductId);
            Assert.AreEqual(mealToFind.Items[0].Unit, patterns.Single().Match.Unit);
            Assert.AreEqual(200, patterns.Single().Match.Value);
            Assert.AreSame(mealToFind, patterns.Single().From);
        }

        [Test]
        public void ReturnsItemForWhichPatternWasFound()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            AddMealInsulinAndSugars("10:00 1 100g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreSame(meal.Items[0], patterns.Single().For);
        }

        [Test]
        public void ReturnsOnlyItemsHavingSimillarPercentageOfEnergyInMeal()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g 2 100g");
            var mealToFind1 = AddMealInsulinAndSugars("06:00 1 1000g 3 1000g", "1", "100 100");
            var mealNotFind = AddMealInsulinAndSugars("08:00 1 160g 3 100g", "1", "100 100");
            var mealToFind2 = AddMealInsulinAndSugars("10:00 1 150g 3 100g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.IsTrue(patterns.Any(p => p.From == mealToFind1), "Same percentage so should match");
            Assert.IsFalse(patterns.Any(p => p.From == mealNotFind), "Percentage different by 11 so should fail");
            Assert.IsTrue(patterns.Any(p => p.From == mealToFind2), "Percentage different by no more than 10 so matches");
        }

        [Test]
        public void MoreSimillarPercentageOfEnergyInMealGivesMoreRightnessPoints()
        {
            var sut = CreateSut(new PatternBuilderImpl.PointsForPercentOfEnergy());
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g 2 100g");
            AddMealInsulinAndSugars("07:00 1 100g 3 100g", "1", "100 100");
            AddMealInsulinAndSugars("07:00 1 116g 3 84g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(8, patterns[0].RightnessPoints - patterns[1].RightnessPoints, "10-2 so should be 8 points");
        }

        [Test]
        public void IfThereIsNoInsulinForFoundMealThenSkipsThatMeal()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            AddMeal("07:00 1 100g");
            AddSugars("07:00 100 08:00 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.IsEmpty(patterns);
        }

        [Test]
        public void IfThereIsInsulinForFoundMealThenReturnsThatMealWithInsulin()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            var mealToFind = AddMeal("07:00 1 100g");
            var insulinToFind = AddInsulin("07:00 1");
            AddSugars("07:00 100 08:00 100");
            var pattern = sut.GetPatternsFor(insulin, meal).Single();
            Assert.AreSame(mealToFind, pattern.From);
            Assert.AreSame(insulinToFind, pattern.Insulin);
        }

        [Test]
        public void IfSugarBeforeCannotBeFoundThenMealIsNotReturned()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            AddMeal("07:00 1 100g");
            AddInsulin("07:00 1");
            AddSugars("08:00 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.IsEmpty(patterns);
        }

        [Test]
        public void IfSugarBeforeCanBeFoundThenMealIsReturnedWithThatSugar()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            var mealToFind = AddMeal("07:00 1 100g");
            AddInsulin("07:00 1");
            var sugarToFind = AddSugars("07:00 100 08:00 100").First();
            var pattern = sut.GetPatternsFor(insulin, meal).Single();
            Assert.AreSame(mealToFind, pattern.From);
            Assert.AreSame(sugarToFind, pattern.Before);
        }

        [Test]
        public void IfSugarsAfterCannotBeFoundThenMealIsNotReturned()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            AddMealInsulinAndSugars("07:00 1 100g", "1", "100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.IsEmpty(patterns);
        }

        [Test]
        public void IfSugarsAfterCanBeFoundThenMealIsReturnedWithThoseSugars()
        {
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            var mealToFind = AddMealInsulinAndSugars("07:00 1 100g", "1", "100");
            var sugarsToFind = AddSugars("07:30 120 08:30 125");
            var pattern = sut.GetPatternsFor(insulin, meal).Single();
            Assert.AreSame(mealToFind, pattern.From);
            Assert.AreEqual(sugarsToFind, pattern.After);
        }

        [Test]
        public void SettingIsUsedToDetermineTimeWindowForSugarsAfter()
        {
            factories.Settings.SugarsAfterInsulinHours = 1;
            var sut = CreateSut();
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            AddMealInsulinAndSugars("07:00 1 100g", "1", "100");
            var sugarsToFind = AddSugars("08:00 120");
            AddSugars("09:00 120");
            var pattern = sut.GetPatternsFor(insulin, meal).Single();
            Assert.AreEqual(sugarsToFind, pattern.After, "Only sugars in 1 hours after should be returned");
        }

        [TestCase(360, 361, 1, Description = "In 360 days = 1 extra point")]
        [TestCase(180, 181, 1, Description = "In 180 days = 1 extra point")]
        [TestCase(90, 91, 1, Description = "In 90 days = 1 extra point")]
        [TestCase(60, 61, 1, Description = "In 60 days = 1 extra point")]
        [TestCase(30, 31, 1, Description = "In 30 days = 1 extra point")]
        [TestCase(15, 16, 1, Description = "In 15 days = 1 extra point")]
        [TestCase(7, 8, 1, Description = "In 7 days = 1 extra point")]
        [TestCase(2, 3, 1, Description = "In 2 days = 1 extra point")]
        [TestCase(1, 2.1, 1, Description = "In 2 days = 1 extra point (by 0.1 day)")]
        public void RecentMealGetsMoreRightnessPoints(double addDays1, double addDays2, int expectedPointsDifference)
        {
            var sut = CreateSut(new PatternBuilderImpl.PointsForRecentMeal());
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            basedate = basedate.AddDays(addDays1);
            AddMealInsulinAndSugars("12:00 1 100g", "1", "100 100");
            basedate = basedate.AddDays(-addDays1).AddDays(addDays2);
            AddMealInsulinAndSugars("12:00 1 100g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(expectedPointsDifference, patterns[0].RightnessPoints - patterns[1].RightnessPoints);
        }

        [TestCase("17:00", "05:00", 00, Description = "12:00 difference = 00 points")]
        [TestCase("17:00", "17:30", 12, Description = "00:30 difference = 12 points")]
        [TestCase("17:00", "17:31", 11, Description = "00:31 difference = 11 points")]
        [TestCase("09:00", "15:00", 06, Description = "06:00 difference = 06 points")]
        [TestCase("14:00", "06:00", 04, Description = "08:00 difference = 04 points")]
        [TestCase("08:31", "10:01", 11, Description = "01:30 difference = 01 points")]
        [TestCase("07:10", "08:00", 11, Description = "00:50 difference = 11 points")]
        [TestCase("12:00", "00:00", 00, Description = "12:00 difference = 00 points")]
        [TestCase("12:00", "01:00", 01, Description = "11:00 difference = 01 points")]
        [TestCase("00:00", "23:00", 11, Description = "01:00 difference = 11 points")]
        [TestCase("01:00", "22:00", 09, Description = "03:00 difference = 09 points")]
        [TestCase("01:30", "22:31", 09, Description = "03:00 difference = 09 points")]
        public void MealAtSimillarHourGetsMoreRightnessPoints(string idealHour, string foundHour, int expectedPoints)
        {
            var sut = CreateSut(new PatternBuilderImpl.PointsForSimillarHour(new HourDifferenceImpl()));
            var insulin = AddInsulin(idealHour + " 1");
            var meal = AddMeal(idealHour + " 1 100g");
            basedate = basedate.AddDays(1); // To avoid same date time of searched and found
            AddMealInsulinAndSugars(foundHour + " 1 100g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(expectedPoints, patterns[0].RightnessPoints);
        }

        [TestCase("", 0)]
        [TestCase("1 2 3", 15)]
        [TestCase("3", 5)]
        [TestCase("10", 0)]
        [TestCase("4 5 6", 0)]
        [TestCase("4 5 6 1", 5)]
        [TestCase("2 1", 10)]
        public void InsulinWithSameCircumstancesGetsMoreRightessPoints(string circumstances, int expectedPoints)
        {
            var sut = CreateSut(new PatternBuilderImpl.PointsForSameCircumstances());
            var insulin = AddInsulin("12:00 1 0 0 1 2 3");
            var meal = AddMeal("12:00 1 100g");
            AddMealInsulinAndSugars("07:00 1 100g", ("1 0 0 " + circumstances).Trim(), "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(expectedPoints, patterns[0].RightnessPoints);
        }

        [TestCase(100, 100, SugarUnit.mgdL, 5)]
        [TestCase(100, 150, SugarUnit.mgdL, 0)]
        [TestCase(100, 144, SugarUnit.mgdL, 1)]
        [TestCase(100, 145, SugarUnit.mgdL, 0)]
        [TestCase(100, 104, SugarUnit.mgdL, 5)]
        [TestCase(100, 105, SugarUnit.mgdL, 4)]
        [TestCase(100, 160, SugarUnit.mgdL, 0)]
        [TestCase(100, 94, SugarUnit.mgdL, 4)]
        [TestCase(100, 95, SugarUnit.mgdL, 4)]
        [TestCase(100, 96, SugarUnit.mgdL, 5)]
        [TestCase(140, 70, SugarUnit.mgdL, 0)]
        [TestCase(140, 120, SugarUnit.mgdL, 3)]
        [TestCase(140, 96, SugarUnit.mgdL, 1)]
        [TestCase(6, 5, SugarUnit.mmolL, 3)]
        [TestCase(6, 8, SugarUnit.mmolL, 1)]
        public void MealWithSimillarSugarBeforeGetsMoreRightnessPoints(int currentSugar, int sugarToFind,
            SugarUnit unit, int expectedPoints)
        {
            factories.Settings.SugarUnit = unit;
            var sut = CreateSut(new PatternBuilderImpl.PointsForSimillarSugarBefore());
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 1 100g");
            AddSugars("12:00 " + currentSugar.ToString());
            AddMealInsulinAndSugars("09:00 1 100g", "1", sugarToFind.ToString() + " 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(expectedPoints, patterns[0].RightnessPoints);
        }

        [TestCase("1 100g 2 200g", "1 100g 2 200g", 1, 1)]
        [TestCase("1 100g", "1 50g", 2)]
        [TestCase("1 0g", "1 0g", 0)]
        [TestCase("1 100g 2 200g", "1 110g 2 190g", 100f / 110f, 200f / 190f)]
        public void CalculatesFactor(string currentMeal, string mealToFind, params float[] expectedFactors)
        {
            var sut = CreateSut(new PatternBuilderImpl.Factor());
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 " + currentMeal);
            AddMealInsulinAndSugars("10:00 " + mealToFind, "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(expectedFactors, patterns.Select(pattern => pattern.Factor).ToArray());
        }

        [TestCase("1 100g", "1 100g", 5)]
        [TestCase("1 100g", "1 50g", 2)]
        [TestCase("1 100g", "1 200g", 2)]
        [TestCase("1 100g", "1 10g", 0)]
        [TestCase("1 100g", "1 20g", 1)]
        [TestCase("1 100g", "1 90g", 4)]
        [TestCase("1 50g", "1 100g", 2)]
        [TestCase("1 200g", "1 100g", 2)]
        [TestCase("1 10g", "1 100g", 0)]
        [TestCase("1 20g", "1 100g", 1)]
        [TestCase("1 90g", "1 100g", 4)]
        public void FactorCloserToOneGivesMoreRighnessPoints(string currentMeal, string mealToFind, int expectedPoints)
        {
            var sut = CreateSut(new PatternBuilderImpl.Factor(),
                new PatternBuilderImpl.PointsForFactorCloserToOne());
            var insulin = AddInsulin("12:00 1");
            var meal = AddMeal("12:00 " + currentMeal);
            AddMealInsulinAndSugars("09:00 " + mealToFind, "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin, meal);
            Assert.AreEqual(expectedPoints, patterns[0].RightnessPoints);
        }
    }
}
