using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Dietphone.Models.Tests
{
    [TestClass]
    public class FinderTests
    {
        private Mock<Factories> factories;

        [TestInitialize()]
        public void TestInitialize()
        {
            factories = new Mock<Factories>();
        }

        [TestMethod]
        public void FindInsulinCircumstanceById()
        {
            var expected = new InsulinCircumstance { Id = Guid.NewGuid() };
            factories.Setup(f => f.InsulinCircumstances).Returns(new List<InsulinCircumstance> { expected });
            var finder = new FinderImpl(factories.Object);
            var actual = finder.FindInsulinCircumstanceById(expected.Id);
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void FindSugarsAfterInsulin_IfNoSugarsInFourHoursAfter_ReturnsEmpty()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(4.1) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(-1) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
            factories.Setup(f => f.Insulins).Returns(new List<Insulin>());
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var sugars = finder.FindSugarsAfterInsulin(insulin);
            Assert.AreEqual(0, sugars.Count);
        }

        [TestMethod]
        public void FindSugarsAfterInsulin_IfSugarsInFourHoursAfter_ReturnsThoseSugars()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(1) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(3.5) };
            var sugar3 = new Sugar { DateTime = DateTime.Now.AddHours(5) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2, sugar3 });
            factories.Setup(f => f.Insulins).Returns(new List<Insulin>());
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var sugars = finder.FindSugarsAfterInsulin(insulin);
            Assert.IsTrue(Enumerable.SequenceEqual(new List<Sugar> { sugar1, sugar2 }, sugars));
        }

        [TestMethod]
        public void FindSugarsAfterInsulin_IfAnotherInsulinSoonerThanFourHoursLater_ReturnsOnlySugarsBeforeThisInsulin()
        {
            Action<int> test = removeMinutes =>
            {
                var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(1) };
                var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(2) };
                factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
                var insulin1 = new Insulin { DateTime = DateTime.Now };
                var insulin2 = new Insulin { DateTime = sugar2.DateTime.AddMinutes(-removeMinutes) };
                factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin1, insulin2 });
                var finder = new FinderImpl(factories.Object);
                var sugars = finder.FindSugarsAfterInsulin(insulin1);
                Assert.IsTrue(Enumerable.SequenceEqual(new List<Sugar> { sugar1 }, sugars));
            };
            test(30);
            test(0);
        }

        [TestMethod]
        public void FindSugarsAfterInsulin_IfAnotherInsulinLaterThanFourHoursLater_ReturnsAllSugarsInFourHours()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(1) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(2) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
            var insulin1 = new Insulin { DateTime = DateTime.Now };
            var insulin2 = new Insulin { DateTime = DateTime.Now.AddHours(5) };
            factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin1, insulin2 });
            var finder = new FinderImpl(factories.Object);
            var sugars = finder.FindSugarsAfterInsulin(insulin1);
            Assert.IsTrue(Enumerable.SequenceEqual(new List<Sugar> { sugar1, sugar2 }, sugars));
        }

        [TestMethod]
        public void FindSugarsAfterInsulin_SortsReturnedSugarsChronologically()
        {
            var sugar1 = new Sugar { DateTime = DateTime.Now.AddHours(2) };
            var sugar2 = new Sugar { DateTime = DateTime.Now.AddHours(1) };
            factories.Setup(f => f.Sugars).Returns(new List<Sugar> { sugar1, sugar2 });
            factories.Setup(f => f.Insulins).Returns(new List<Insulin>());
            var finder = new FinderImpl(factories.Object);
            var insulin = new Insulin { DateTime = DateTime.Now };
            var sugars = finder.FindSugarsAfterInsulin(insulin);
            Assert.IsTrue(Enumerable.SequenceEqual(new List<Sugar> { sugar2, sugar1 }, sugars));
        }

        [TestMethod]
        public void FindNextInsulin_ReturnsNullIfNoNextInsulin()
        {
            var insulin1 = new Insulin { DateTime = DateTime.Now };
            var insulin2 = new Insulin { DateTime = DateTime.Now.AddHours(-1) };
            factories.Setup(f => f.Insulins).Returns(new List<Insulin> { insulin2 });
            var finder = new FinderImpl(factories.Object);
            var insulin = finder.FindNextInsulin(insulin1);
            Assert.IsNull(insulin);
        }

        [TestMethod]
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
