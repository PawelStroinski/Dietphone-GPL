using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;

namespace Dietphone.Common.Phone.Tests
{
    public class TypedViewModelTests
    {
        private InsulinViewModel insulin;
        private SugarViewModel sugar;
        private MealViewModel meal;

        [SetUp]
        public void TestInitialize()
        {
            insulin = new InsulinViewModel(new Insulin(), Substitute.For<Factories>(), null);
            sugar = new SugarViewModel(new Sugar(), Substitute.For<Factories>());
            meal = new MealViewModel(new Meal(), Substitute.For<Factories>());
        }

        [Test]
        public void IsInsulin()
        {
            Assert.IsTrue(insulin.IsInsulin);
            Assert.IsFalse(sugar.IsInsulin);
            Assert.IsFalse(meal.IsInsulin);
        }

        [Test]
        public void IsSugar()
        {
            Assert.IsFalse(insulin.IsSugar);
            Assert.IsTrue(sugar.IsSugar);
            Assert.IsFalse(meal.IsSugar);
        }

        [Test]
        public void IsMeal()
        {
            Assert.IsFalse(insulin.IsMeal);
            Assert.IsFalse(sugar.IsMeal);
            Assert.IsTrue(meal.IsMeal);
        }

        [Test]
        public void IsNotMeal()
        {
            Assert.IsTrue(insulin.IsNotMeal);
            Assert.IsTrue(sugar.IsNotMeal);
            Assert.IsFalse(meal.IsNotMeal);
        }
    }
}
