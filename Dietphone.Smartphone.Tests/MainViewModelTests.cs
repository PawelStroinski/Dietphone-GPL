using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using Dietphone.Tools;
using System;
using System.Threading;
using Dietphone.Models.Tests;
using System.Collections.Generic;

namespace Dietphone.Smartphone.Tests
{
    public class MainViewModelTests
    {
        public class WhenAddingMealItem : MainViewModelTests
        {
            private Factories factories;
            private MainViewModel sut;
            private ProductListingViewModel productListing;
            private MealItemEditingViewModel mealItemEditing;
            private Navigator navigator;

            [SetUp]
            public void TestInitialize()
            {
                factories = new FactoriesImpl();
                factories.StorageCreator = new StorageCreatorStub();
                sut = new MainViewModel(factories, Substitute.For<Cloud>(),
                    Substitute.For<TimerFactory>(), new BackgroundWorkerSyncFactory());
                productListing = new ProductListingViewModel(factories,
                    new BackgroundWorkerSyncFactory());
                sut.ProductListing = productListing;
                mealItemEditing = new MealItemEditingViewModel();
                sut.MealItemEditing = mealItemEditing;
                var stateProvider = Substitute.For<StateProvider>();
                stateProvider.State.Returns(new Dictionary<string, object>());
                sut.StateProvider = stateProvider;
                navigator = Substitute.For<Navigator>();
            }

            [TestCase(true)]
            [TestCase(false)]
            public void SetsAddMruToTrue(bool shouldAddMealItem)
            {
                navigator.ShouldAddMealItem().Returns(shouldAddMealItem);
                sut.Navigator = navigator;
                Assert.AreEqual(shouldAddMealItem, productListing.AddMru);
            }

            [Test]
            public void InitializesUnit()
            {
                navigator.ShouldAddMealItem().Returns(true);
                sut.Navigator = navigator;
                var product = factories.CreateProduct();
                var productViewModel = new ProductViewModel(product);
                factories.Settings.Unit = Unit.Pound;
                productListing.Choose(productViewModel);
                Assert.AreEqual(Unit.Pound, mealItemEditing.Subject.Model.Unit);
                product.EnergyPerServing = 100;
                product.ServingSizeValue = 15;
                product.ServingSizeUnit = Unit.Mililiter;
                productListing.Choose(productViewModel);
                Assert.AreEqual(Unit.Mililiter, mealItemEditing.Subject.Model.Unit);
            }
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
            var factories = Substitute.For<Factories>();
            factories.Settings.Returns(new Settings());
            var sut = new MainViewModel(factories, cloud, timerFactory,
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

        [TestCase(true)]
        [TestCase(false)]
        public void UiRenderedShowsWelcomeScreen(bool showWelcomeScreen)
        {
            var factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            factories.Settings.ShowWelcomeScreen = showWelcomeScreen;
            var sut = new MainViewModel(factories, Substitute.For<Cloud>(),
                Substitute.For<TimerFactory>(), new BackgroundWorkerSyncFactory());
            var stateProvider = Substitute.For<StateProvider>();
            stateProvider.State.Returns(new Dictionary<string, object>());
            sut.StateProvider = stateProvider;
            var showWelcomeScreenCalled = false;
            sut.ShowWelcomeScreen += delegate { showWelcomeScreenCalled = true; };
            sut.UiRendered();
            Assert.AreEqual(showWelcomeScreen, showWelcomeScreenCalled);
            Assert.IsFalse(factories.Settings.ShowWelcomeScreen);
        }
    }
}
