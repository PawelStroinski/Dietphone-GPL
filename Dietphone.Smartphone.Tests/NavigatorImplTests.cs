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
            var id = Guid.NewGuid();
            sut.GoToMealEditing(id);
            var request = mockDispatcher.Requests.Single();
            Assert.AreEqual(typeof(MealEditingViewModel), request.ViewModelType);
            Assert.AreEqual(id.ToString(), request.ParameterValues["MealId"]);
        }
    }
}
