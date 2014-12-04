using System;
using Dietphone.Models;
using Dietphone.Tools;
using System.Threading;

namespace Dietphone.ViewModels
{
    public class MainViewModel : PivotTombstoningViewModel
    {
        public ProductListingViewModel ProductListing { private get; set; }
        public MealItemEditingViewModel MealItemEditing { private get; set; }
        public MealEditingViewModel MealEditing { private get; set; }
        public event EventHandler ShowProductsOnly;
        public event EventHandler ExportToCloudError;
        private string search = string.Empty;
        private Navigator navigator;
        private MealItem tempMealItem;
        private bool addMealItem;
        private readonly Factories factories;
        private readonly Cloud cloud;
        private readonly TimerFactory timerFactory;
        private readonly BackgroundWorkerFactory workerFactory;
        private const string MEAL_ITEM_EDITING = "MEAL_ITEM_EDITING";
        private const string MEAL_ITEM_PRODUCT = "MEAL_ITEM_PRODUCT";

        public MainViewModel(Factories factories, Cloud cloud, TimerFactory timerFactory,
            BackgroundWorkerFactory workerFactory)
        {
            this.factories = factories;
            this.cloud = cloud;
            this.timerFactory = timerFactory;
            this.workerFactory = workerFactory;
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

        public void GoingToMealEditing()
        {
            if (addMealItem)
            {
                MealEditing.AddCopyOfThisItem = tempMealItem;
            }
        }

        public void About()
        {
            navigator.GoToAbout();
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
        }

        protected void OnNavigatorChanged()
        {
            if (navigator.ShouldAddMealItem())
            {
                AddingMealItem();
            }
        }

        private void CreateTimerToExportToCloud()
        {
            Timer timer = null;
            timer = timerFactory.Create(callback: _ =>
            {
                var worker = workerFactory.Create();
                worker.DoWork += delegate { ExportToCloud(); };
                worker.RunWorkerCompleted += delegate { timer.Dispose(); };
                worker.RunWorkerAsync();
            }, state: null, dueTime: 500, period: -1);
        }

        private void AddingMealItem()
        {
            ProductListing.Choosed -= ProductListing_Choosed;
            ProductListing.Choosed += ProductListing_Choosed;
            ProductListing.AddMru = true;
            MealItemEditing.Confirmed -= MealItemEditing_Confirmed;
            MealItemEditing.Confirmed += MealItemEditing_Confirmed;
            MealItemEditing.StateProvider = StateProvider;
            OnShowProductsOnly();
        }

        private void TombstoneMealItemEditing()
        {
            var state = StateProvider.State;
            state[MEAL_ITEM_EDITING] = MealItemEditing.IsVisible;
            if (MealItemEditing.IsVisible)
            {
                var mealItem = MealItemEditing.Subject;
                state[MEAL_ITEM_PRODUCT] = mealItem.ProductId;
                MealItemEditing.Tombstone();
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
                var products = ProductListing.Products;
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
                OnExportToCloudError();
            }
        }

        private void ProductListing_Choosed(object sender, ChoosedEventArgs e)
        {
            AddMealItemWithProduct(e.Product);
            e.Handled = true;
        }

        private void MealItemEditing_Confirmed(object sender, EventArgs e)
        {
            addMealItem = true;
            navigator.GoBack();
        }

        private void AddMealItemWithProduct(ProductViewModel product)
        {
            tempMealItem = factories.CreateMealItem();
            tempMealItem.ProductId = product.Id;
            var tempViewModel = new MealItemViewModel(tempMealItem, factories);
            MealItemEditing.Show(tempViewModel);
        }

        protected void OnShowProductsOnly()
        {
            if (ShowProductsOnly != null)
            {
                ShowProductsOnly(this, EventArgs.Empty);
            }
        }

        protected void OnExportToCloudError()
        {
            if (ExportToCloudError != null)
            {
                ExportToCloudError(this, EventArgs.Empty);
            }
        }
    }
}