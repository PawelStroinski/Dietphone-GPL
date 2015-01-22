using System;
using Dietphone.Models;
using Dietphone.Models.Tests;
using Dietphone.ViewModels;
using NUnit.Framework;

namespace Dietphone.Common.Phone.Tests
{
    public class MealItemViewModelTests
    {
        private Product product;
        private MealItemViewModel sut;

        [SetUp]
        public void TestInitialize()
        {
            var factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            product = factories.CreateProduct();
            product.EnergyPer100g = 100;
            product.EnergyPerServing = 100;
            var meal = factories.CreateMeal();
            var item = meal.AddItem();
            item.ProductId = product.Id;
            sut = new MealItemViewModel(item, factories);
        }

        [Test]
        public void AllUsableUnitsWithDetalis()
        {
            var gram = Unit.Gram.GetAbbreviation();
            var mililiter = Unit.Mililiter.GetAbbreviation();
            Func<string> servingSize = () => Unit.ServingSize.GetAbbreviationOrServingSizeDetalis(product);
            Assert.AreEqual(new[] { gram }, sut.AllUsableUnitsWithDetalis);
            product.ServingSizeValue = 1;
            Assert.AreEqual(new[] { gram, servingSize() }, sut.AllUsableUnitsWithDetalis);
            product.ServingSizeUnit = Unit.Mililiter;
            Assert.AreEqual(new[] { gram, mililiter, servingSize() }, sut.AllUsableUnitsWithDetalis);
            product.EnergyPer100g = 0;
            Assert.AreEqual(new[] { mililiter, servingSize() }, sut.AllUsableUnitsWithDetalis);
            product.ServingSizeUnit = Unit.Gram;
            Assert.AreEqual(new[] { gram, servingSize() }, sut.AllUsableUnitsWithDetalis);
        }
    }
}
