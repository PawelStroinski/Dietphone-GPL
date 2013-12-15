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
        private const float SUGAR_CHART_MARGIN_MAXIMUM_MGDL = 55f;
        private const float SUGAR_CHART_MARGIN_MINIMUM_MMOLL = 0.55f;
        private const float SUGAR_CHART_MARGIN_MAXIMUM_MMOLL = 3.05f;
        private const string INSULIN = "INSULIN";
        private const string INSULIN_SUGAR = "INSULIN_SUGAR";
        private const string CIRCUMSTANCES = "CIRCUMSTANCES";
        private const string IS_CALCULATED = "IS_CALCULATED";
        private const string IS_CALCULATED_TEXT = "IS_CALCULATED_TEXT";
        private const string SUGAR_CHART = "SUGAR_CHART";
        private const string BOLUS_EDITED = "BOLUS_EDITED";
        private const string INSULIN_ID = "INSULIN_ID";

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
            var relatedMealId = Navigator.GetRelatedMealId();
            if (relatedMealId == Guid.Empty)
                Navigator.GoBack();
            else
                Navigator.GoToMainToInsulinAndSugarTab();
        }

        public void DeleteAndSaveAndReturn()
        {
            var models = factories.Insulins;
            models.Remove(modelSource);
            SaveCircumstances();
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
                id = UntombstoneInsulinId();
            var modelIsNew = id == Guid.Empty;
            if (modelIsNew)
                modelSource = factories.CreateInsulin();
            else
                modelSource = finder.FindInsulinById(id);
            FindMeal();
            var mealFound = meal != null;
            if (modelIsNew && mealFound)
                modelSource.DateTime = meal.DateTime;
            modelCopy = modelSource.GetCopy();
            modelCopy.SetOwner(factories);
            modelCopy.InitializeCircumstances(modelSource.ReadCircumstances().ToList());
        }

        protected override void MakeViewModel()
        {
            LoadCircumstances();
            UntombstoneCircumstances();
            MakeInsulinViewModelInternal();
            MakeSugarViewModel();
            base.MakeViewModel();
        }

        protected override string Validate()
        {
            return string.Empty;
        }

        protected override void TombstoneModel()
        {
            var state = StateProvider.State;
            var dto = new InsulinDTO();
            dto.CopyFrom(modelCopy);
            dto.CopyCircumstancesFrom(modelCopy);
            state[INSULIN] = dto.Serialize(string.Empty);
        }

        protected override void UntombstoneModel()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(INSULIN))
            {
                var dtoState = (string)state[INSULIN];
                var dto = dtoState.Deserialize<InsulinDTO>(string.Empty);
                if (dto.Id == modelCopy.Id)
                {
                    modelCopy.CopyFrom(dto);
                    modelCopy.CopyCircumstancesFrom(dto);
                }
            }
        }

        protected override void TombstoneOtherThings()
        {
            base.TombstoneOtherThings();
            TombstoneSugar();
            TombstoneCircumstances();
            TombstoneCalculation();
            TombstoneInsulinId();
        }

        protected override void UntombstoneOtherThings()
        {
            base.UntombstoneOtherThings();
            UntombstoneSugar();
        }

        protected override void OnModelReady()
        {
            openedWithNoBolus = modelSource.NormalBolus == 0 && modelSource.SquareWaveBolus == 0;
        }

        protected override void OnCommonUiReady()
        {
            SugarChart = new ObservableCollection<SugarChartItemViewModel>();
            UntombstoneCalculation();
            if (!IsCalculated)
                StartCalculation();
        }

        private void SaveWithUpdatedTime()
        {
            UpdateLockedDateTime();
            modelSource.CopyFrom(modelCopy);
            modelSource.CopyCircumstancesFrom(modelCopy);
            sugarSource.CopyFrom(sugarCopy);
            sugarSource.DateTime = modelSource.DateTime;
            SaveCircumstances();
        }

        private void SaveCircumstances()
        {
            // TODO: This is duplicate of same stuff in product and meal editing view models, refactor it.
            foreach (var viewModel in Circumstances)
            {
                viewModel.FlushBuffer();
            }
            var models = factories.InsulinCircumstances;
            foreach (var viewModel in addedCircumstances)
            {
                models.Add(viewModel.Model);
            }
            foreach (var viewModel in deletedCircumstances)
            {
                models.Remove(viewModel.Model);
            }
        }

        private void LoadCircumstances()
        {
            var loader = new InsulinAndSugarListingViewModel.CircumstancesAndInsulinsAndSugarsLoader(factories, true,
                workerFactory);
            Circumstances = loader.Circumstances;
            foreach (var circumstance in Circumstances)
                circumstance.MakeBuffer();
        }

        private void MakeInsulinViewModelInternal()
        {
            Subject = new InsulinViewModel(modelCopy, factories, allCircumstances: Circumstances);
            Subject.PropertyChanged += (_, eventArguments) =>
            {
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
                IsDirty = true;
                if (eventArguments.PropertyName == "BloodSugar")
                    StartCalculation();
            };
        }

        private void TombstoneSugar()
        {
            var state = StateProvider.State;
            state[INSULIN_SUGAR] = sugarCopy;
        }

        private void UntombstoneSugar()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(INSULIN_SUGAR))
            {
                var untombstoned = state[INSULIN_SUGAR] as Sugar;
                if (untombstoned != null)
                    sugarCopy.CopyFrom(untombstoned);
            }
        }

        private void TombstoneCircumstances()
        {
            var circumstances = new List<InsulinCircumstance>();
            foreach (var circumstance in Circumstances)
                circumstance.AddModelTo(circumstances);
            var state = StateProvider.State;
            state[CIRCUMSTANCES] = circumstances;
        }

        private void UntombstoneCircumstances()
        {
            // TODO: This is duplicate of same stuff in product and meal editing view models, refactor it.
            var state = StateProvider.State;
            if (state.ContainsKey(CIRCUMSTANCES))
            {
                var untombstoned = (List<InsulinCircumstance>)state[CIRCUMSTANCES];
                addedCircumstances.Clear();
                var notUntombstoned = from circumstance in Circumstances
                                      where untombstoned.FindById(circumstance.Id) == null
                                      select circumstance;
                deletedCircumstances = notUntombstoned.ToList();
                foreach (var deletedCircumstance in deletedCircumstances)
                {
                    Circumstances.Remove(deletedCircumstance);
                }
                foreach (var model in untombstoned)
                {
                    var existingViewModel = Circumstances.FindById(model.Id);
                    if (existingViewModel != null)
                    {
                        existingViewModel.CopyFromModel(model);
                    }
                    else
                    {
                        var addedViewModel = new InsulinCircumstanceViewModel(model, factories);
                        Circumstances.Add(addedViewModel);
                        addedCircumstances.Add(addedViewModel);
                    }
                }
            }
        }

        private void TombstoneCalculation()
        {
            var state = StateProvider.State;
            state[IS_CALCULATED] = IsCalculated;
            state[IS_CALCULATED_TEXT] = IsCalculatedText;
            var sugars = new List<Sugar>();
            foreach (var item in SugarChart)
                item.AddModelTo(sugars);
            state[SUGAR_CHART] = sugars;
            state[BOLUS_EDITED] = bolusEdited;
        }

        private void UntombstoneCalculation()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(IS_CALCULATED))
                IsCalculated = (bool)state[IS_CALCULATED];
            if (state.ContainsKey(IS_CALCULATED_TEXT))
                IsCalculatedText = (string)state[IS_CALCULATED_TEXT];
            if (state.ContainsKey(SUGAR_CHART))
            {
                var sugars = state[SUGAR_CHART] as List<Sugar>;
                if (sugars != null)
                    SugarChart = new ObservableCollection<SugarChartItemViewModel>(
                        sugars.Select(sugar => new SugarChartItemViewModel(sugar)).ToList());
            }
            if (state.ContainsKey(BOLUS_EDITED))
                bolusEdited = (bool)state[BOLUS_EDITED];
        }

        private void TombstoneInsulinId()
        {
            var state = StateProvider.State;
            state[INSULIN_ID] = modelSource.Id;
        }

        private Guid UntombstoneInsulinId()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(INSULIN_ID))
                return (Guid)state[INSULIN_ID];
            return Guid.Empty;
        }

        private void FindMeal()
        {
            var relatedMealId = Navigator.GetRelatedMealId();
            if (relatedMealId == Guid.Empty)
                meal = finder.FindMealByInsulin(modelSource);
            else
                meal = finder.FindMealById(relatedMealId);
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
            IsCalculatedText = string.Empty;
            ClearSugarChart();
        }

        private void PopulateSugarChart(IList<Sugar> estimatedSugars)
        {
            var currentSugar = sugarCopy.GetCopy();
            currentSugar.DateTime = meal.DateTime;
            SugarChart = new ObservableCollection<SugarChartItemViewModel>(
                new Sugar[] { currentSugar }
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

            public void AddModelTo(List<Sugar> target)
            {
                target.Add(sugar);
            }
        }
    }
}
