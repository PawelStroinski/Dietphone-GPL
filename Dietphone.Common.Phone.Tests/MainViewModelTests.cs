using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;

namespace Dietphone.Common.Phone.Tests
{
    public class MainViewModelTests
    {
        [Test]
        public void WhenShouldGoToInsulinAndSugarTabGoesToThisTabButOnlyOnce()
        {
            var sut = new MainViewModel(Substitute.For<Factories>());
            var navigator = Substitute.For<Navigator>();
            navigator.ShouldGoToInsulinAndSugarTab().Returns(true);
            sut.Navigator = navigator;
            Assert.AreEqual(2, sut.Pivot);
            sut.Pivot = 1;
            sut.Navigator = navigator;
            Assert.AreEqual(1, sut.Pivot);
        }
    }
}
