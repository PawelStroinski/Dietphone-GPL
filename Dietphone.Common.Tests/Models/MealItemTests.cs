using System;
using NUnit.Framework;

namespace Dietphone.Models.Tests
{
    public class MealItemTests
    {
        [Test]
        public void PercentOfEnergyInMeal()
        {
            var factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            var meal1 = factories.CreateMeal();
            var meal2 = factories.CreateMeal();
            var product1 = factories.CreateProduct();
            product1.EnergyPer100g = 100;
            var product2 = factories.CreateProduct();
            product2.EnergyPer100g = 200;
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
        }
    }
}
