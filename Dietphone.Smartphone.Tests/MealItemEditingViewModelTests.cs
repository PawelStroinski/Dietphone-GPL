using System.Collections.Generic;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace Dietphone.Smartphone.Tests
{
    public class MealItemEditingViewModelTests
    {
        private StateProvider stateProvider;

        [SetUp]
        public void TestInitialize()
        {
            stateProvider = Substitute.For<StateProvider>();
            stateProvider.State.Returns(new Dictionary<string, object>());
            stateProvider.State[MealItemEditingViewModel.MEAL_ITEM] = "state";
        }

        [Test]
        public void Show()
        {
            var sut = new MealItemEditingViewModel();
            var model = new MealItem { Value = 5 };
            var mealItem = new MealItemViewModel(model, Substitute.For<Factories>());
            var needToShow = false;
            stateProvider.State[MealItemEditingViewModel.MEAL_ITEM] = model.Serialize(string.Empty);
            model.Value = 1;
            sut.StateProvider = stateProvider;
            sut.NeedToShow += delegate { needToShow = true; };
            sut.ChangesProperty("IsVisible", () => sut.Show(mealItem));
            Assert.AreEqual("5", sut.Subject.Value);
            Assert.IsTrue(needToShow);
            Assert.IsTrue(sut.IsVisible);
        }

        [Test]
        public void Confirm()
        {
            var sut = new MealItemEditingViewModel();
            var confirmed = false;
            sut.StateProvider = stateProvider;
            sut.Confirmed += delegate { confirmed = true; };
            sut.ChangesProperty("IsVisible", () => sut.Confirm.Call());
            Assert.IsTrue(confirmed);
            Assert.IsFalse(sut.IsVisible);
            Assert.IsFalse(stateProvider.State.ContainsKey(MealItemEditingViewModel.MEAL_ITEM));
        }

        [Test]
        public void Cancel()
        {
            var sut = new MealItemEditingViewModel();
            var cancelled = false;
            sut.StateProvider = stateProvider;
            sut.Cancelled += delegate { cancelled = true; };
            sut.ChangesProperty("IsVisible", () => sut.Cancel.Call());
            Assert.IsTrue(cancelled);
            Assert.IsFalse(sut.IsVisible);
            Assert.IsFalse(stateProvider.State.ContainsKey(MealItemEditingViewModel.MEAL_ITEM));
        }

        [Test]
        public void Delete()
        {
            var sut = new MealItemEditingViewModel();
            var needToDelete = false;
            sut.StateProvider = stateProvider;
            sut.NeedToDelete += delegate { needToDelete = true; };
            sut.ChangesProperty("IsVisible", () => sut.Delete.Call());
            Assert.IsTrue(needToDelete);
            Assert.IsFalse(sut.IsVisible);
            Assert.IsFalse(stateProvider.State.ContainsKey(MealItemEditingViewModel.MEAL_ITEM));
        }
    }
}
