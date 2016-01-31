using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using System.Collections.Generic;
using Ploeh.AutoFixture;
using System.Linq;
using System;

namespace Dietphone.Smartphone.Tests
{
    public class MealViewModelTests
    {
        private Factories factories;
        private Meal meal;
        private MealViewModel sut;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            meal = new Meal();
            sut = new MealViewModel(meal, factories);
            meal.SetOwner(factories);
            meal.InitializeItems(new List<MealItem>());
            factories.CreateMealItem().Returns(_ =>
            {
                var mealItem = new MealItem();
                mealItem.SetOwner(factories);
                return mealItem;
            });
            var fixture = new Fixture();
            factories.Products.Returns(fixture.CreateMany<Product>(10).ToList());
            factories.Finder.Returns(new FinderImpl(factories));
        }

        private void AddFiveItems()
        {
            sut.Items.Clear();
            for (int i = 0; i < 5; i++)
                sut.AddItem().ProductId = factories.Products[i].Id;
        }

        [Test]
        public void TextAndText2()
        {
            Assert.IsEmpty(sut.Text);
            Assert.IsEmpty(sut.Text2);
        }

        [Test]
        public void ChangingTheItemsInvalidatesTheScores()
        {
            sut.Scores.ChangesProperty(string.Empty, () => sut.AddItem());
            var item = sut.Items[0];
            sut.Scores.ChangesProperty(string.Empty, () => item.Value = "100");
        }

        [Test]
        public void ProductsHead()
        {
            AddFiveItems();
            Assert.AreEqual(
                factories.Products[0].Name + " | "
                + factories.Products[1].Name + " | "
                + factories.Products[2].Name,
                sut.ProductsHead);
        }

        [Test]
        public void Products()
        {
            AddFiveItems();
            Assert.AreEqual(
                factories.Products[0].Name + " | "
                + factories.Products[1].Name + " | "
                + factories.Products[2].Name + " | "
                + factories.Products[3].Name + " | "
                + factories.Products[4].Name,
                sut.Products);
        }

        [Test]
        public void HasName()
        {
            Assert.IsFalse(sut.HasName);
            sut = new MealViewModel(meal, factories);
            sut.Names = new[] { new MealNameViewModel(new MealName { Id = Guid.NewGuid() }, factories) };
            meal.NameId = sut.Names.First().Id;
            Assert.IsTrue(sut.HasName);
        }

        [Test]
        public void HasNote()
        {
            meal.Note = string.Empty;
            Assert.IsFalse(sut.HasNote);
            meal.Note = "note";
            Assert.IsTrue(sut.HasNote);
        }
    }
}
