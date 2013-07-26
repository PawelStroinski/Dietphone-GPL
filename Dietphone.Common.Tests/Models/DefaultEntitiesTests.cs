using System;
using NUnit.Framework;

namespace Dietphone.Models.Tests
{
    public class DefaultEntitiesTests
    {
        private DefaultEntities defaultEntities;
            
        [SetUp]
        public void TestInitialize()
        {
            var factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            defaultEntities = new DefaultEntitiesImpl(factories);
        }

        [Test]
        public void Creates_Correct_MealName()
        {
            var mealName = defaultEntities.MealName;
            Assert.AreEqual(Guid.Empty, mealName.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(mealName.Name));
        }

        [Test]
        public void Creates_Correct_Product()
        {
            var product = defaultEntities.Product;
            Assert.AreEqual(Guid.Empty, product.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(product.Name));
        }

        [Test]
        public void Creates_Correct_InsulinCircumstance()
        {
            var insulinCircumstance = defaultEntities.InsulinCircumstance;
            Assert.AreEqual(Guid.Empty, insulinCircumstance.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(insulinCircumstance.Name));
        }

        [Test]
        public void Creates_MealName_One_Time()
        {
            var mealName1 = defaultEntities.MealName;
            var mealName2 = defaultEntities.MealName;
            Assert.IsNotNull(mealName1);
            Assert.AreSame(mealName1, mealName2);
        }

        [Test]
        public void Creates_Product_One_Time()
        {
            var product1 = defaultEntities.Product;
            var product2 = defaultEntities.Product;
            Assert.IsNotNull(product1);
            Assert.AreSame(product1, product2);
        }

        [Test]
        public void Creates_InsulinCircumstance_One_Time()
        {
            var circumstance1 = defaultEntities.InsulinCircumstance;
            var circumstance2 = defaultEntities.InsulinCircumstance;
            Assert.IsNotNull(circumstance1);
            Assert.AreSame(circumstance1, circumstance2);
        }
    }
}
