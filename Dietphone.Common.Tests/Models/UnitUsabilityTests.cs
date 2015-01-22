using NUnit.Framework;

namespace Dietphone.Models.Tests
{
    public class UnitUsabilityTests
    {
        private Product product;
        private UnitUsability sut;

        [SetUp]
        public void TestInitialize()
        {
            product = new Product();
            product.EnergyPer100g = 100;
            product.EnergyPerServing = 100;
            sut = new UnitUsability();
            sut.Product = product;
        }

        [Test]
        public void AnyNutrientsPerUnitPresent()
        {
            sut.Unit = Unit.Gram;
            Assert.IsTrue(sut.AnyNutrientsPerUnitPresent);
            sut.Unit = Unit.Mililiter;
            Assert.IsFalse(sut.AnyNutrientsPerUnitPresent);
            sut.Unit = Unit.ServingSize;
            Assert.IsFalse(sut.AnyNutrientsPerUnitPresent);
            product.ServingSizeValue = 1;
            Assert.IsTrue(sut.AnyNutrientsPerUnitPresent);
            product.ServingSizeUnit = Unit.Mililiter;
            Assert.IsTrue(sut.AnyNutrientsPerUnitPresent);
            sut.Unit = Unit.Mililiter;
            Assert.IsTrue(sut.AnyNutrientsPerUnitPresent);
        }

        [Test]
        public void AreNutrientsPer100gUsable()
        {
            sut.Unit = Unit.Gram;
            Assert.IsTrue(sut.AreNutrientsPer100gUsable);
            sut.Unit = Unit.Mililiter;
            Assert.IsFalse(sut.AreNutrientsPer100gUsable);
            sut.Unit = Unit.ServingSize;
            Assert.IsFalse(sut.AreNutrientsPer100gUsable);
            product.ServingSizeValue = 1;
            Assert.IsFalse(sut.AreNutrientsPer100gUsable);
            product.ServingSizeUnit = Unit.Mililiter;
            Assert.IsFalse(sut.AreNutrientsPer100gUsable);
            sut.Unit = Unit.Mililiter;
            Assert.IsFalse(sut.AreNutrientsPer100gUsable);
        }

        [Test]
        public void AreNutrientsPerServingUsable()
        {
            sut.Unit = Unit.Gram;
            Assert.IsFalse(sut.AreNutrientsPerServingUsable);
            sut.Unit = Unit.Mililiter;
            Assert.IsFalse(sut.AreNutrientsPerServingUsable);
            sut.Unit = Unit.ServingSize;
            Assert.IsFalse(sut.AreNutrientsPerServingUsable);
            product.ServingSizeValue = 1;
            Assert.IsTrue(sut.AreNutrientsPerServingUsable);
            product.ServingSizeUnit = Unit.Mililiter;
            Assert.IsTrue(sut.AreNutrientsPerServingUsable);
            sut.Unit = Unit.Mililiter;
            Assert.IsTrue(sut.AreNutrientsPerServingUsable);
            sut.Unit = Unit.Gram;
            Assert.IsFalse(sut.AreNutrientsPerServingUsable);
            product.ServingSizeUnit = Unit.Gram;
            Assert.IsTrue(sut.AreNutrientsPerServingUsable);
        }
    }
}
