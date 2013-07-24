using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dietphone.Models.Tests
{
    [TestClass]
    public class MealItemTests
    {
        [TestMethod]
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
    }
}
