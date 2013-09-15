using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Dietphone.ViewModels;
using System.Windows.Navigation;
using Dietphone.Tools;
using Telerik.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Collections.Generic;

namespace Dietphone.Views
{
    public partial class InsulinEditing : StateProviderPage
    {
        private InsulinEditingViewModel viewModel;

        public InsulinEditing()
        {
            InitializeComponent();
            Save = this.GetIcon(0);
            TranslateApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var navigator = new NavigatorImpl(NavigationService, NavigationContext);
            viewModel = new InsulinEditingViewModel(MyApp.Factories);
            viewModel.StateProvider = this;
            viewModel.Navigator = navigator;
            viewModel.IsDirtyChanged += ViewModel_IsDirtyChanged;
            viewModel.CannotSave += ViewModel_CannotSave;
            viewModel.Load();
            DataContext = viewModel;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                viewModel.Tombstone();
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
        }

        private void Delete_Click(object sender, EventArgs e)
        {
        }

        private void ViewModel_IsDirtyChanged(object sender, EventArgs e)
        {
            Save.IsEnabled = viewModel.IsDirty;
        }

        private void ViewModel_CannotSave(object sender, CannotSaveEventArgs e)
        {
            e.Ignore = (MessageBox.Show(e.Reason, Translations.AreYouSureYouWantToSaveThisInsulin,
                MessageBoxButton.OKCancel) == MessageBoxResult.OK);
        }

        private void TranslateApplicationBar()
        {
            Save.Text = Translations.Save;
            this.GetIcon(1).Text = Translations.Cancel;
            this.GetMenuItem(0).Text = Translations.Delete;
        }
    }
}