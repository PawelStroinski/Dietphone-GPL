using System;
using System.Windows.Input;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.Views;
using MvvmCross.Core.ViewModels;

namespace Dietphone.ViewModels
{
    public class MainViewModel : PivotTombstoningViewModel
    {
        public JournalViewModel Journal { get { return journal; } }
        public ProductListingViewModel ProductListing { get { return productListing; } }
        public MealItemEditingViewModel MealItemEditing { get { return mealItemEditing; } }
        public WelcomeScreen WelcomeScreen { get; private set; }
        public event EventHandler ShowProductsOnly;
        private string search = string.Empty;
        private Navigator navigator;
        private Navigation navigation;
        private MealItem tempMealItem;
        private readonly Factories factories;
        private readonly Cloud cloud;
        private readonly TimerFactory timerFactory;
        private readonly BackgroundWorkerFactory workerFactory;
        private readonly MealEditingViewModel.BackNavigation mealEditingBackNavigation;
        private readonly JournalViewModel journal;
        private readonly ProductListingViewModel productListing;
        private readonly MealItemEditingViewModel mealItemEditing;
        private readonly MessageDialog messageDialog;
        private readonly CloudMessages cloudMessages;
        private const string MEAL_ITEM_EDITING = "MEAL_ITEM_EDITING";
        private const string MEAL_ITEM_PRODUCT = "MEAL_ITEM_PRODUCT";

        public MainViewModel(Factories factories, Cloud cloud, TimerFactory timerFactory,
            BackgroundWorkerFactory workerFactory, MealEditingViewModel.BackNavigation mealEditingBackNavigation,
            JournalViewModel journal, ProductListingViewModel productListing, MealItemEditingViewModel mealItemEditing,
            MessageDialog messageDialog, WelcomeScreen welcomeScreen, CloudMessages cloudMessages)
        {
            this.factories = factories;
            this.cloud = cloud;
            this.timerFactory = timerFactory;
            this.workerFactory = workerFactory;
            this.mealEditingBackNavigation = mealEditingBackNavigation;
            this.journal = journal;
            this.productListing = productListing;
            this.mealItemEditing = mealItemEditing;
            this.messageDialog = messageDialog;
            this.cloudMessages = cloudMessages;
            WelcomeScreen = welcomeScreen;
            ShareStateProvider();
        }

        public string Search
        {
            get
            {
                return search;
            }
            set
            {
                if (search != value)
                {
                    search = value;
                    OnPropertyChanged("Search");
                }
            }
        }

        public Navigator Navigator
        {
            set
            {
                navigator = value;
                OnNavigatorChanged();
            }
        }

        public void Init(Navigation navigation)
        {
            this.navigation = navigation;
        }

        public void About()
        {
            navigator.GoToAbout();
        }

        public void EmbeddedAbout()
        {
            navigator.GoToEmbeddedAbout();
        }

        public void ExportAndImport()
        {
            navigator.GoToExportAndImport();
        }

        public void Settings()
        {
            navigator.GoToSettings();
        }

        public override void Tombstone()
        {
            base.Tombstone();
            TombstoneMealItemEditing();
        }

        public void UiRendered()
        {
            UntombstoneMealItemEditing();
            if (cloud.ShouldExport())
                CreateTimerToExportToCloud();
            HandleShowWelcomeScreen();
        }

        protected void OnNavigatorChanged()
        {
            if (navigation.ShouldAddMealItem)
            {
                AddingMealItem();
            }
        }

        private void ShareStateProvider()
        {
            journal.StateProvider = StateProvider;
            mealItemEditing.StateProvider = StateProvider;
        }

        private void CreateTimerToExportToCloud()
        {
            timerFactory.Create(callback: () =>
            {
                var worker = workerFactory.Create();
                worker.DoWork += delegate { ExportToCloud(); };
                worker.RunWorkerAsync();
            }, dueTime: 500);
        }

        private void HandleShowWelcomeScreen()
        {
            var settings = factories.Settings;
            if (settings.ShowWelcomeScreen)
            {
                WelcomeScreen.Show.Execute(null);
                settings.ShowWelcomeScreen = false;
            }
        }

        private void AddingMealItem()
        {
            productListing.Choosed -= ProductListing_Choosed;
            productListing.Choosed += ProductListing_Choosed;
            productListing.AddMru = true;
            mealItemEditing.Confirmed -= MealItemEditing_Confirmed;
            mealItemEditing.Confirmed += MealItemEditing_Confirmed;
            mealItemEditing.StateProvider = StateProvider;
            OnShowProductsOnly();
        }

        private void TombstoneMealItemEditing()
        {
            var state = StateProvider.State;
            state[MEAL_ITEM_EDITING] = mealItemEditing.IsVisible;
            if (mealItemEditing.IsVisible)
            {
                var mealItem = mealItemEditing.Subject;
                state[MEAL_ITEM_PRODUCT] = mealItem.ProductId;
                mealItemEditing.Tombstone();
            }
        }

        private void UntombstoneMealItemEditing()
        {
            var state = StateProvider.State;
            var mealItemEditing = false;
            if (state.ContainsKey(MEAL_ITEM_EDITING))
            {
                mealItemEditing = (bool)state[MEAL_ITEM_EDITING];
            }
            if (mealItemEditing)
            {
                var productId = (Guid)state[MEAL_ITEM_PRODUCT];
                var products = productListing.Products;
                var product = products.FindById(productId);
                if (product != null)
                {
                    AddMealItemWithProduct(product);
                }
            }
        }

        private void ExportToCloud()
        {
            try
            {
                cloud.Export();
            }
            catch (Exception)
            {
                messageDialog.Show(cloudMessages.ExportToCloudError);
            }
        }

        private void ProductListing_Choosed(object sender, ChoosedEventArgs e)
        {
            AddMealItemWithProduct(e.Product);
            e.Handled = true;
        }

        private void MealItemEditing_Confirmed(object sender, EventArgs e)
        {
            mealEditingBackNavigation.AddCopyOfThisItem = tempMealItem;
            navigator.GoBack();
        }

        private void AddMealItemWithProduct(ProductViewModel product)
        {
            tempMealItem = factories.CreateMealItem();
            tempMealItem.ProductId = product.Id;
            var tempViewModel = new MealItemViewModel(tempMealItem, factories);
            tempViewModel.InitializeUnit();
            mealItemEditing.Show(tempViewModel);
        }

        protected void OnShowProductsOnly()
        {
            if (ShowProductsOnly != null)
            {
                ShowProductsOnly(this, EventArgs.Empty);
            }
        }

        public class Navigation
        {
            public bool ShouldAddMealItem { get; set; }
        }
    }

    public abstract class WelcomeScreen
    {
        public event EventHandler LaunchBrowser;

        public abstract ICommand Show { get; }

        protected void OnLaunchBrowser()
        {
            if (LaunchBrowser != null)
            {
                LaunchBrowser(this, EventArgs.Empty);
            }
        }
    }

    public class WelcomeScreenImpl : WelcomeScreen
    {
        private readonly MessageDialog messageDialog;

        public WelcomeScreenImpl(MessageDialog messageDialog)
        {
            this.messageDialog = messageDialog;
        }

        public override ICommand Show
        {
            get
            {
                return new MvxCommand(() =>
                {
                    if (messageDialog.Confirm(Translations.WelcomeScreenText, Translations.WelcomeScreenHeader))
                    {
                        OnLaunchBrowser();
                    }
                });
            }
        }
    }
}