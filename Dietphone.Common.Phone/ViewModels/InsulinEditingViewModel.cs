using Dietphone.Models;
using Dietphone.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using Dietphone.Views;

namespace Dietphone.ViewModels
{
    public class InsulinEditingViewModel : EditingViewModelWithDate<Insulin, InsulinViewModel>
    {
        public ObservableCollection<InsulinCircumstanceViewModel> Circumstances { get; private set; }
        public SugarViewModel CurrentSugar { get; private set; }
        public ObservableCollection<SugarChartItemViewModel> SugarChart { get; private set; }
        private List<InsulinCircumstanceViewModel> addedCircumstances = new List<InsulinCircumstanceViewModel>();
        private List<InsulinCircumstanceViewModel> deletedCircumstances = new List<InsulinCircumstanceViewModel>();
        private Sugar sugarSource;
        private Sugar sugarCopy;
        private bool isBusy;
        private bool isCalculated;
        private string isCalculatedText;
        private bool openedWithNoBolus;
        private bool bolusEdited;
        private bool sugarIsNew;
        private Meal meal;
        private readonly ReplacementBuilderAndSugarEstimatorFacade facade;
        private readonly BackgroundWorkerFactory workerFactory;
        private const float SUGAR_CHART_MARGIN_MINIMUM_MGDL = 10f;
        private const float SUGAR_CHART_MARGIN_MAXIMUM_MGDL = 50f;
        private const float SUGAR_CHART_MARGIN_MINIMUM_MMOLL = 0.55f;
        private const float SUGAR_CHART_MARGIN_MAXIMUM_MMOLL = 2.77f;

        public InsulinEditingViewModel(Factories factories, ReplacementBuilderAndSugarEstimatorFacade facade,
            BackgroundWorkerFactory workerFactory)
            : base(factories)
        {
            this.facade = facade;
            this.IsCalculatedText = string.Empty;
            this.workerFactory = workerFactory;
        }

        public string NameOfFirstChoosenCircumstance
        {
            get
            {
                return Subject.Circumstances.Any() ? Subject.Circumstances.First().Name : string.Empty;
            }
            set
            {
                if (!Subject.Circumstances.Any())
                    throw new InvalidOperationException("No insulin circumstance choosen.");
                Subject.Circumstances.First().Name = value;
            }
        }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public bool IsCalculated
        {
            get
            {
                return isCalculated;
            }
            private set
            {
                isCalculated = value;
                OnPropertyChanged("IsCalculated");
            }
        }

        public string IsCalculatedText
        {
            get
            {
                return isCalculatedText;
            }
            private set
            {
                isCalculatedText = value;
                OnPropertyChanged("IsCalculatedText");
            }
        }

        public float SugarChartMinimum
        {
            get
            {
                var margin = factories.Settings.SugarUnit == SugarUnit.mgdL
                    ? SUGAR_CHART_MARGIN_MINIMUM_MGDL : SUGAR_CHART_MARGIN_MINIMUM_MMOLL;
                return SugarChart.Any() ? SugarChart.Min(sugar => sugar.BloodSugar) - margin : 100;
            }
        }

        public float SugarChartMaximum
        {
            get
            {
                var margin = factories.Settings.SugarUnit == SugarUnit.mgdL
                    ? SUGAR_CHART_MARGIN_MAXIMUM_MGDL : SUGAR_CHART_MARGIN_MAXIMUM_MMOLL;
                return SugarChart.Any() ? SugarChart.Max(sugar => sugar.BloodSugar) + margin : 100;
            }
        }

        public void AddCircumstance(string name)
        {
            var tempModel = factories.CreateInsulinCircumstance();
            var models = factories.InsulinCircumstances;
            models.Remove(tempModel);
            var viewModel = new InsulinCircumstanceViewModel(tempModel, factories);
            viewModel.Name = name;
            Circumstances.Add(viewModel);
            addedCircumstances.Add(viewModel);
        }

        public bool CanEditCircumstance()
        {
            return Subject.Circumstances.Any();
        }

        public CanDeleteCircumstanceResult CanDeleteCircumstance()
        {
            if (Circumstances.Count < 2)
                return CanDeleteCircumstanceResult.NoThereIsOnlyOneCircumstance;
            if (!Subject.Circumstances.Any())
                return CanDeleteCircumstanceResult.NoCircumstanceChoosen;
            return CanDeleteCircumstanceResult.Yes;
        }

        public void DeleteCircumstance()
        {
            var toDelete = Subject.Circumstances.First();
            var choosenViewModels = Subject.Circumstances.ToList();
            choosenViewModels.Remove(toDelete);
            Subject.Circumstances = choosenViewModels;
            Circumstances.Remove(toDelete);
            deletedCircumstances.Add(toDelete);
        }

        public void SaveWithUpdatedTimeAndReturn()
        {
            SaveWithUpdatedTime();
            Navigator.GoBack();
        }

        public string SummaryForSelectedCircumstances()
        {
            return string.Join(", ",
                Subject.Circumstances.Select(circumstance => circumstance.Name));
        }

        public void InvalidateCircumstances()
        {
            var newCircumstances = new ObservableCollection<InsulinCircumstanceViewModel>();
            foreach (var circumstance in Circumstances)
            {
                var newCircumstance = new InsulinCircumstanceViewModel(circumstance.Model, factories);
                newCircumstance.MakeBuffer();
                newCircumstance.CopyFrom(circumstance);
                newCircumstances.Add(newCircumstance);
            }
            Circumstances = newCircumstances;
            Subject.InvalidateCircumstances(Circumstances);
            OnPropertyChanged("Circumstances");
        }

        public bool ShouldFocusSugar()
        {
            return sugarIsNew;
        }

        protected override void FindAndCopyModel()
        {
            var id = Navigator.GetInsulinIdToEdit();
            if (id == Guid.Empty)
                modelSource = factories.CreateInsulin();
            else
                modelSource = finder.FindInsulinById(id);
            modelCopy = modelSource.GetCopy();
            modelCopy.SetOwner(factories);
            modelCopy.InitializeCircumstances(modelSource.ReadCircumstances().ToList());
        }

        protected override void MakeViewModel()
        {
            LoadCircumstances();
            MakeInsulinViewModelInternal();
            MakeSugarViewModel();
            base.MakeViewModel();
        }

        protected override string Validate()
        {
            return string.Empty;
        }

        protected override void TombstoneOtherThings()
        {
        }

        protected override void UntombstoneOtherThings()
        {
        }

        protected override void OnModelReady()
        {
            openedWithNoBolus = modelSource.NormalBolus == 0 && modelSource.SquareWaveBolus == 0;
            var relatedMealId = Navigator.GetRelatedMealId();
            if (relatedMealId == Guid.Empty)
                meal = finder.FindMealByInsulin(modelSource);
            else
                meal = finder.FindMealById(relatedMealId);
        }

        protected override void OnCommonUiReady()
        {
            SugarChart = new ObservableCollection<SugarChartItemViewModel>();
        }

        private void SaveWithUpdatedTime()
        {
            UpdateLockedDateTime();
            modelSource.CopyFrom(modelCopy);
            modelSource.CopyCircumstancesFrom(modelCopy);
            sugarSource.CopyFrom(sugarCopy);
            sugarSource.DateTime = modelSource.DateTime;
        }

        private void LoadCircumstances()
        {
            var loader = new InsulinListingViewModel.CircumstancesAndInsulinsLoader(factories, true);
            Circumstances = loader.Circumstances;
            foreach (var circumstance in Circumstances)
                circumstance.MakeBuffer();
        }

        private void MakeInsulinViewModelInternal()
        {
            Subject = new InsulinViewModel(modelCopy, factories, allCircumstances: Circumstances);
            Subject.PropertyChanged += (_, eventArguments) =>
            {
                IsDirty = true;
                if (eventArguments.PropertyName == "Circumstances")
                    StartCalculation();
                if (new string[] { "NormalBolus", "SquareWaveBolus", "SquareWaveBolusHours" }
                    .Contains(eventArguments.PropertyName))
                {
                    BolusChanged();
                }
            };
        }

        private void MakeSugarViewModel()
        {
            sugarSource = finder.FindSugarBeforeInsulin(modelSource);
            sugarIsNew = sugarSource == null;
            if (sugarIsNew)
            {
                sugarSource = factories.CreateSugar();
                sugarSource.DateTime = modelSource.DateTime;
            }
            sugarCopy = sugarSource.GetCopy();
            sugarCopy.SetOwner(factories);
            CurrentSugar = new SugarViewModel(sugarCopy, factories);
            CurrentSugar.PropertyChanged += (_, eventArguments) =>
            {
                if (eventArguments.PropertyName == "BloodSugar")
                    StartCalculation();
            };
        }

        private void BolusChanged()
        {
            IsCalculated = false;
            ClearSugarChart();
            var bolusEdited = modelCopy.NormalBolus != 0
                || modelCopy.SquareWaveBolus != 0
                || modelCopy.SquareWaveBolusHours != 0;
            if (this.bolusEdited != bolusEdited)
            {
                this.bolusEdited = bolusEdited;
                if (!bolusEdited)
                    StartCalculation();
            }
        }

        private void StartCalculation()
        {
            var mealPresent = meal != null;
            var sugarEntered = sugarCopy.BloodSugar != 0;
            if (openedWithNoBolus && !bolusEdited && mealPresent && sugarEntered)
                StartCalculationInternal();
        }

        private void StartCalculationInternal()
        {
            var worker = workerFactory.Create();
            worker.DoWork += DoCalculation;
            worker.RunWorkerCompleted += CalculationCompleted;
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        private void DoCalculation(object sender, DoWorkEventArgs e)
        {
            e.Result = facade.GetReplacementAndEstimatedSugars(meal, modelCopy, sugarCopy);
        }

        private void CalculationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            var replacementAndEstimatedSugars = e.Result as ReplacementAndEstimatedSugars;
            var replacement = replacementAndEstimatedSugars.Replacement;
            if (replacement.Items.Any())
                ShowCalculation(replacementAndEstimatedSugars);
            else
                ShowNoCalculation();
        }

        private void ShowCalculation(ReplacementAndEstimatedSugars replacementAndEstimatedSugars)
        {
            var replacement = replacementAndEstimatedSugars.Replacement;
            var insulin = replacement.InsulinTotal;
            Subject.Insulin.NormalBolus = insulin.NormalBolus;
            Subject.Insulin.SquareWaveBolus = insulin.SquareWaveBolus;
            Subject.Insulin.SquareWaveBolusHours = insulin.SquareWaveBolusHours;
            Subject.NotifyBolusChange();
            bolusEdited = false;
            IsCalculated = true;
            IsCalculatedText = replacement.IsComplete
                ? Translations.InsulinHeaderCalculated : Translations.InsulinHeaderIncomplete;
            PopulateSugarChart(replacementAndEstimatedSugars.EstimatedSugars);
        }

        private void ShowNoCalculation()
        {
            Subject.Insulin.NormalBolus = 0;
            Subject.Insulin.SquareWaveBolus = 0;
            Subject.Insulin.SquareWaveBolusHours = 0;
            IsCalculated = false;
            ClearSugarChart();
        }

        private void PopulateSugarChart(IList<Sugar> estimatedSugars)
        {
            SugarChart = new ObservableCollection<SugarChartItemViewModel>(
                new Sugar[] { sugarCopy }
                .Concat(estimatedSugars)
                .Select(sugar => new SugarChartItemViewModel(sugar)));
            OnPropertyChanged("SugarChart");
            OnPropertyChanged("SugarChartMinimum");
            OnPropertyChanged("SugarChartMaximum");
        }

        private void ClearSugarChart()
        {
            SugarChart.Clear();
            OnPropertyChanged("SugarChart");
        }

        public enum CanDeleteCircumstanceResult { Yes, NoCircumstanceChoosen, NoThereIsOnlyOneCircumstance };

        public class SugarChartItemViewModel
        {
            private readonly Sugar sugar;

            public SugarChartItemViewModel(Sugar sugar)
            {
                this.sugar = sugar;
            }

            public DateTime DateTime { get { return sugar.DateTime; } }
            public float BloodSugar { get { return sugar.BloodSugar; } }
        }
    }
}
