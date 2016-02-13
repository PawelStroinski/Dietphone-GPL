using System;
using System.Linq;
using Dietphone.Smartphone.Tests.Tools;
using Dietphone.Tools;
using Dietphone.ViewModels;
using MvvmCross.Core.Platform;
using MvvmCross.Core.Views;
using MvvmCross.Test.Core;
using NSubstitute;
using NUnit.Framework;

namespace Dietphone.Smartphone.Tests
{
    public class NavigatorImplTests : MvxIoCSupportingTest
    {
        private Navigator sut;
        private MockDispatcher mockDispatcher;

        [SetUp]
        public void TestInitialize()
        {
            base.Setup();
            mockDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(mockDispatcher);
            Ioc.RegisterSingleton<IMvxStringToTypeParser>(new MvxStringToTypeParser());
            var navigationService = Substitute.For<NavigationService>();
            var navigationContext = Substitute.For<NavigationContext>();
            sut = new NavigatorImpl(navigationService, navigationContext);
        }

        [Test]
        public void GoToMealEditing()
        {
            var mealId = Guid.NewGuid();
            sut.GoToMealEditing(mealId);
            var request = mockDispatcher.Requests.Single();
            Assert.AreEqual(typeof(MealEditingViewModel), request.ViewModelType);
            Assert.AreEqual(mealId.ToString(), request.ParameterValues["MealIdToEdit"]);
        }

        [Test]
        public void GoToInsulinEditing()
        {
            var insulinId = Guid.NewGuid();
            sut.GoToInsulinEditing(insulinId);
            var request = mockDispatcher.Requests.Single();
            Assert.AreEqual(typeof(InsulinEditingViewModel), request.ViewModelType);
            Assert.AreEqual(insulinId.ToString(), request.ParameterValues["InsulinIdToEdit"]);
            Assert.AreEqual(Guid.Empty.ToString(), request.ParameterValues["RelatedMealId"]);
        }

        [Test]
        public void GoToNewInsulin()
        {
            sut.GoToNewInsulin();
            var request = mockDispatcher.Requests.Single();
            Assert.AreEqual(typeof(InsulinEditingViewModel), request.ViewModelType);
            Assert.AreEqual(Guid.Empty.ToString(), request.ParameterValues["InsulinIdToEdit"]);
            Assert.AreEqual(Guid.Empty.ToString(), request.ParameterValues["RelatedMealId"]);
        }

        [Test]
        public void GoToNewInsulinRelatedToMeal()
        {
            var mealId = Guid.NewGuid();
            sut.GoToNewInsulinRelatedToMeal(mealId);
            var request = mockDispatcher.Requests.Single();
            Assert.AreEqual(typeof(InsulinEditingViewModel), request.ViewModelType);
            Assert.AreEqual(Guid.Empty.ToString(), request.ParameterValues["InsulinIdToEdit"]);
            Assert.AreEqual(mealId.ToString(), request.ParameterValues["RelatedMealId"]);
        }

        [Test]
        public void GoToInsulinEditingRelatedToMeal()
        {
            var insulinId = Guid.NewGuid();
            var mealId = Guid.NewGuid();
            sut.GoToInsulinEditingRelatedToMeal(insulinId: insulinId, mealId: mealId);
            var request = mockDispatcher.Requests.Single();
            Assert.AreEqual(typeof(InsulinEditingViewModel), request.ViewModelType);
            Assert.AreEqual(insulinId.ToString(), request.ParameterValues["InsulinIdToEdit"]);
            Assert.AreEqual(mealId.ToString(), request.ParameterValues["RelatedMealId"]);
        }

        [Test]
        public void GoToMain()
        {
            sut.GoToMain();
            var request = mockDispatcher.Requests.Single();
            Assert.AreEqual(typeof(MainViewModel), request.ViewModelType);
            Assert.AreEqual("False", request.ParameterValues["ShouldAddMealItem"]);
        }

        [Test]
        public void GoToMainToAddMealItem()
        {
            sut.GoToMainToAddMealItem();
            var request = mockDispatcher.Requests.Single();
            Assert.AreEqual(typeof(MainViewModel), request.ViewModelType);
            Assert.AreEqual("True", request.ParameterValues["ShouldAddMealItem"]);
        }
    }
}
