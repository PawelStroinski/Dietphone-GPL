using System;
using System.Windows;
using Dietphone.ViewModels;
using System.Windows.Navigation;
using Dietphone.Tools;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using Telerik.Windows.Controls;
using System.Windows.Threading;

namespace Dietphone.Views
{
    public partial class InsulinEditing : PageBase
    {
        private new InsulinEditingViewModel ViewModel { get { return (InsulinEditingViewModel)base.ViewModel; } }
        private const int CHART_PADDING_TOP = 51;

        public InsulinEditing()
        {
            InitializeComponent();
        }

        protected override void OnInitializePage()
        {
            ViewModel.IsDirtyChanged += ViewModel_IsDirtyChanged;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.CircumstanceEdit += ViewModel_CircumstanceEdit;
            ViewModel.CircumstanceDelete += ViewModel_CircumstanceDelete;
            MealScores.ScoreClick += MealScores_ScoreClick;
            Save = this.GetIcon(0);
            TranslateApplicationBar();
            InsulinCircumstances.SummaryForSelectedItemsDelegate
                += InsulinCircumstancesSummaryForSelectedItemsDelegate;
            Loaded += delegate
            {
                if (ViewModel.ShouldFocusSugar())
                    CurrentSugar.Focus();
                RestoreCalculationDetailsPickers();
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ViewModel.Navigator == null)
            {
                var navigator = new NavigatorImpl(new NavigationServiceImpl(NavigationService));
                ViewModel.Navigator = navigator;
                ViewModel.Load();
                MealScores.DataContext = ViewModel.MealScores;
            }
            else
            {
                ViewModel.ReturnedFromNavigation();
                RestoreCalculationDetailsPickers();
            }
            PopulateListPickerWithSelectedInsulinCircumstances();
            InteractionEffectManager.AllowedTypes.Remove(typeof(RadDataBoundListBoxItem));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            CalculationDetailsAlternativesPicker.IsPopupAnimationEnabled = false;
            CalculationDetailsAlternativesPicker.IsPopupOpen = false;
            CalculationDetailsPicker.IsPopupAnimationEnabled = false;
            CalculationDetailsPicker.IsPopupOpen = false;
            if (e.NavigationMode != NavigationMode.Back)
            {
                ViewModel.Tombstone();
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (ViewModel.CalculationDetailsVisible || ViewModel.CalculationDetailsAlternativesVisible)
            {
                ViewModel.CloseCalculationDetailsOrAlternativesOnBackButton();
                e.Cancel = true;
            }
            base.OnBackKeyPress(e);
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

        private void Meal_Click(object sender, EventArgs e)
        {
            ViewModel.GoToMealEditing();
        }

        private void CopyAsText_Click(object sender, EventArgs e)
        {
            ViewModel.CopyAsText();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            ViewModel.DeleteAndSaveAndReturn();
        }

        private void ViewModel_IsDirtyChanged(object sender, EventArgs e)
        {
            Save.IsEnabled = ViewModel.IsDirty;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CalculationDetailsVisible")
                CalculationDetailsPicker.IsPopupOpen = ViewModel.CalculationDetailsVisible;
            if (e.PropertyName == "CalculationDetailsAlternativesVisible")
                CalculationDetailsAlternativesPicker.IsPopupOpen = ViewModel.CalculationDetailsAlternativesVisible;
        }

        private void ViewModel_CircumstanceEdit(object sender, Action action)
        {
            InsulinCircumstances.QuicklyCollapse();
            action();
            InvalidateCircumstancesListPicker();
        }

        private void ViewModel_CircumstanceDelete(object sender, Action action)
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
                action();
                Save.IsEnabled = ViewModel.IsDirty;
            });
        }

        private void MealScores_ScoreClick(object sender, EventArgs e)
        {
            ViewModel.OpenScoresSettings();
        }

        private void Chart_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var position = e.GetPosition(Chart);
            if (position.Y > CHART_PADDING_TOP)
                ViewModel.ShowSugarChartAsText.Execute(null);
        }

        private void CalculationIncomplete_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ViewModel.ShowListOfMealItemsNotIncludedInCalculation.Execute(null);
        }

        private void CloseCalculationDetails_Click(object sender, EventArgs e)
        {
            ViewModel.CloseCalculationDetails();
        }

        private void CloseCalculationDetailsAlternatives_Click(object sender, EventArgs e)
        {
            ViewModel.CloseCalculationDetailsAlternatives();
        }

        private void TranslateApplicationBar()
        {
            Save.Text = Translations.Save;
            this.GetIcon(1).Text = Translations.Cancel;
            this.GetIcon(2).Text = Translations.Meal;
            this.GetMenuItem(0).Text = Translations.Copy;
            this.GetMenuItem(1).Text = Translations.Delete;
            TranslatePickerApplicationBar(CalculationDetailsPicker);
            TranslatePickerApplicationBar(CalculationDetailsAlternativesPicker);
        }

        private void TranslatePickerApplicationBar(RadPickerBox picker)
        {
            var pickerApplicationBar = picker.ApplicationBarInfo;
            var close = pickerApplicationBar.Buttons[0];
            close.Text = Translations.Close;
        }

        private void RestoreCalculationDetailsPickers()
        {
            if (ViewModel.CalculationDetailsVisible || ViewModel.CalculationDetailsAlternativesVisible)
                Dispatcher.BeginInvoke(() =>
                {
                    CalculationDetailsPicker.IsPopupOpen
                        = ViewModel.CalculationDetailsVisible;
                    DispatchWithDelay(TimeSpan.FromSeconds(0.5), () =>
                        CalculationDetailsAlternativesPicker.IsPopupOpen
                            = ViewModel.CalculationDetailsAlternativesVisible);
                    DispatchWithDelay(TimeSpan.FromSeconds(5), () =>
                    {
                        CalculationDetailsPicker.IsPopupAnimationEnabled = true;
                        CalculationDetailsAlternativesPicker.IsPopupAnimationEnabled = true;
                    });
                });
        }

        private void DispatchWithDelay(TimeSpan delay, Action action)
        {
            var timer = new DispatcherTimer();
            timer.Interval = delay;
            timer.Tick += (_, __) =>
            {
                timer.Stop();
                action();
            };
            timer.Start();
        }

        private string InsulinCircumstancesSummaryForSelectedItemsDelegate(IList newValue)
        {
            ViewModel.Subject.Circumstances = newValue.Cast<InsulinCircumstanceViewModel>().ToList();
            return ViewModel.Subject.CircumstancesSummary;
        }

        private void PopulateListPickerWithSelectedInsulinCircumstances()
        {
            foreach (var circumstance in ViewModel.Subject.Circumstances.ToList())
                InsulinCircumstances.SelectedItems.Add(circumstance);
        }

        private void InvalidateCircumstancesListPicker()
        {
            InsulinCircumstances.SummaryForSelectedItemsDelegate
                -= InsulinCircumstancesSummaryForSelectedItemsDelegate;
            try
            {
                InsulinCircumstances.SelectedItems.Clear();
                ViewModel.InvalidateCircumstances();
            }
            finally
            {
                InsulinCircumstances.SummaryForSelectedItemsDelegate
                    += InsulinCircumstancesSummaryForSelectedItemsDelegate;
            }
            PopulateListPickerWithSelectedInsulinCircumstances();
        }
    }
}