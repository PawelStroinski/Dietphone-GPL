using System;
using NUnit.Framework;
using System.Linq;

namespace Dietphone.Models.Tests
{
    public class MealItemTests
    {
        [Test]
        public void PercentOfEnergyCuFpuInMeal()
        {
            var factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            var meal1 = factories.CreateMeal();
            var meal2 = factories.CreateMeal();
            var product1 = factories.CreateProduct();
            product1.EnergyPer100g = 100;
            product1.CarbsTotalPer100g = 200;
            product1.FatPer100g = 300;
            product1.ProteinPer100g = 400;
            var product2 = factories.CreateProduct();
            product2.EnergyPer100g = 200;
            product2.CarbsTotalPer100g = 100;
            product2.FatPer100g = 50;
            product2.ProteinPer100g = 25;
            var item1 = meal1.AddItem();
            item1.ProductId = product1.Id;
            item1.Value = 10;
            var item2 = meal1.AddItem();
            item2.ProductId = product2.Id;
            item2.Value = 25;
            var item3 = meal2.AddItem();
            Assert.AreEqual(17, item1.PercentOfEnergyInMeal(meal1));
            Assert.AreEqual(83, item2.PercentOfEnergyInMeal(meal1));
            Assert.AreEqual(0, item3.PercentOfEnergyInMeal(meal2));
            Assert.AreEqual(44, item1.PercentOfCuInMeal(meal1));
            Assert.AreEqual(56, item2.PercentOfCuInMeal(meal1));
            Assert.AreEqual(0, item3.PercentOfCuInMeal(meal2));
            Assert.AreEqual(75, item1.PercentOfFpuInMeal(meal1));
            Assert.AreEqual(25, item2.PercentOfFpuInMeal(meal1));
            Assert.AreEqual(0, item3.PercentOfFpuInMeal(meal2));
        }

        public class NutrientsTests : MealItemTests
        {
            private Product product;
            private MealItem item;
            private Func<float[]> actual;

            private void SetPer100g(Product product, short energy, float protein, float fat, float carb)
            {
                product.EnergyPer100g = energy;
                product.ProteinPer100g = protein;
                product.FatPer100g = fat;
                product.CarbsTotalPer100g = carb;
            }

            private void SetPerServing(Product product, short energy, float protein, float fat, float carb)
            {
                product.EnergyPerServing = energy;
                product.ProteinPerServing = protein;
                product.FatPerServing = fat;
                product.CarbsTotalPerServing = carb;
            }

            private float[] Round(float[] numbers)
            {
                return numbers.Select(number => (float)Math.Round(number, 0)).ToArray();
            }

            [SetUp]
            public void TestInitialize()
            {
                var factories = new FactoriesImpl();
                factories.StorageCreator = new StorageCreatorStub();
                product = factories.CreateProduct();
                SetPer100g(product, energy: 100, protein: 200, fat: 300, carb: 400);
                product.ServingSizeValue = 500;
                SetPerServing(product, energy: 600, protein: 700, fat: 800, carb: 900);
                var meal = factories.CreateMeal();
                item = meal.AddItem();
                item.ProductId = product.Id;
                item.Value = 10;
                actual = () => new[] { item.Energy, item.Protein, item.Fat, item.DigestibleCarbs };
            }

            [Test]
            public void Nutrients()
            {
                product.ServingSizeUnit = Unit.Mililiter;
                Assert.AreEqual(new[] { 10, 20, 30, 40 }, actual());
                item.Unit = Unit.Mililiter;
                Assert.AreEqual(new[] { 12, 14, 16, 18 }, actual());
                item.Unit = Unit.ServingSize;
                Assert.AreEqual(new[] { 6000, 7000, 8000, 9000 }, actual());
                item.Unit = Unit.Gram;
                product.ServingSizeUnit = Unit.Gram;
                SetPer100g(product, energy: 0, protein: 0, fat: 0, carb: 0);
                Assert.AreEqual(new[] { 12, 14, 16, 18 }, actual());
            }

            [Test]
            public void NutrientsWhenServingSizeIsInGrams()
            {
                product.ServingSizeUnit = Unit.Gram;
                item.Unit = Unit.ServingSize;
                Assert.AreEqual(new[] { 6000, 7000, 8000, 9000 }, actual());
                SetPerServing(product, energy: 0, protein: 0, fat: 0, carb: 0);
                Assert.AreEqual(new[] { 5000, 10000, 15000, 20000 }, actual());
            }

            [Test]
            public void Ounce()
            {
                product.ServingSizeUnit = Unit.Gram;
                item.Unit = Unit.Ounce;
                Assert.AreEqual(new[] { 283, 567, 850, 1134 }, Round(actual()));
                SetPer100g(product, energy: 0, protein: 0, fat: 0, carb: 0);
                Assert.AreEqual(new[] { 340, 397, 454, 510 }, Round(actual()));
                product.ServingSizeUnit = Unit.Ounce;
                product.ServingSizeValue = 50;
                item.Unit = Unit.Gram;
                var fiveOuncesInGrams = 142;
                item.Value = fiveOuncesInGrams;
                Assert.AreEqual(new[] { 60, 70, 80, 90 }, Round(actual()));
            }

            [Test]
            public void NutrientsWhenServingSizeIsInOunces()
            {
                product.ServingSizeUnit = Unit.Ounce;
                item.Unit = Unit.ServingSize;
                Assert.AreEqual(new[] { 6000, 7000, 8000, 9000 }, actual());
                SetPerServing(product, energy: 0, protein: 0, fat: 0, carb: 0);
                product.ServingSizeValue = 5;
                Assert.AreEqual(new[] { 1417, 2835, 4252, 5670 }, Round(actual()));
            }

            [Test]
            public void ConvertToPounds()
            {
                SetPer100g(product, energy: 0, protein: 0, fat: 0, carb: 0);
                item.Value = 0.5f;
                item.Unit = Unit.Pound;
                product.ServingSizeValue = 2;
                product.ServingSizeUnit = Unit.Ounce;
                Assert.AreEqual(new[] { 2400, 2800, 3200, 3600 }, actual());
                product.ServingSizeValue = 150;
                product.ServingSizeUnit = Unit.Gram;
                Assert.AreEqual(new[] { 907, 1058, 1210, 1361 }, Round(actual()));
            }

            [Test]
            public void ConvertFromPounds()
            {
                SetPer100g(product, energy: 0, protein: 0, fat: 0, carb: 0);
                product.ServingSizeValue = 0.5f;
                product.ServingSizeUnit = Unit.Pound;
                item.Value = 2;
                item.Unit = Unit.Ounce;
                Assert.AreEqual(new[] { 150, 175, 200, 225 }, actual());
                item.Value = 150;
                item.Unit = Unit.Gram;
                Assert.AreEqual(new[] { 397, 463, 529, 595 }, Round(actual()));
            }
        }
    }
}
