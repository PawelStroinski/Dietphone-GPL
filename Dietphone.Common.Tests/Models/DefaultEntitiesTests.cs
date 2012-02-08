using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dietphone.Models.Tests
{
    [TestClass]
    public class DefaultEntitiesTests
    {
        private DefaultEntities defaultEntities;
            
        [TestInitialize()]
        public void TestInitialize()
        {
            var factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            defaultEntities = new DefaultEntitiesImpl(factories);
        }

        [TestMethod]
        public void Creates_Correct_MealName()
        {
            var mealName = defaultEntities.MealName;
            Assert.AreEqual(Guid.Empty, mealName.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(mealName.Name));
        }

        [TestMethod]
        public void Creates_Correct_Product()
        {
            var product = defaultEntities.Product;
            Assert.AreEqual(Guid.Empty, product.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(product.Name));
        }

        [TestMethod]
        public void Creates_MealName_One_Time()
        {
            var mealName1 = defaultEntities.MealName;
            var mealName2 = defaultEntities.MealName;
            Assert.IsNotNull(mealName1);
            Assert.AreSame(mealName1, mealName2);
        }

        [TestMethod]
        public void Creates_Product_One_Time()
        {
            var product1 = defaultEntities.Product;
            var product2 = defaultEntities.Product;
            Assert.IsNotNull(product1);
            Assert.AreSame(product1, product2);
        }
    }
}
