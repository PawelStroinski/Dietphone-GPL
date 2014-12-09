using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using Dietphone.Tools;
using System;
using System.Threading;

namespace Dietphone.Common.Phone.Tests
{
    public class MainViewModelTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void WhenAddingMealItemSetsAddMruToTrue(bool shouldAddMealItem)
        {
            var sut = new MainViewModel(Substitute.For<Factories>(), Substitute.For<Cloud>(),
                Substitute.For<TimerFactory>(), new BackgroundWorkerSyncFactory());
            var productListing = new ProductListingViewModel(Substitute.For<Factories>(),
                new BackgroundWorkerSyncFactory());
            sut.ProductListing = productListing;
            sut.MealItemEditing = new MealItemEditingViewModel();
            var navigator = Substitute.For<Navigator>();
            navigator.ShouldAddMealItem().Returns(shouldAddMealItem);
            sut.Navigator = navigator;
            Assert.AreEqual(shouldAddMealItem, productListing.AddMru);
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(true, false)]
        public void UiRenderedCreatesTimerToExportToCloud(bool shouldExport, bool throwInExport)
        {
            var cloud = Substitute.For<Cloud>();
            cloud.When(Cloud => Cloud.Export()).Do(_ => { if (throwInExport) throw new Exception(); });
            cloud.ShouldExport().Returns(shouldExport);
            var timerFactory = Substitute.For<TimerFactory>();
            TimerCallback timerCallback = null;
            timerFactory.Create(null, null, 0, 0).ReturnsForAnyArgs((args) =>
            {
                timerCallback = (TimerCallback)args[0];
                Assert.IsNull(args[1]);
                Assert.AreEqual(500, args[2]);
                Assert.AreEqual(-1, args[3]);
                return new Timer(timerCallback);
            });
            var sut = new MainViewModel(Substitute.For<Factories>(), cloud, timerFactory,
                new BackgroundWorkerSyncFactory());
            sut.StateProvider = Substitute.For<StateProvider>();
            var exportToCloudErrored = false;
            sut.ExportToCloudError += delegate { exportToCloudErrored = true; };
            Assert.IsNull(timerCallback);
            sut.UiRendered();
            if (shouldExport)
            {
                Assert.IsNotNull(timerCallback);
                cloud.DidNotReceive().Export();
                timerCallback(null);
                cloud.Received().Export();
                Assert.AreEqual(throwInExport, exportToCloudErrored);
            } else
                Assert.IsNull(timerCallback);
        }
    }
}
