using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Moq;
using System.Text.RegularExpressions;

namespace Dietphone.Models.Tests
{
    public class PatternsTests
    {
        private Factories factories;
        private static readonly DateTime basedate = new DateTime(2013, 07, 24);

        [SetUp]
        public void InitializeOwner()
        {
            factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            AddProduct(1, energy: 100, carbs: 100, protein: 100, fat: 100);
            AddProduct(2, energy: 100, carbs: 100, protein: 100, fat: 100);
            AddProduct(3, energy: 100, carbs: 100, protein: 100, fat: 100);
        }

        private Product AddProduct(byte productId, byte energy, byte carbs, byte protein, byte fat)
        {
            var product = factories.CreateProduct();
            product.Id = productId.ToGuid();
            product.EnergyPer100g = energy;
            product.CarbsTotalPer100g = carbs;
            product.ProteinPer100g = protein;
            product.FatPer100g = fat;
            return product;
        }

        private Meal AddMealInsulinAndSugars(string meal, string insulinWithoutHour,
            string sugarsBeforeAndAfterWithoutHour)
            // e.g. ("12:00", "1", "100 120 140") -> ("12:00", "12:00 1", "12:00 100" "13:00 120" "14:00 140")
        {
            var addedMeal = AddMeal(meal);
            var mealHour = meal.Split(' ').First();
            AddInsulin(mealHour + " " + insulinWithoutHour);
            var sugarsSplet = sugarsBeforeAndAfterWithoutHour.Split(' ');
            for (int i = 0; i < sugarsSplet.Count(); i += 1)
                AddSugar((TimeSpan.Parse(mealHour) + TimeSpan.FromHours(i)).ToString() + " " + sugarsSplet[i]);
            return addedMeal;
        }

        private Meal AddMeal(string hourAndProductIdsAndValues) // e.g. "12:00 1 100g 2 100g"
        {
            var splet = hourAndProductIdsAndValues.Split(' ');
            var meal = factories.CreateMeal();
            meal.DateTime = basedate + TimeSpan.Parse(splet[0]);
            for (int i = 1; i < splet.Count(); i += 2)
            {
                var item = meal.AddItem();
                item.ProductId = splet[i].ToGuid();
                var match = Regex.Match(splet[i + 1], @"(\d*)(\D*)");
                var value = match.Groups[1].Value;
                var unit = match.Groups[2].Value;
                item.Value = int.Parse(value);
                item.Unit = item.Unit.TryGetValueOfAbbreviation(unit);
            }
            return meal;
        }

        private Insulin AddInsulin(string hourAndNormalBolusAndSquareWaveBolusAndSquareWaveBolusHoursAndCircumstances)
            // e.g. "12:00 1", "12:00 1 0 0", "12:00 2 2 3 1 2 3"
        {
            var splet = hourAndNormalBolusAndSquareWaveBolusAndSquareWaveBolusHoursAndCircumstances.Split(' ');
            var insulin = factories.CreateInsulin();
            insulin.DateTime = basedate + TimeSpan.Parse(splet[0]);
            insulin.NormalBolus = int.Parse(splet[1]);
            if (splet.Length > 2)
            {
                insulin.SquareWaveBolus = int.Parse(splet[2]);
                insulin.SquareWaveBolusHours = int.Parse(splet[3]);
                insulin.InitializeCircumstances(splet.Skip(4).Select(s => byte.Parse(s).ToGuid()).ToList());
            }
            return insulin;
        }

        private Sugar AddSugar(string hourAndBloodSugar) // e.g. "12:00 100"
        {
            var splet = hourAndBloodSugar.Split(' ');
            var sugar = factories.CreateSugar();
            sugar.DateTime = basedate + TimeSpan.Parse(splet[0]);
            sugar.BloodSugar = int.Parse(splet[1]);
            return sugar;
        }

        [Test]
        public void IfNoMealForQueredInsulinThenReturnsEmpty()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00 1");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.AreEqual(0, patterns.Count());
        }

        [Test]
        public void IfNoMatchingItemThenReturnsEmpty()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00 1");
            AddMeal("12:00 1 100g");
            AddMealInsulinAndSugars("11:00 2 100g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.AreEqual(0, patterns.Count());
        }

        [Test]
        public void IfMatchingItemThenReturnsIt()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00 1");
            AddMeal("12:00 1 100g");
            var mealToFind = AddMealInsulinAndSugars("11:00 1 100g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.AreSame(mealToFind.Items[0], patterns.Single().Match);
            Assert.AreSame(mealToFind, patterns.Single().From);
        }

        [Test]
        public void ReturnsMatchesForEveryItemInMeal()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00 1");
            AddMeal("12:00 1 100g 2 200g");
            var mealToFind = AddMealInsulinAndSugars("11:00 1 100g 2 200g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.AreEqual(2, patterns.Count);
            Assert.AreSame(mealToFind.Items[0], patterns[0].Match);
            Assert.AreSame(mealToFind.Items[1], patterns[1].Match);
            Assert.AreSame(mealToFind, patterns[0].From);
            Assert.AreSame(mealToFind, patterns[1].From);
        }

        [Test]
        public void ReturnsOnlyItemsHavingSimillarPercentageOfEnergyInMeal()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00 1");
            AddMeal("12:00 1 100g 2 100g");
            var mealToFind1 = AddMealInsulinAndSugars("07:00 1 1000g 3 1000g", "1", "100 100");
            var mealNotFind = AddMealInsulinAndSugars("08:00 1 160g 3 100g", "1", "100 100");
            var mealToFind2 = AddMealInsulinAndSugars("09:00 1 150g 3 100g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.IsTrue(patterns.Any(p => p.From == mealToFind1), "Same percentage so should match");
            Assert.IsFalse(patterns.Any(p => p.From == mealNotFind), "Percentage different by 11 so should fail");
            Assert.IsTrue(patterns.Any(p => p.From == mealToFind2), "Percentage different by no more than 10 so matches");
        }

        [Test]
        public void MoreSimillarPercentageOfEnergyInMealGivesMoreRightnessPoints()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00 1");
            AddMeal("12:00 1 100g 2 100g");
            AddMealInsulinAndSugars("07:00 1 100g 3 100g", "1", "100 100");
            AddMealInsulinAndSugars("07:00 1 116g 3 84g", "1", "100 100");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.AreEqual(8, patterns[0].RightnessPoints - patterns[1].RightnessPoints, "10-2 so should be 8 points");
        }

        [Test]
        public void IfThereIsNoInsulinForFoundMealThenSkipsThatMeal()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00 1");
            AddMeal("12:00 1 100g");
            AddMeal("07:00 1 100g");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.IsEmpty(patterns);
        }

        [Test]
        public void IfThereIsInsulinForFoundMealThenReturnsThatMealWithInsulin()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00 1");
            AddMeal("12:00 1 100g");
            var mealToFind = AddMeal("07:00 1 100g");
            var insulinToFind = AddInsulin("07:00 1");
            var pattern = sut.GetPatternsFor(insulin).Single();
            Assert.AreSame(mealToFind, pattern.From);
            Assert.AreSame(insulinToFind, pattern.Insulin);
        }

        [Test]
        public void IfSugarBeforeCannotBeFoundThenMealIsNotReturned()
        {
            
        }
    }
}
