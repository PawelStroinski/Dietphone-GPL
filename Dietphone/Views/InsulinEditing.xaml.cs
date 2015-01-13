using System;
using System.Windows;
using Dietphone.ViewModels;
using System.Windows.Navigation;
using Dietphone.Tools;
using Dietphone.Models;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using Telerik.Windows.Controls;
using System.Windows.Threading;

namespace Dietphone.Views
{
    public partial class InsulinEditing : StateProviderPage
    {
        private const int CHART_PADDING_TOP = 51;
        private InsulinEditingViewModel viewModel;

        public InsulinEditing()
        {
            InitializeComponent();
            viewModel = new InsulinEditingViewModel(MyApp.Factories, CreateFacade(),
                new BackgroundWorkerWrapperFactory());
            viewModel.StateProvider = this;
            viewModel.IsDirtyChanged += ViewModel_IsDirtyChanged;
            viewModel.CannotSave += ViewModel_CannotSave;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            MealScores.ScoreClick += MealScores_ScoreClick;
            InteractionEffectManager.AllowedTypes.Remove(typeof(RadDataBoundListBoxItem));
            Save = this.GetIcon(0);
            TranslateApplicationBar();
            InsulinCircumstances.SummaryForSelectedItemsDelegate
                += InsulinCircumstancesSummaryForSelectedItemsDelegate;
            Loaded += delegate
            {
                if (viewModel.ShouldFocusSugar())
                    CurrentSugar.Focus();
                Dispatcher.BeginInvoke(() =>
                {
                    CalculationDetailsPicker.IsPopupOpen = viewModel.CalculationDetailsVisible;
                    Dispatcher.BeginInvoke(() =>
                    {
                        CalculationDetailsAlternativesPicker.IsPopupOpen
                            = viewModel.CalculationDetailsAlternativesVisible;
                        CalculationDetailsPicker.IsPopupAnimationEnabled = true;
                        CalculationDetailsAlternativesPicker.IsPopupAnimationEnabled = true;
                    });
                });
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
                MealScores.DataContext = viewModel.MealScores;
                DataContext = viewModel;
            }
            else
            {
                viewModel.ReturnedFromNavigation();
            }
            PopulateListPickerWithSelectedInsulinCircumstances();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            CalculationDetailsAlternativesPicker.IsPopupAnimationEnabled = false;
            CalculationDetailsAlternativesPicker.IsPopupOpen = false;
            CalculationDetailsPicker.IsPopupAnimationEnabled = false;
            CalculationDetailsPicker.IsPopupOpen = false;
            if (e.NavigationMode != NavigationMode.Back)
            {
                viewModel.Tombstone();
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (viewModel.CalculationDetailsVisible || viewModel.CalculationDetailsAlternativesVisible)
            {
                viewModel.CloseCalculationDetailsÓrAlternativesOnBackButton();
                e.Cancel = true;
            }
            base.OnBackKeyPress(e);
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
            Focus();
            Dispatcher.BeginInvoke(() =>
            {
                if (viewModel.CanSave())
                {
                    viewModel.SaveWithUpdatedTimeAndReturn();
                }
            });
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            viewModel.CancelAndReturn();
        }

        private void Meal_Click(object sender, EventArgs e)
        {
            viewModel.GoToMealEditing();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                String.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisInsulin,
                viewModel.Subject.DateAndTime),
                Translations.DeleteInsulin, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                viewModel.DeleteAndSaveAndReturn();
            }
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

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CalculationDetailsVisible")
                CalculationDetailsPicker.IsPopupOpen = viewModel.CalculationDetailsVisible;
            if (e.PropertyName == "CalculationDetailsAlternativesVisible")
                CalculationDetailsAlternativesPicker.IsPopupOpen = viewModel.CalculationDetailsAlternativesVisible;
        }

        private void MealScores_ScoreClick(object sender, EventArgs e)
        {
            viewModel.OpenScoresSettings();
        }

        private void Chart_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var position = e.GetPosition(Chart);
            if (position.Y > CHART_PADDING_TOP)
                MessageBox.Show(viewModel.SugarChartAsText);
        }

        private void CalculationIncomplete_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        private void UseCalculation_Tap(object sender, RoutedEventArgs e)
        {

        }

        private void CalculationDetails_Tap(object sender, RoutedEventArgs e)
        {
            viewModel.CalculationDetails();
        }

        private void CloseCalculationDetails_Click(object sender, EventArgs e)
        {
            viewModel.CloseCalculationDetails();
        }

        private void CloseCalculationDetailsAlternatives_Click(object sender, EventArgs e)
        {
            viewModel.CloseCalculationDetailsAlternatives();
        }

        private void TranslateApplicationBar()
        {
            Save.Text = Translations.Save;
            this.GetIcon(1).Text = Translations.Cancel;
            this.GetIcon(2).Text = Translations.Meal;
            this.GetMenuItem(0).Text = Translations.Delete;
            TranslatePickerApplicationBar(CalculationDetailsPicker);
            TranslatePickerApplicationBar(CalculationDetailsAlternativesPicker);
        }

        private void TranslatePickerApplicationBar(RadPickerBox picker)
        {
            var pickerApplicationBar = picker.ApplicationBarInfo;
            var close = pickerApplicationBar.Buttons[0];
            close.Text = Translations.Close;
        }

        private string InsulinCircumstancesSummaryForSelectedItemsDelegate(IList newValue)
        {
            viewModel.Subject.Circumstances = newValue.Cast<InsulinCircumstanceViewModel>().ToList();
            return viewModel.Subject.CircumstancesSummary;
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