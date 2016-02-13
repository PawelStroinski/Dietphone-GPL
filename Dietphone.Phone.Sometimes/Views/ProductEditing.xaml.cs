using System;
using System.Windows;
using Dietphone.ViewModels;
using System.Windows.Navigation;
using Dietphone.Tools;
using Telerik.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Dietphone.Views
{
    public partial class ProductEditing : StateProviderPage
    {
        public new ProductEditingViewModel ViewModel { get { return (ProductEditingViewModel)base.ViewModel; } }

        public ProductEditing()
        {
            InitializeComponent();
            Save = this.GetIcon(0);
            Loaded += new RoutedEventHandler(ProductEditing_Loaded);
            TranslateApplicationBar();
        }

        protected override void OnInitializePage()
        {
            var navigator = new NavigatorImpl(new NavigationServiceImpl(NavigationService));
            ViewModel.StateProvider = this;
            ViewModel.Navigator = navigator;
            ViewModel.IsDirtyChanged += ViewModel_IsDirtyChanged;
            ViewModel.CannotSave += ViewModel_CannotSave;
            ViewModel.Load();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                ViewModel.Tombstone();
            }
        }

        private void ProductEditing_Loaded(object sender, RoutedEventArgs e)
        {
            var product = ViewModel.Subject;
            if (string.IsNullOrEmpty(product.Name))
            {
                NameBox.Focus();
            }
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            Category.IsExpanded = false;
            var input = new XnaInputBox(this)
            {
                Title = Translations.AddCategory,
                Description = Translations.Name
            };
            input.Show();
            input.Confirmed += delegate
            {
                ViewModel.AddAndSetCategory(input.Text);
                Category.ForceRefresh(ProgressBar);
            };
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            Category.IsExpanded = false;
            var input = new XnaInputBox(this)
            {
                Title = Translations.EditCategory,
                Description = Translations.Name,
                Text = ViewModel.CategoryName
            };
            input.Show();
            input.Confirmed += delegate
            {
                ViewModel.CategoryName = input.Text;
                Category.ForceRefresh(ProgressBar);
            };
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CanDeleteCategory())
            {
                DeleteCategory();
            }
            else
            {
                MessageBox.Show(Translations.ThisCategoryIncludesOtherProducts,
                    Translations.CannotDelete, MessageBoxButton.OK);
            }
        }

        private void DeleteCategory()
        {
            if (MessageBox.Show(
                String.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisCategory,
                ViewModel.CategoryName),
                Translations.DeleteCategory, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Save.IsEnabled = false;
                Category.IsExpanded = false;
                Dispatcher.BeginInvoke(() =>
                {
                    ViewModel.DeleteCategory();
                    Category.ForceRefresh(ProgressBar);
                    Save.IsEnabled = ViewModel.IsDirty;
                });
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            Focus();
            Dispatcher.BeginInvoke(() =>
            {
                if (ViewModel.CanSave())
                {
                    ViewModel.SaveAndReturn();
                }
            });
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            ViewModel.CancelAndReturn();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            var product = ViewModel.Subject;
            if (MessageBox.Show(
                String.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisProduct,
                product.Name),
                Translations.DeleteProduct, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ViewModel.DeleteAndSaveAndReturn();
            }
        }

        private void ViewModel_IsDirtyChanged(object sender, EventArgs e)
        {
            Save.IsEnabled = ViewModel.IsDirty;
        }

        private void ViewModel_CannotSave(object sender, CannotSaveEventArgs e)
        {
            e.Ignore = (MessageBox.Show(e.Reason, Translations.AreYouSureYouWantToSaveThisProduct,
                MessageBoxButton.OKCancel) == MessageBoxResult.OK);
        }

        private void Categories_ItemClick(object sender, SelectorItemClickEventArgs e)
        {
            Vibration vibration = new VibrationImpl();
            vibration.VibrateOnButtonPress();
        }

        private void Cu_Click(object sender, MouseButtonEventArgs e)
        {
            var learn = new LearningCuAndFpu();
            learn.LearnCu();
        }

        private void Fpu_Click(object sender, MouseButtonEventArgs e)
        {
            var learn = new LearningCuAndFpu();
            learn.LearnFpu();
        }

        private void TranslateApplicationBar()
        {
            Save.Text = Translations.Save;
            this.GetIcon(1).Text = Translations.Cancel;
            this.GetMenuItem(0).Text = Translations.Delete;
        }
    }
}