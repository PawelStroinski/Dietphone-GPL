using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Dietphone.ViewModels;
using System.Windows.Navigation;
using Dietphone.Tools;
using Telerik.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Collections.Generic;
using Dietphone.Models;
using System.Linq;
using System.Collections.ObjectModel;

namespace Dietphone.Views
{
    public partial class InsulinEditing : StateProviderPage
    {
        private InsulinEditingViewModel viewModel;

        public InsulinEditing()
        {
            InitializeComponent();
            viewModel = new InsulinEditingViewModel(MyApp.Factories);
            viewModel.StateProvider = this;
            viewModel.IsDirtyChanged += ViewModel_IsDirtyChanged;
            viewModel.CannotSave += ViewModel_CannotSave;
            Chart.Series[0].ItemsSource = new ChartDataObject[]
            {
                new ChartDataObject(DateTime.Now, 100),
                new ChartDataObject(DateTime.Now.AddMinutes(60), 150),
                new ChartDataObject(DateTime.Now.AddMinutes(120), 90),
                new ChartDataObject(DateTime.Now.AddMinutes(180), 120)
            };
            Save = this.GetIcon(0);
            TranslateApplicationBar();
            InsulinCircumstances.SummaryForSelectedItemsDelegate += (newValue) =>
            {
                //viewModel.Subject.Circumstances 
                //    = new ObservableCollection<InsulinCircumstanceViewModel>(
                //        newValue.Cast<InsulinCircumstanceViewModel>());
                return "";
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (viewModel.Navigator == null)
            {
                var navigator = new NavigatorImpl(new NavigationServiceImpl(NavigationService),
                    new NavigationContextImpl(NavigationContext));
                viewModel.Navigator = navigator;
                viewModel.Load();
                DataContext = viewModel;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                viewModel.Tombstone();
            }
        }

        private void AddCircumstance_Click(object sender, RoutedEventArgs e)
        {
        }

        private void EditCircumstance_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DeleteCircumstance_Click(object sender, RoutedEventArgs e)
        {
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

    public class ChartDataObject
    {
        public ChartDataObject(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }

        public DateTime Date
        {
            get;
            set;
        }
        public double Value
        {
            get;
            set;
        }
    }
}