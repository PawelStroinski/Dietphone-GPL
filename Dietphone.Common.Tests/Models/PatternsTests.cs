using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text.RegularExpressions;

namespace Dietphone.Models.Tests
{
    [TestClass]
    public class PatternsTests
    {
        private Factories factories;
        private static readonly DateTime basedate = new DateTime(2013, 07, 24);

        [TestInitialize]
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

        private Meal AddMeal(string hourAndProductIdsAndValues) // e.g. "12:00 1 100g 2 100g"
        {
            var splet = hourAndProductIdsAndValues.Split(" ".ToArray());
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

        private Insulin AddInsulin(string hour, byte normalBolus, byte squareWaveBolus, byte squareWaveBolusHours,
            params byte[] circumstances)
        {
            var insulin = factories.CreateInsulin();
            insulin.DateTime = basedate + TimeSpan.Parse(hour);
            insulin.NormalBolus = normalBolus;
            insulin.SquareWaveBolus = squareWaveBolus;
            insulin.SquareWaveBolusHours = squareWaveBolusHours;
            insulin.InitializeCircumstances(circumstances.Select(c => c.ToGuid()).ToList());
            return insulin;
        }

        private Sugar AddSugar(string hour, byte bloodSugar)
        {
            var sugar = factories.CreateSugar();
            sugar.DateTime = basedate + TimeSpan.Parse(hour);
            sugar.BloodSugar = bloodSugar;
            return sugar;
        }

        [TestMethod]
        public void IfNoMealForInsulinThenReturnsEmpty()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00", 1, 0, 0);
            var patterns = sut.GetPatternsFor(insulin);
            Assert.AreEqual(0, patterns.Count());
        }

        [TestMethod]
        public void IfNoMatchingItemThenReturnsEmpty()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00", 1, 0, 0);
            AddMeal("12:00 1 100g");
            AddMeal("11:00 2 100g");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.AreEqual(0, patterns.Count());
        }

        [TestMethod]
        public void IfMatchingItemThenReturnsIt()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00", 1, 0, 0);
            AddMeal("12:00 1 100g");
            var mealToFind = AddMeal("11:00 1 100g");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.AreSame(mealToFind.Items[0], patterns.Single().Match);
            Assert.AreSame(mealToFind, patterns.Single().From);
        }

        [TestMethod]
        public void ReturnsMatchesForEveryItemInMeal()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00", 1, 0, 0);
            AddMeal("12:00 1 100g 2 200g");
            var mealToFind = AddMeal("11:00 1 100g 2 200g");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.AreEqual(2, patterns.Count);
            Assert.AreSame(mealToFind.Items[0], patterns[0].Match);
            Assert.AreSame(mealToFind.Items[1], patterns[1].Match);
            Assert.AreSame(mealToFind, patterns[0].From);
            Assert.AreSame(mealToFind, patterns[1].From);
        }

        [TestMethod]
        public void ReturnsOnlyItemsHavingSimillarPercentageOfEnergyInMeal()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00", 1, 0, 0);
            AddMeal("12:00 1 100g 2 100g");
            var mealToFind1 = AddMeal("07:00 1 1000g 3 1000g");
            var mealNotFind = AddMeal("08:00 1 160g 3 100g");
            var mealToFind2 = AddMeal("09:00 1 150g 3 100g");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.IsTrue(patterns.Any(p => p.From == mealToFind1), "Same percentage so should match");
            Assert.IsFalse(patterns.Any(p => p.From == mealNotFind), "Percentage different by 11 so should fail");
            Assert.IsTrue(patterns.Any(p => p.From == mealToFind2), "Percentage different by no more than 10 so matches");
        }

        [TestMethod]
        public void MoreSimillarPercentageOfEnergyInMealGivesMoreRightnessPoints()
        {
            var sut = new PatternsImpl(factories);
            var insulin = AddInsulin("12:00", 1, 0, 0);
            AddMeal("12:00 1 100g 2 100g");
            AddMeal("07:00 1 100g 3 100g");
            AddMeal("07:00 1 116g 3 84g");
            var patterns = sut.GetPatternsFor(insulin);
            Assert.AreEqual(8, patterns[0].RightnessPoints - patterns[1].RightnessPoints, "10-2 so should be 8 points");
        }

        [TestMethod]
        public void IfSugarBeforeCannotBeFoundThenMealIsNotReturned()
        {
            
        }
    }
}
