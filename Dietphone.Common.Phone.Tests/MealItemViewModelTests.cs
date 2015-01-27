using System;
using Dietphone.Models;
using Dietphone.Models.Tests;
using Dietphone.ViewModels;
using NUnit.Framework;

namespace Dietphone.Common.Phone.Tests
{
    public class MealItemViewModelTests
    {
        private Factories factories;
        private Product product;
        private MealItem item;
        private MealItemViewModel sut;
        private string gram, mililiter, ounce, pound;
        private Func<string> servingSize;

        [SetUp]
        public void TestInitialize()
        {
            factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            product = factories.CreateProduct();
            product.EnergyPer100g = 100;
            product.EnergyPerServing = 100;
            var meal = factories.CreateMeal();
            item = meal.AddItem();
            item.ProductId = product.Id;
            sut = new MealItemViewModel(item, factories);
            gram = Unit.Gram.GetAbbreviation();
            mililiter = Unit.Mililiter.GetAbbreviation();
            servingSize = () => Unit.ServingSize.GetAbbreviationOrServingSizeDetalis(product);
            ounce = Unit.Ounce.GetAbbreviation();
            pound = Unit.Pound.GetAbbreviation();
        }

        [Test]
        public void AllUsableUnitsWithDetalis()
        {
            Assert.AreEqual(new[] { gram, ounce, pound }, sut.AllUsableUnitsWithDetalis);
            product.ServingSizeValue = 1;
            Assert.AreEqual(new[] { gram, servingSize(), ounce, pound }, sut.AllUsableUnitsWithDetalis);
            product.ServingSizeUnit = Unit.Mililiter;
            Assert.AreEqual(new[] { gram, mililiter, servingSize(), ounce, pound }, sut.AllUsableUnitsWithDetalis);
            product.EnergyPer100g = 0;
            Assert.AreEqual(new[] { mililiter, servingSize() }, sut.AllUsableUnitsWithDetalis);
            product.ServingSizeUnit = Unit.Gram;
            Assert.AreEqual(new[] { gram, servingSize(), ounce, pound }, sut.AllUsableUnitsWithDetalis);
        }

        [Test]
        public void AllUsableUnitsWithDetalisWhenServingSizeIsInGramsButNutritionsAreOnlyPer100g()
        {
            product.ServingSizeValue = 15;
            product.ServingSizeUnit = Unit.Gram;
            product.EnergyPerServing = 0;
            Assert.AreEqual(new[] { gram, servingSize(), ounce, pound }, sut.AllUsableUnitsWithDetalis);
        }

        [Test]
        public void AllUsableUnitsWithDetalisWhenNoNutritions()
        {
            factories.Settings.Unit = Unit.Mililiter;
            product.EnergyPer100g = 0;
            product.EnergyPerServing = 0;
            Assert.AreEqual(new[] { mililiter }, sut.AllUsableUnitsWithDetalis);
        }

        [Test]
        public void ValueWrapperDoesNotNotifyAboutItself()
        {
            // This behaviour is useful for entering decimal point numbers into fields with binding on each key press.
            sut.Value = 100.ToString();
            Assert.AreEqual(100.ToString(), sut.ValueWrapper);
            sut.ChangesProperty("Value", () =>
            {
                sut.NotChangesProperty("ValueWrapper", () =>
                {
                    sut.ValueWrapper = (100.10).ToString();
                });
            });
            Assert.AreEqual((100.10).ToString(), sut.Value);
            Assert.AreEqual((100.10).ToString(), sut.ValueWrapper);
            sut.ChangesProperty("ValueWrapper", () => sut.Value = 100.ToString());
        }

        [Test]
        public void InitializeUnit()
        {
            factories.Settings.Unit = Unit.Ounce;
            sut.InitializeUnit();
            Assert.AreEqual(Unit.Ounce, item.Unit);
            product.ServingSizeValue = 15;
            product.ServingSizeUnit = Unit.Mililiter;
            sut.InitializeUnit();
            Assert.AreEqual(Unit.Ounce, item.Unit);
            product.EnergyPer100g = 0;
            sut.InitializeUnit();
            Assert.AreEqual(Unit.Mililiter, item.Unit);
            product.EnergyPerServing = 0;
            sut.InitializeUnit();
            Assert.AreEqual(Unit.Ounce, item.Unit);
        }
    }
}
