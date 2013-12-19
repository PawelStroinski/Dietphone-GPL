using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;

namespace Dietphone.Common.Phone.Tests
{
    public class MainViewModelTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void WhenAddingMealItemSetsAddMruToTrue(bool shouldAddMealItem)
        {
            var sut = new MainViewModel(Substitute.For<Factories>());
            var productListing = new ProductListingViewModel(Substitute.For<Factories>(),
                new BackgroundWorkerSyncFactory());
            sut.ProductListing = productListing;
            sut.MealItemEditing = new MealItemEditingViewModel();
            var navigator = Substitute.For<Navigator>();
            navigator.ShouldAddMealItem().Returns(shouldAddMealItem);
            sut.Navigator = navigator;
            Assert.AreEqual(shouldAddMealItem, productListing.AddMru);
        }
    }
}
