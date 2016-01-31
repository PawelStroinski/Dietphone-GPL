using System;
using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;

namespace Dietphone.Common.Phone.Tests
{
    public class ViewModelWithDateTests
    {
        [Test]
        public void IsNewer()
        {
            var factories = Substitute.For<Factories>();
            var meal = new Meal();
            var sut = new MealViewModel(meal, factories);
            Assert.IsTrue(sut.IsNewer);
            sut.Date = DateViewModel.CreateNormalDate(DateTime.Now);
            Assert.IsTrue(sut.IsNewer);
            sut.Date = DateViewModel.CreateGroupOfOlder();
            Assert.IsFalse(sut.IsNewer);
        }

        [Test]
        public void IsOlder()
        {
            var factories = Substitute.For<Factories>();
            var meal = new Meal();
            var sut = new MealViewModel(meal, factories);
            Assert.IsFalse(sut.IsOlder);
            sut.Date = DateViewModel.CreateNormalDate(DateTime.Now);
            Assert.IsFalse(sut.IsOlder);
            sut.Date = DateViewModel.CreateGroupOfOlder();
            Assert.IsTrue(sut.IsOlder);
        }
    }
}
