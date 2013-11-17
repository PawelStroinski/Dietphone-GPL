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
using System.Windows.Controls;
using System.Collections;

namespace Dietphone.Views
{
    public partial class InsulinEditing : StateProviderPage
    {
        private InsulinEditingViewModel viewModel;

        public InsulinEditing()
        {
            InitializeComponent();
            viewModel = new InsulinEditingViewModel(MyApp.Factories, CreateFacade(),
                new BackgroundWorkerWrapperFactory());
            viewModel.StateProvider = this;
            viewModel.IsDirtyChanged += ViewModel_IsDirtyChanged;
            viewModel.CannotSave += ViewModel_CannotSave;
            Save = this.GetIcon(0);
            TranslateApplicationBar();
            InsulinCircumstances.SummaryForSelectedItemsDelegate
                += InsulinCircumstancesSummaryForSelectedItemsDelegate;
            Loaded += delegate
            {
                if (viewModel.ShouldFocusSugar())
                    CurrentSugar.Focus();
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
            PopulateListPickerWithSelectedInsulinCircumstances();
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
            var input = new XnaInputBox(this)
            {
                Title = Translations.AddCircumstance,
                Description = Translations.Name
            };
            input.Show();
            input.Confirmed += delegate
            {
                viewModel.AddCircumstance(input.Text);
            };
        }

        private void EditCircumstance_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.CanEditCircumstance())
            {
                EditCircumstanceDo();
            }
            else
            {
                MessageBox.Show(Translations.SelectCircumstanceFirst);
            }
        }

        private void DeleteCircumstance_Click(object sender, RoutedEventArgs e)
        {
            var canDelete = viewModel.CanDeleteCircumstance();
            switch (canDelete)
            {
                case InsulinEditingViewModel.CanDeleteCircumstanceResult.Yes:
                    DeleteCircumstanceDo();
                    break;
                case InsulinEditingViewModel.CanDeleteCircumstanceResult.NoCircumstanceChoosen:
                    MessageBox.Show(Translations.SelectCircumstanceFirst);
                    break;
                case InsulinEditingViewModel.CanDeleteCircumstanceResult.NoThereIsOnlyOneCircumstance:
                    MessageBox.Show(Translations.CannotDeleteCircumstanceBecauseOnlyOneLeft);
                    break;
                default:
                    throw new Exception(string.Format("Unknown result {0}", canDelete));
            }
        }

        private void EditCircumstanceDo()
        {
            var input = new XnaInputBox(this)
            {
                Title = Translations.EditCircumstance,
                Description = Translations.Circumstance,
                Text = viewModel.NameOfFirstChoosenCircumstance
            };
            input.Show();
            input.Confirmed += delegate
            {
                InsulinCircumstances.QuicklyCollapse();
                viewModel.NameOfFirstChoosenCircumstance = input.Text;
                InvalidateCircumstancesListPicker();
            };
        }

        private void DeleteCircumstanceDo()
        {
            if (MessageBox.Show(
                String.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisCircumstance,
                viewModel.NameOfFirstChoosenCircumstance),
                Translations.DeleteCircumstance, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Save.IsEnabled = false;
                InsulinCircumstances.QuicklyCollapse();
                Dispatcher.BeginInvoke(() =>
                {
                    InsulinCircumstances.SummaryForSelectedItemsDelegate
                        -= InsulinCircumstancesSummaryForSelectedItemsDelegate;
                    try
                    {
                        InsulinCircumstances.SelectedItems.RemoveAt(0);
                    }
                    finally
                    {
                        InsulinCircumstances.SummaryForSelectedItemsDelegate
                            += InsulinCircumstancesSummaryForSelectedItemsDelegate;
                    }
                    viewModel.DeleteCircumstance();
                    Save.IsEnabled = viewModel.IsDirty;
                });
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

        private string InsulinCircumstancesSummaryForSelectedItemsDelegate(IList newValue)
        {
            viewModel.Subject.Circumstances = newValue.Cast<InsulinCircumstanceViewModel>().ToList();
            return viewModel.SummaryForSelectedCircumstances();
        }

        private void PopulateListPickerWithSelectedInsulinCircumstances()
        {
            foreach (var circumstance in viewModel.Subject.Circumstances.ToList())
                InsulinCircumstances.SelectedItems.Add(circumstance);
        }

        private void InvalidateCircumstancesListPicker()
        {
            InsulinCircumstances.SummaryForSelectedItemsDelegate
                -= InsulinCircumstancesSummaryForSelectedItemsDelegate;
            try
            {
                InsulinCircumstances.SelectedItems.Clear();
                viewModel.InvalidateCircumstances();
            }
            finally
            {
                InsulinCircumstances.SummaryForSelectedItemsDelegate
                    += InsulinCircumstancesSummaryForSelectedItemsDelegate;
            }
            PopulateListPickerWithSelectedInsulinCircumstances();
        }

        private ReplacementBuilderAndSugarEstimatorFacade CreateFacade()
        {
            var patternBuilder = new PatternBuilderImpl(MyApp.Factories,
                new PatternBuilderImpl.Factor(),
                new PatternBuilderImpl.PointsForPercentOfEnergy(),
                new PatternBuilderImpl.PointsForRecentMeal(),
                new PatternBuilderImpl.PointsForSimillarHour(new HourDifferenceImpl()),
                new PatternBuilderImpl.PointsForSameCircumstances(),
                new PatternBuilderImpl.PointsForSimillarSugarBefore(),
                new PatternBuilderImpl.PointsForFactorCloserToOne());
            var replacementBuilder = new ReplacementBuilderImpl(new ReplacementBuilderImpl.IsComplete(),
                new ReplacementBuilderImpl.InsulinTotal());
            var sugarEstimator = new SugarEstimatorImpl();
            var facade = new ReplacementBuilderAndSugarEstimatorFacadeImpl(patternBuilder,
                replacementBuilder, sugarEstimator);
            return facade;
        }
    }
}