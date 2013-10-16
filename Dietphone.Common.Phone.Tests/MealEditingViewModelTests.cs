using System;
using System.Collections.Generic;
using System.Linq;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace Dietphone.Common.Phone.Tests
{
    public class MealEditingViewModelTests
    {
        private MealEditingViewModel sut;
        private Meal meal;
        private Factories factories;

        [SetUp]
        public void TestInitialize()
        {
            meal = new Meal();
            meal.InitializeItems(new List<MealItem>());
            factories = Substitute.For<Factories>();
            factories.Finder.FindMealById(Guid.Empty).Returns(meal);
            factories.MealNames.Returns(new List<MealName>());
            sut = new MealEditingViewModel(factories);
            sut.Navigator = Substitute.For<Navigator>();
            sut.StateProvider = Substitute.For<StateProvider>();
        }

        [Test]
        public void IsLockedDateTime_DefaultsToTrueWhenDateTimeIsIn3Minutes()
        {
            meal.DateTime = DateTime.Now.AddMinutes(-2.9);
            sut.Load();
            Assert.IsTrue(!sut.NotIsLockedDateTime);
        }

        [Test]
        public void IsLockedDateTime_DefaultsToFalseWhenDateTimeIsNotIn3Minutes()
        {
            meal.DateTime = DateTime.Now.AddMinutes(-3.1);
            sut.Load();
            Assert.IsFalse(!sut.NotIsLockedDateTime);
        }

        [Test]
        public void IsLockedDateTime_SetToTrueSetsDateTimeToCurrent()
        {
            sut.Load();
            sut.NotIsLockedDateTime = !true;
            Assert.IsTrue(DateTime.Now - sut.Subject.DateTime <= TimeSpan.FromSeconds(1));
        }

        [Test]
        public void DateTime_WhenChanged_SetsIsLockedDateTimeToFalse()
        {
            meal.DateTime = DateTime.Now;
            sut.Load();
            Assert.IsTrue(!sut.NotIsLockedDateTime);
            sut.Subject.DateTime = DateTime.Now.AddSeconds(1);
            Assert.IsFalse(!sut.NotIsLockedDateTime);
        }
    }
}
