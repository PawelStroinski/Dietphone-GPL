using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using System.Collections.Generic;

namespace Dietphone.Models.Tests
{
    public class FinderTests
    {
        private Mock<Factories> factories;

        [SetUp]
        public void TestInitialize()
        {
            factories = new Mock<Factories>();
        }

        [Test]
        public void FindInsulinCircumstanceById()
        {
            var expected = new InsulinCircumstance { Id = Guid.NewGuid() };
            factories.Setup(f => f.InsulinCircumstances).Returns(new List<InsulinCircumstance> { expected });
            var finder = new FinderImpl(factories.Object);
            var actual = finder.FindInsulinCircumstanceById(expected.Id);
            Assert.AreSame(expected, actual);
        }

        [Test]
        public void FindMealByInsulin_IfNoMealsInHour_ReturnsNull()
        {
            var meal1 = new Meal { DateTime = DateTime.Now.AddHours(1.1) };
            var meal2 = new Meal { DateTime = DateTime.Now.AddHours(-1.1) };
            factories.Setup(f => f.Meals).Returns(new List<Meal> { meal1, meal2 });
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var meal = finder.FindMealByInsulin(insulin);
            Assert.IsNull(meal);
        }

        [Test]
        public void FindMealByInsulin_IfMealInHourBefore_ReturnsMeal()
        {
            var meal1 = new Meal { DateTime = DateTime.Now.AddHours(1.1) };
            var meal2 = new Meal { DateTime = DateTime.Now.AddHours(-0.9) };
            factories.Setup(f => f.Meals).Returns(new List<Meal> { meal1, meal2 });
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var meal = finder.FindMealByInsulin(insulin);
            Assert.AreSame(meal2, meal);
        }

        [Test]
        public void FindMealByInsulin_IfMealInHourAfter_ReturnsMeal()
        {
            var meal1 = new Meal { DateTime = DateTime.Now.AddHours(0.9) };
            var meal2 = new Meal { DateTime = DateTime.Now.AddHours(-1.1) };
            factories.Setup(f => f.Meals).Returns(new List<Meal> { meal1, meal2 });
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var meal = finder.FindMealByInsulin(insulin);
            Assert.AreSame(meal1, meal);
        }

        [Test]
        public void FindMealByInsulin_IfTwoMealsInHour_ReturnsCloserMeal()
        {
            var meal1 = new Meal { DateTime = DateTime.Now.AddHours(0.9) };
            var meal2 = new Meal { DateTime = DateTime.Now.AddHours(-0.8) };
            factories.Setup(f => f.Meals).Returns(new List<Meal> { meal1, meal2 });
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var meal = finder.FindMealByInsulin(insulin);
            Assert.AreSame(meal2, meal);
        }

        [Test]
        public void FindInsulinByMeal_IfNoInsulinsInHour_ReturnsNull()
        {
            var insulin1 = new Insulin { DateTime = DateTime.Now.AddHours(1.1) };
            var insulin2 = new Insulin { DateTime = DateTime.Now.AddHours(-1.1) };
            factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin1, insulin2 });
            var finder = new FinderImpl(factories.Object);
            var meal = new Meal { DateTime = DateTime.Now };
            var insulin = finder.FindInsulinByMeal(meal);
            Assert.IsNull(insulin);
        }

        [Test]
        public void FindInsulinByMeal_IfInsulinInHourBefore_ReturnsInsulin()
        {
            var insulin1 = new Insulin { DateTime = DateTime.Now.AddHours(1.1) };
            var insulin2 = new Insulin { DateTime = DateTime.Now.AddHours(-0.9) };
            factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin1, insulin2 });
            var finder = new FinderImpl(factories.Object);
            var meal = new Meal { DateTime = DateTime.Now };
            var insulin = finder.FindInsulinByMeal(meal);
            Assert.AreSame(insulin2, insulin);
        }

        [Test]
        public void FindInsulinByMeal_IfInsulinInHourAfter_ReturnsInsulin()
        {
            var insulin1 = new Insulin { DateTime = DateTime.Now.AddHours(0.9) };
            var insulin2 = new Insulin { DateTime = DateTime.Now.AddHours(-1.1) };
            factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin1, insulin2 });
            var finder = new FinderImpl(factories.Object);
            var meal = new Meal { DateTime = DateTime.Now };
            var insulin = finder.FindInsulinByMeal(meal);
            Assert.AreSame(insulin1, insulin);
        }

        [Test]
        public void FindInsulinByMeal_IfTwoInsulinsInHour_ReturnsCloserInsulin()
        {
            var insulin1 = new Insulin { DateTime = DateTime.Now.AddHours(0.9) };
            var insulin2 = new Insulin { DateTime = DateTime.Now.AddHours(-0.8) };
            factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin1, insulin2 });
            var finder = new FinderImpl(factories.Object);
            var meal = new Meal { DateTime = DateTime.Now };
            var insulin = finder.FindInsulinByMeal(meal);
            Assert.AreSame(insulin2, insulin);
        }

        [Test]
        public void FindSugarBeforeInsulin_IfNoSugarsInHalfAnHourBefore_ReturnsNull()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(0.1) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(-0.6) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var sugar = finder.FindSugarBeforeInsulin(insulin);
            Assert.IsNull(sugar);
        }

        [Test]
        public void FindSugarBeforeInsulin_IfSugarInHalfAnHourBefore_ReturnsSugar()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(-0.6) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(-0.4) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var sugar = finder.FindSugarBeforeInsulin(insulin);
            Assert.AreSame(sugar2, sugar);
        }

        [Test]
        public void FindSugarBeforeInsulin_ReturnsSugarWhichIsAtSameTimeAsInsulin()
        {
            var sugar = new Sugar { DateTime = DateTime.Now };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar });
            factories.Setup(f => f.Insulins).Returns(new List<Insulin>());
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = sugar.DateTime };
            var foundSugar = finder.FindSugarBeforeInsulin(insulin);
            Assert.AreSame(sugar, foundSugar);
        }

        [Test]
        public void FindSugarBeforeInsulin_IfTwoSugarsInHalfAnHour_ReturnsLatestSugar()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(-0.4) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(-0.3) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var sugar = finder.FindSugarBeforeInsulin(insulin);
            Assert.AreSame(sugar2, sugar);
        }

        [Test]
        public void FindSugarsAfterInsulin_IfNoSugarsInThreeHoursAfter_ReturnsEmpty()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(3.1) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(-1) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
            factories.Setup(f => f.Insulins).Returns(new List<Insulin>());
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var sugars = finder.FindSugarsAfterInsulin(insulin, 3);
            Assert.AreEqual(0, sugars.Count);
        }

        [Test]
        public void FindSugarsAfterInsulin_DoesntReturnSugarsWhichIsAtSameTimeAsInsulin()
        {
            var sugar = new Sugar { DateTime = DateTime.Now };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar });
            factories.Setup(f => f.Insulins).Returns(new List<Insulin>());
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = sugar.DateTime };
            var sugars = finder.FindSugarsAfterInsulin(insulin, 2);
            Assert.IsEmpty(sugars);
        }

        [Test]
        public void FindSugarsAfterInsulin_IfSugarsInFourHoursAfter_ReturnsThoseSugars()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(1) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(3.5) };
            var sugar3 = new Sugar { DateTime = DateTime.Now.AddHours(5) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2, sugar3 });
            factories.Setup(f => f.Insulins).Returns(new List<Insulin>());
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var sugars = finder.FindSugarsAfterInsulin(insulin, 4);
            Assert.IsTrue(Enumerable.SequenceEqual(new List<Sugar> { sugar1, sugar2 }, sugars));
        }

        [TestCase(30)]
        [TestCase(0)]
        public void FindSugarsAfterInsulin_IfAnotherInsulinSoonerThanFourHoursLater_ReturnsOnlySugarsBeforeThisInsulin(
            int removeMinutes)
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(1) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(2) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
            var insulin1 = new Insulin { DateTime = DateTime.Now };
            var insulin2 = new Insulin { DateTime = sugar2.DateTime.AddMinutes(-removeMinutes) };
            factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin1, insulin2 });
            var finder = new FinderImpl(factories.Object);
            var sugars = finder.FindSugarsAfterInsulin(insulin1, 4);
            Assert.IsTrue(Enumerable.SequenceEqual(new List<Sugar> { sugar1 }, sugars));
        }

        [Test]
        public void FindSugarsAfterInsulin_IfAnotherInsulinLaterThanFourHoursLater_ReturnsAllSugarsInFourHours()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(1) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(2) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
            var insulin1 = new Insulin { DateTime = DateTime.Now };
            var insulin2 = new Insulin { DateTime = DateTime.Now.AddHours(5) };
            factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin1, insulin2 });
            var finder = new FinderImpl(factories.Object);
            var sugars = finder.FindSugarsAfterInsulin(insulin1, 4);
            Assert.IsTrue(Enumerable.SequenceEqual(new List<Sugar> { sugar1, sugar2 }, sugars));
        }

        [Test]
        public void FindSugarsAfterInsulin_SortsReturnedSugarsChronologically()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(2) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(1) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
            factories.Setup(f => f.Insulins).Returns(new List<Insulin>());
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var sugars = finder.FindSugarsAfterInsulin(insulin, 4);
            Assert.IsTrue(Enumerable.SequenceEqual(new List<Sugar> { sugar2, sugar1 }, sugars));
        }

        [Test]
        public void FindNextInsulin_ReturnsNullIfNoNextInsulin()
        {
            var insulin1 = new Insulin { DateTime = DateTime.Now };
            var insulin2 = new Insulin { DateTime = DateTime.Now.AddHours(-1) };
            factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin2 });
            var finder = new FinderImpl(factories.Object);
            var insulin = finder.FindNextInsulin(insulin1);
            Assert.IsNull(insulin);
        }

        [Test]
        public void FindNextInsulin_ReturnsFirstChronologicallyFollowingInsulin()
        {
            var insulin1 = new Insulin { DateTime = DateTime.Now };
            var insulin2 = new Insulin { DateTime = DateTime.Now.AddHours(2) };
            var insulin3 = new Insulin { DateTime = DateTime.Now.AddHours(1) };
            factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin2, insulin3 });
            var finder = new FinderImpl(factories.Object);
            var insulin = finder.FindNextInsulin(insulin1);
            Assert.AreSame(insulin3, insulin);
        }
    }
}
