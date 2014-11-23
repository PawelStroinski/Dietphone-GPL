using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using Ploeh.AutoFixture;
using System.Collections.Generic;

namespace Dietphone.Common.Phone.Tests
{
    public class MealViewModelTests
    {
        private Factories factories;
        private Fixture fixture;
        private Meal meal;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            fixture = new Fixture();
            meal = new Meal();
            meal.SetOwner(factories);
            meal.InitializeItems(new List<MealItem>());
            var product = fixture.Create<Product>();
            factories.Finder.FindProductById(product.Id).Returns(product);
            var item = new MealItem();
            item.SetOwner(factories);
            factories.CreateMealItem().Returns(item);
            meal.AddItem().ProductId = product.Id;
        }

        [Test]
        public void TextAndText2WithName()
        {
            var name = fixture.Create<MealName>();
            meal.NameId = name.Id;
            var sut = new MealViewModel(meal, factories);
            sut.Names = new List<MealNameViewModel> { new MealNameViewModel(name, factories) };
            Assert.AreEqual(name.Name, sut.Text);
            Assert.AreEqual(sut.ProductsHead, sut.Text2);
        }

        [Test]
        public void TextAndText2WithoutName()
        {
            var sut = new MealViewModel(meal, factories);
            Assert.AreEqual(sut.ProductsHead, sut.Text);
            Assert.IsEmpty(sut.Text2);
        }
    }
}
