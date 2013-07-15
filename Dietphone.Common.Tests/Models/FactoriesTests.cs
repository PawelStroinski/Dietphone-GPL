using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Dietphone.Models.Tests
{
    [TestClass]
    public class FactoriesTests
    {
        private Factories factories;

        [TestInitialize]
        public void TestInitialize()
        {
            factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
        }

        [ExpectedException(typeof(NullReferenceException))]
        [TestMethod]
        public void Requires_StorageCreator_To_Create_Enity()
        {
            var factories = new FactoriesImpl();
            factories.CreateMeal();
        }

        [ExpectedException(typeof(NullReferenceException))]
        [TestMethod]
        public void Requires_StorageCreator_To_Expose_Enities()
        {
            var factories = new FactoriesImpl();
            var dummy = factories.Meals;
        }

        [TestMethod]
        public void Has_Finder()
        {
            var finder = factories.Finder;
            Assert.IsInstanceOfType(finder, typeof(Finder));
        }

        [TestMethod]
        public void Has_DefaultEntities()
        {
            var defaultEntities = factories.DefaultEntities;
            Assert.IsInstanceOfType(defaultEntities, typeof(DefaultEntities));
        }

        [TestMethod]
        public void Exposes_Created_Meals()
        {
            var meals = factories.Meals;
            var meal = factories.CreateMeal();
            Assert.AreEqual(1, meals.Count);
            Assert.AreSame(meal, meals[0]);
        }

        [TestMethod]
        public void Exposes_Created_MealNames()
        {
            var mealNames = factories.MealNames;
            var mealName = factories.CreateMealName();
            Assert.AreEqual(1, mealNames.Count);
            Assert.AreSame(mealName, mealNames[0]);
        }

        [TestMethod]
        public void Exposes_Created_Products()
        {
            var products = factories.Products;
            var product = factories.CreateProduct();
            Assert.AreEqual(1, products.Count);
            Assert.AreSame(product, products[0]);
        }

        [TestMethod]
        public void Exposes_Created_Categories()
        {
            var categories = factories.Categories;
            var category = factories.CreateCategory();
            Assert.AreEqual(2, categories.Count);
            Assert.AreSame(category, categories[1]);
        }

        [TestMethod]
        public void Exposes_Created_Sugars()
        {
            var sugars = factories.Sugars;
            var sugar = factories.CreateSugar();
            Assert.AreEqual(1, sugars.Count);
            Assert.AreSame(sugar, sugars[0]);
        }

        [TestMethod]
        public void Exposes_Created_Insulins()
        {
            var insulins = factories.Insulins;
            var insulin = factories.CreateInsulin();
            Assert.AreEqual(1, insulins.Count);
            Assert.AreSame(insulin, insulins[0]);
        }

        [TestMethod]
        public void Exposes_Settings()
        {
            var settings = factories.Settings;
            Assert.IsInstanceOfType(settings, typeof(Settings));
        }

        [TestMethod]
        public void Creates_Correct_Meal()
        {
            var meal = factories.CreateMeal();
            Assert.IsInstanceOfType(meal, typeof(Meal));
            Assert.AreNotEqual(Guid.Empty, meal.Id);
            Assert.AreNotEqual(default(DateTime), meal.DateTime);
            Assert.AreEqual(DateTimeKind.Utc, meal.DateTime.Kind);
            Assert.IsNotNull(meal.Items);
        }

        [TestMethod]
        public void Creates_Correct_MealName()
        {
            var mealName = factories.CreateMealName();
            Assert.IsInstanceOfType(mealName, typeof(MealName));
            Assert.AreNotEqual(Guid.Empty, mealName.Id);
        }

        [TestMethod]
        public void Creates_MealItem()
        {
            var mealItem = factories.CreateMealItem();
            Assert.IsInstanceOfType(mealItem, typeof(MealItem));
        }

        [TestMethod]
        public void Creates_Correct_Product()
        {
            var category = factories.Categories[0];
            category.Id = Guid.NewGuid();
            var product = factories.CreateProduct();
            Assert.IsInstanceOfType(product, typeof(Product));
            Assert.AreNotEqual(Guid.Empty, product.Id);
            Assert.AreEqual(category.Id, product.CategoryId);
        }

        [TestMethod]
        public void Creates_Correct_Category()
        {
            var category = factories.CreateCategory();
            Assert.IsInstanceOfType(category, typeof(Category));
            Assert.AreNotEqual(Guid.Empty, category.Id);
        }

        [TestMethod]
        public void Creates_Correct_Sugar()
        {
            var sugar = factories.CreateSugar();
            Assert.IsInstanceOfType(sugar, typeof(Sugar));
            Assert.AreNotEqual(Guid.Empty, sugar.Id);
            Assert.AreNotEqual(default(DateTime), sugar.DateTime);
            Assert.AreEqual(DateTimeKind.Utc, sugar.DateTime.Kind);
        }

        [TestMethod]
        public void Creates_Correct_Insulin()
        {
            var insulin = factories.CreateInsulin();
            Assert.IsInstanceOfType(insulin, typeof(Insulin));
            Assert.AreNotEqual(Guid.Empty, insulin.Id);
            Assert.AreNotEqual(default(DateTime), insulin.DateTime);
            Assert.AreEqual(DateTimeKind.Utc, insulin.DateTime.Kind);
            Assert.AreEqual(string.Empty, insulin.Note);
        }

        [TestMethod]
        public void Saves_Everything()
        {
            var factories = new FactoriesImpl();
            var stub = new SaveStorageCreatorStub();
            factories.StorageCreator = stub;
            factories.CreateCategory();
            factories.CreateMeal();
            factories.CreateMealName();
            factories.CreateProduct();
            factories.CreateSugar();
            factories.CreateInsulin();
            var dummy = factories.Settings;
            factories.Save();
            stub.Verify();
        }
    }

    public class SaveStorageCreatorStub : StorageCreator
    {
        public string CultureName { get; set; }
        private List<Action> verificationActions = new List<Action>();

        public Storage<T> CreateStorage<T>() where T : Entity, new()
        {
            var mock = new Mock<Storage<T>>();
            var entities = new List<T>();
            entities.Add(new T());
            mock.Setup(m => m.Load()).Returns(entities);
            mock.Setup(m => m.Save(It.IsAny<List<T>>())).Verifiable();
            verificationActions.Add(() =>
            {
                mock.Verify();
            });
            return mock.Object;
        }

        public void Verify()
        {
            verificationActions.ForEach(a => a.Invoke());
        }
    }
}
