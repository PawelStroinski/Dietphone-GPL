using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using Dietphone.Tools;
using System;
using Dietphone.Models.Tests;
using System.Collections.Generic;
using Dietphone.Views;
using Ploeh.AutoFixture;

namespace Dietphone.Smartphone.Tests
{
    public class MainViewModelTests : TestBase
    {
        private MainViewModel CreateSut(Factories factories = null, Cloud cloud = null,
            TimerFactory timerFactory = null, ProductListingViewModel productListing = null,
            MealItemEditingViewModel mealItemEditing = null, MessageDialog messageDialog = null,
            WelcomeScreen welcomeScreen = null, CloudMessages cloudMessages = null)
        {
            if (factories == null)
                factories = Substitute.For<Factories>();
            if (cloud == null)
                cloud = Substitute.For<Cloud>();
            if (timerFactory == null)
                timerFactory = Substitute.For<TimerFactory>();
            if (productListing == null)
                productListing = new ProductListingViewModel(factories, new BackgroundWorkerSyncFactory());
            if (mealItemEditing == null)
                mealItemEditing = new MealItemEditingViewModel();
            if (messageDialog == null)
                messageDialog = Substitute.For<MessageDialog>();
            if (welcomeScreen == null)
                welcomeScreen = new WelcomeScreenImpl(messageDialog);
            if (cloudMessages == null)
                cloudMessages = new CloudMessages();
            var journal = new JournalViewModel(factories, new BackgroundWorkerSyncFactory(),
                new SugarEditingViewModel());
            return new MainViewModel(factories, cloud, timerFactory, new BackgroundWorkerSyncFactory(),
                new MealEditingViewModel.BackNavigation(), journal, productListing, mealItemEditing, messageDialog,
                welcomeScreen, cloudMessages);
        }

        public class WhenAddingMealItem : MainViewModelTests
        {
            private Factories factories;
            private MainViewModel sut;
            private ProductListingViewModel productListing;
            private MealItemEditingViewModel mealItemEditing;
            private Navigator navigator;
            private MainViewModel.Navigation navigation;

            [SetUp]
            public void TestInitialize()
            {
                factories = new FactoriesImpl();
                factories.StorageCreator = new StorageCreatorStub();
                productListing = new ProductListingViewModel(factories,
                    new BackgroundWorkerSyncFactory());
                mealItemEditing = new MealItemEditingViewModel();
                sut = CreateSut(factories, productListing: productListing, mealItemEditing: mealItemEditing);
                navigation = new MainViewModel.Navigation();
                sut.Init(navigation);
                var stateProvider = Substitute.For<StateProvider>();
                stateProvider.State.Returns(new Dictionary<string, object>());
                sut.StateProvider = stateProvider;
                navigator = Substitute.For<Navigator>();
            }

            [TestCase(true)]
            [TestCase(false)]
            public void SetsAddMruToTrue(bool shouldAddMealItem)
            {
                navigation.ShouldAddMealItem = shouldAddMealItem;
                sut.Navigator = navigator;
                Assert.AreEqual(shouldAddMealItem, productListing.AddMru);
            }

            [Test]
            public void InitializesUnit()
            {
                navigation.ShouldAddMealItem = true;
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

        [TestCase(true)]
        [TestCase(false)]
        public void WelcomeScreen(bool confirm)
        {
            var messageDialog = Substitute.For<MessageDialog>();
            var sut = CreateSut(messageDialog: messageDialog);
            string launchedBrowserWith = null;
            var welcomeScreen = sut.WelcomeScreen;
            welcomeScreen.LaunchBrowser += (e, url) => { launchedBrowserWith = url; };
            messageDialog.Confirm(Translations.WelcomeScreenText, Translations.WelcomeScreenHeader).Returns(confirm);
            welcomeScreen.Show.Call();
            messageDialog.Received().Confirm(Translations.WelcomeScreenText, Translations.WelcomeScreenHeader);
            Assert.AreEqual(confirm ? Translations.WelcomeScreenLink : null, launchedBrowserWith);
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
            Action timerCallback = null;
            timerFactory.WhenForAnyArgs(factory => factory.Create(null, 0)).Do((args) =>
            {
                timerCallback = (Action)args[0];
                Assert.AreEqual(500, args[1]);
            });
            var factories = Substitute.For<Factories>();
            factories.Settings.Returns(new Settings());
            var messageDialog = Substitute.For<MessageDialog>();
            var cloudMessages = new Fixture().Create<CloudMessages>();
            var sut = CreateSut(factories, cloud, timerFactory, messageDialog: messageDialog,
                cloudMessages: cloudMessages);
            sut.StateProvider = Substitute.For<StateProvider>();
            Assert.IsNull(timerCallback);
            sut.UiRendered();
            if (shouldExport)
            {
                Assert.IsNotNull(timerCallback);
                cloud.DidNotReceive().Export();
                timerCallback();
                cloud.Received().Export();
                messageDialog.Received(throwInExport ? 1 : 0).Show(cloudMessages.ExportToCloudError);
            }
            else
                Assert.IsNull(timerCallback);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void UiRenderedShowsWelcomeScreen(bool showWelcomeScreen)
        {
            var factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            factories.Settings.ShowWelcomeScreen = showWelcomeScreen;
            var welcomeScreen = Substitute.For<WelcomeScreen>();
            var sut = CreateSut(factories, welcomeScreen: welcomeScreen);
            var stateProvider = Substitute.For<StateProvider>();
            stateProvider.State.Returns(new Dictionary<string, object>());
            sut.StateProvider = stateProvider;
            sut.UiRendered();
            welcomeScreen.Show.Received(showWelcomeScreen ? 1 : 0).Execute(null);
            Assert.IsFalse(factories.Settings.ShowWelcomeScreen);
        }
    }
}
