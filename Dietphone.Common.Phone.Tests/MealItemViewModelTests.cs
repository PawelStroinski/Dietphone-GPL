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
        private string gram, mililiter;
        private Func<string> servingSize;

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
            gram = Unit.Gram.GetAbbreviation();
            mililiter = Unit.Mililiter.GetAbbreviation();
            servingSize = () => Unit.ServingSize.GetAbbreviationOrServingSizeDetalis(product);
        }

        [Test]
        public void AllUsableUnitsWithDetalis()
        {
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

        [Test]
        public void AllUsableUnitsWithDetalisWhenServingSizeIsInGramsButNutritionsAreOnlyPer100g()
        {
            product.ServingSizeValue = 15;
            product.ServingSizeUnit = Unit.Gram;
            product.EnergyPerServing = 0;
            Assert.AreEqual(new[] { gram, servingSize() }, sut.AllUsableUnitsWithDetalis);
        }
    }
}
