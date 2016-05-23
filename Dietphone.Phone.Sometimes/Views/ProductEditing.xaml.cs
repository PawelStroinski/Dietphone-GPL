using System;
using System.Windows;
using Dietphone.ViewModels;
using System.Windows.Navigation;
using Dietphone.Tools;
using Telerik.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Dietphone.Views
{
    public partial class ProductEditing : PageBase
    {
        private new ProductEditingViewModel ViewModel { get { return (ProductEditingViewModel)base.ViewModel; } }

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
            ViewModel.Navigator = navigator;
            ViewModel.IsDirtyChanged += ViewModel_IsDirtyChanged;
            ViewModel.BeforeAddingEditingCategory += ViewModel_BeforeAddingEditingCategory;
            ViewModel.AfterAddedEditedCategory += ViewModel_AfterAddedEditedCategory;
            ViewModel.CategoryDelete += ViewModel_CategoryDelete;
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
            if (ViewModel.ShouldFocusName)
            {
                NameBox.Focus();
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            Focus();
            Dispatcher.BeginInvoke(() =>
            {
                ViewModel.SaveAndReturn();
            });
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            ViewModel.CancelAndReturn();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            ViewModel.DeleteAndSaveAndReturn();
        }

        private void ViewModel_IsDirtyChanged(object sender, EventArgs e)
        {
            Save.IsEnabled = ViewModel.IsDirty;
        }

        private void ViewModel_BeforeAddingEditingCategory(object sender, EventArgs e)
        {
            Category.IsExpanded = false;
        }

        private void ViewModel_AfterAddedEditedCategory(object sender, EventArgs e)
        {
            Category.ForceRefresh(ProgressBar);
        }

        private void ViewModel_CategoryDelete(object sender, Action action)
        {
            Save.IsEnabled = false;
            Category.IsExpanded = false;
            Dispatcher.BeginInvoke(() =>
            {
                action();
                Category.ForceRefresh(ProgressBar);
                Save.IsEnabled = ViewModel.IsDirty;
            });
        }

        private void Categories_ItemClick(object sender, SelectorItemClickEventArgs e)
        {
            Vibration vibration = new VibrationImpl();
            vibration.VibrateOnButtonPress();
        }

        private void Cu_Click(object sender, MouseButtonEventArgs e)
        {
            ViewModel.LearnCu.Execute(null);
        }

        private void Fpu_Click(object sender, MouseButtonEventArgs e)
        {
            ViewModel.LearnFpu.Execute(null);
        }

        private void TranslateApplicationBar()
        {
            Save.Text = Translations.Save;
            this.GetIcon(1).Text = Translations.Cancel;
            this.GetMenuItem(0).Text = Translations.Delete;
        }
    }
}