using System;
using System.Linq;
using System.Collections.Generic;
using Dietphone.Models;
using NSubstitute;
using NUnit.Framework;

namespace Dietphone.Common.Tests.Models
{
    public class ExportAndImportTests
    {
        private Factories factories;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            factories.Finder.Returns(new FinderImpl(factories));
            SetupMeals();
            SetupMealNames();
            SetupProducts();
            SetupCategories();
            SetupSugars();
            SetupInsulins();
            factories.Settings.Returns(new Settings());
        }

        private void SetupMeals()
        {
            var meal = new Meal();
            meal.SetOwner(factories);
            meal.Id = Guid.NewGuid();
            var mealItem = new MealItem();
            mealItem.SetOwner(factories);
            mealItem.Value = 100;
            meal.InitializeItems(new List<MealItem> { mealItem });
            factories.Meals.Returns(new List<Meal> { meal });
            factories.CreateMeal().Returns(_ => { factories.Meals.Add(meal); return meal; });
        }

        private void SetupMealNames()
        {
            var mealName = new MealName();
            mealName.SetOwner(factories);
            mealName.Id = Guid.NewGuid();
            factories.MealNames.Returns(new List<MealName> { mealName });
            factories.CreateMealName().Returns(_ => { factories.MealNames.Add(mealName); return mealName; });
        }

        private void SetupProducts()
        {
            var product = new Product();
            product.SetOwner(factories);
            product.Id = Guid.NewGuid();
            product.AddedByUser = true;
            factories.Products.Returns(new List<Product> { product });
            factories.CreateProduct().Returns(_ => { factories.Products.Add(product); return product; });
        }

        private void SetupCategories()
        {
            var category = new Category();
            category.SetOwner(factories);
            category.Id = Guid.NewGuid();
            factories.Categories.Returns(new List<Category> { category });
            factories.CreateCategory().Returns(_ => { factories.Categories.Add(category); return category; });
        }

        private void SetupSugars()
        {
            var sugar = new Sugar();
            sugar.SetOwner(factories);
            sugar.Id = Guid.NewGuid();
            factories.Sugars.Returns(new List<Sugar> { sugar });
            factories.CreateSugar().Returns(_ => { factories.Sugars.Add(sugar); return sugar; });
        }

        private void SetupInsulins()
        {
            var insulin = new Insulin();
            insulin.SetOwner(factories);
            insulin.Id = Guid.NewGuid();
            insulin.InitializeCircumstances(new List<Guid>());
            factories.Insulins.Returns(new List<Insulin> { insulin });
            factories.CreateInsulin().Returns(_ => { factories.Insulins.Add(insulin); return insulin; });
            var insulinCircumstance = new InsulinCircumstance();
            insulinCircumstance.SetOwner(factories);
            insulinCircumstance.Id = Guid.NewGuid();
            insulin.AddCircumstance(insulinCircumstance);
            factories.InsulinCircumstances.Returns(new List<InsulinCircumstance> { insulinCircumstance });
            factories.CreateInsulinCircumstance()
                .Returns(_ => { factories.InsulinCircumstances.Add(insulinCircumstance); return insulinCircumstance; });

        }

        [Test]
        public void ExportsAndImportsMealsAndMealNamesAndProductsAndCategories()
        {
            var meal = factories.Meals[0];
            var mealName = factories.MealNames[0];
            var product = factories.Products[0];
            var category = factories.Categories[0];
            var sut = new ExportAndImportImpl(factories);
            var data = sut.Export();
            factories.Meals.Clear();
            factories.MealNames.Clear();
            factories.Products.Clear();
            factories.Categories.Clear();
            var empty = new Meal();
            empty.InitializeItems(new List<MealItem>());
            meal.CopyItemsFrom(empty);
            Assert.IsEmpty(meal.Items);
            sut.Import(data);
            Assert.AreEqual(meal.Id, factories.Meals[0].Id);
            Assert.AreEqual(meal.Items[0].Value, factories.Meals[0].Items[0].Value);
            Assert.AreEqual(mealName.Id, factories.MealNames[0].Id);
            Assert.AreEqual(product.Id, factories.Products[0].Id);
            Assert.AreEqual(category.Id, factories.Categories[0].Id);
            sut.Import(data);
            Assert.AreEqual(1, factories.Meals.Count);
            Assert.AreEqual(1, factories.MealNames.Count);
            Assert.AreEqual(1, factories.Products.Count);
            Assert.AreEqual(1, factories.Categories.Count);
        }

        [Test]
        public void ExportsAndImportsSugarsAndInsulinsAndInsulinCircumstances()
        {
            var sugar = factories.Sugars[0];
            var insulin = factories.Insulins[0];
            var insulinCircumstance = factories.InsulinCircumstances[0];
            var sut = new ExportAndImportImpl(factories);
            var data = sut.Export();
            factories.Sugars.Clear();
            factories.Insulins.Clear();
            factories.InsulinCircumstances.Clear();
            var empty = new Insulin();
            empty.InitializeCircumstances(new List<Guid>());
            insulin.CopyCircumstancesFrom(empty);
            Assert.IsEmpty(insulin.Circumstances);
            sut.Import(data);
            Assert.AreEqual(sugar.Id, factories.Sugars[0].Id);
            Assert.AreEqual(insulin.Id, factories.Insulins[0].Id);
            Assert.AreSame(factories.InsulinCircumstances[0], factories.Insulins[0].Circumstances.Single());
            Assert.AreEqual(insulinCircumstance.Id, factories.InsulinCircumstances[0].Id);
            sut.Import(data);
            Assert.AreEqual(1, factories.Sugars.Count);
            Assert.AreEqual(1, factories.Insulins.Count);
            Assert.AreEqual(1, factories.InsulinCircumstances.Count);
        }

        [Test]
        public void ImportsDefaultSettingWhenSettingNotExported()
        {
            factories.Settings.SugarsAfterInsulinHours = 0;
            var sut = new ExportAndImportImpl(factories);
            var data = sut.Export();
            var removeThis = "<SugarsAfterInsulinHours>0</SugarsAfterInsulinHours>";
            Assert.IsTrue(data.Contains(removeThis), "This is requirement to perform test");
            data = data.Replace(removeThis, string.Empty);
            sut.Import(data);
            Assert.AreNotEqual(0, factories.Settings.SugarsAfterInsulinHours);
        }
    }
}
