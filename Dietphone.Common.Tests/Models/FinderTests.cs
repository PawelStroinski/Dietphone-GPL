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
    }
}
