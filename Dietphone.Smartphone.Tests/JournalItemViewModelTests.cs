using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using System;

namespace Dietphone.Smartphone.Tests
{
    public class JournalItemViewModelTests
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

        [Test]
        public void HasText2()
        {
            var sut = new Sut();
            Assert.IsTrue(sut.HasText2);
            sut.text2 = null;
            Assert.IsFalse(sut.HasText2);
            sut.text2 = string.Empty;
            Assert.IsFalse(sut.HasText2);
        }

        [Test]
        public void FilterIn()
        {
            var sut = new Sut();
            Assert.IsTrue(sut.FilterIn("foo"));
            Assert.IsFalse(sut.FilterIn("z"));
            Assert.IsTrue(sut.FilterIn("TWO"));
        }

        class Sut : JournalItemViewModel
        {
            public string text2 = "tWO";

            public override string Text
            {
                get { return "Foo bar"; }
            }

            public override string Text2
            {
                get { return text2; }
            }

            public override DateTime DateTime
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public override Guid Id
            {
                get { throw new NotImplementedException(); }
            }

            public override bool IsInsulin
            {
                get { throw new NotImplementedException(); }
            }

            public override bool IsSugar
            {
                get { throw new NotImplementedException(); }
            }

            public override bool IsMeal
            {
                get { throw new NotImplementedException(); }
            }

            public override bool IsNotMeal
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
