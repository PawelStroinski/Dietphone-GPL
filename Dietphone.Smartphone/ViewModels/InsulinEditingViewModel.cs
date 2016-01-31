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
        public InsulinViewModel Calculated { get; private set; }
        public ObservableCollection<SugarChartItemViewModel> SugarChart { get; private set; }
        public ObservableCollection<ReplacementItemViewModel> ReplacementItems { get; private set; }
        public IList<PatternViewModel> CalculationDetailsAlternatives { get; private set; }
        public ScoreSelector MealScores { get; private set; }
        public bool MealScoresVisible { get; private set; }
        private List<InsulinCircumstanceViewModel> addedCircumstances = new List<InsulinCircumstanceViewModel>();
        private List<InsulinCircumstanceViewModel> deletedCircumstances = new List<InsulinCircumstanceViewModel>();
        private Sugar sugarSource;
        private Sugar sugarCopy;
        private bool isBusy;
        private bool isCalculated;
        private bool isCalculationIncomplete;
        private bool isCalculationEmpty;
        private bool noMealPresent;
        private bool noSugarEntered;
        private bool calculationDetailsVisible;
        private bool calculationDetailsAlternativesVisible;
        private bool sugarIsNew;
        private Meal meal;
        private bool wentToSettings;
        private IList<ReplacementItem> replacementItems;
        private IEnumerable<MealNameViewModel> names;
        private MealNameViewModel defaultName;
        private readonly ReplacementBuilderAndSugarEstimatorFacade facade;
        private readonly BackgroundWorkerFactory workerFactory;
        private readonly Action<string> setClipboard;
        private const decimal SUGAR_CHART_MARGIN_MINIMUM_MGDL = (decimal)10;
        private const decimal SUGAR_CHART_MARGIN_MAXIMUM_MGDL = (decimal)55;
        private const decimal SUGAR_CHART_MARGIN_MINIMUM_MMOLL = (decimal)0.55;
        private const decimal SUGAR_CHART_MARGIN_MAXIMUM_MMOLL = (decimal)3.05;
        private const string INSULIN = "INSULIN";
        private const string INSULIN_SUGAR = "INSULIN_SUGAR";
        private const string CIRCUMSTANCES = "CIRCUMSTANCES";
        private const string IS_CALCULATED = "IS_CALCULATED";
        private const string IS_CALCULATION_INCOMPLETE = "IS_CALCULATION_INCOMPLETE";
        private const string CALCULATED = "CALCULATED";
        private const string SUGAR_CHART = "SUGAR_CHART";
        private const string INSULIN_ID = "INSULIN_ID";
        private const string REPLACEMENT_ITEMS = "REPLACEMENT_ITEMS";
        private const string CALCULATION_DETAILS_VISIBLE = "CALCULATION_DETAILS_VISIBLE";
        private const string CALCULATION_DETAILS_ALTERNATIVES_VISIBLE = "CALCULATION_DETAILS_ALTERNATIVES_VISIBLE";
        private const string CALCULATION_DETAILS_ALTERNATIVES_INDEX = "CALCULATION_DETAILS_ALTERNATIVES_INDEX";

        public InsulinEditingViewModel(Factories factories, ReplacementBuilderAndSugarEstimatorFacade facade,
            BackgroundWorkerFactory workerFactory, Action<string> setClipboard)
            : base(factories)
        {
            this.facade = facade;
            this.workerFactory = workerFactory;
            this.setClipboard = setClipboard;
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

        public bool IsCalculationIncomplete
        {
            get
            {
                return isCalculationIncomplete;
            }
            private set
            {
                isCalculationIncomplete = value;
                OnPropertyChanged("IsCalculationIncomplete");
            }
        }

        public bool IsCalculationEmpty
        {
            get
            {
                return isCalculationEmpty;
            }
            private set
            {
                isCalculationEmpty = value;
                OnPropertyChanged("IsCalculationEmpty");
            }
        }

        public bool NoMealPresent
        {
            get
            {
                return noMealPresent;
            }
            private set
            {
                noMealPresent = value;
                OnPropertyChanged("NoMealPresent");
            }
        }

        public bool NoSugarEntered
        {
            get
            {
                return noSugarEntered;
            }
            private set
            {
                noSugarEntered = value;
                OnPropertyChanged("NoSugarEntered");
            }
        }

        public bool CalculationDetailsVisible
        {
            get
            {
                return calculationDetailsVisible;
            }
            private set
            {
                calculationDetailsVisible = value;
                OnPropertyChanged("CalculationDetailsVisible");
            }
        }

        public bool CalculationDetailsAlternativesVisible
        {
            get
            {
                return calculationDetailsAlternativesVisible;
            }
            private set
            {
                calculationDetailsAlternativesVisible = value;
                OnPropertyChanged("CalculationDetailsAlternativesVisible");
            }
        }

        public decimal SugarChartMinimum
        {
            get
            {
                var margin = factories.Settings.SugarUnit == SugarUnit.mgdL
                    ? SUGAR_CHART_MARGIN_MINIMUM_MGDL : SUGAR_CHART_MARGIN_MINIMUM_MMOLL;
                return SugarChart.Any() ? SugarChart.Min(sugar => sugar.BloodSugar) - margin : 100;
            }
        }

        public decimal SugarChartMaximum
        {
            get
            {
                var margin = factories.Settings.SugarUnit == SugarUnit.mgdL
                    ? SUGAR_CHART_MARGIN_MAXIMUM_MGDL : SUGAR_CHART_MARGIN_MAXIMUM_MMOLL;
                return SugarChart.Any() ? SugarChart.Max(sugar => sugar.BloodSugar) + margin : 100;
            }
        }

        public string SugarChartAsText
        {
            get
            {
                var sugarUnit = factories.Settings.SugarUnit == SugarUnit.mgdL
                    ? Translations.BloodSugarMgdL : Translations.BloodSugarMmolL;
                return Translations.EstimatedBloodSugar + Environment.NewLine + Environment.NewLine
                    + string.Join(Environment.NewLine, SugarChart
                        .Select(item => string.Format("{0}   {1}",
                            item.DateTime.ToString("t"),
                            string.Format(sugarUnit, item.BloodSugar))));
            }
        }

        public string ListOfMealItemsNotIncludedInCalculation
        {
            get
            {
                CheckReplacementItems();
                return Translations.IngredientsNotIncluded + Environment.NewLine + Environment.NewLine
                    + string.Join(Environment.NewLine, meal.Items
                        .Where(mealItem => !replacementItems
                            .Any(replacementItem => replacementItem.Pattern.For.ProductId == mealItem.ProductId))
                        .Select(mealItem => mealItem.Product.Name));
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
                Navigator.GoToMain();
        }

        public void DeleteAndSaveAndReturn()
        {
            var models = factories.Insulins;
            models.Remove(modelSource);
            DeleteNewSugar();
            SaveCircumstances();
            Navigator.GoBack();
        }

        public override void CancelAndReturn()
        {
            DeleteNewSugar();
            base.CancelAndReturn();
        }

        public void GoToMealEditing()
        {
            SaveWithUpdatedTime();
            IsDirty = false;
            FindMeal();
            if (meal == null)
                meal = factories.CreateMeal();
            Navigator.GoToMealEditing(meal.Id);
        }

        public void CopyAsText()
        {
            setClipboard(Subject.Text);
        }

        public void OpenScoresSettings()
        {
            wentToSettings = true;
            Navigator.GoToSettings();
        }

        public void UseCalculation()
        {
            Subject.NormalBolus = Calculated.NormalBolus;
            Subject.SquareWaveBolus = Calculated.SquareWaveBolus;
            Subject.SquareWaveBolusHours = Calculated.SquareWaveBolusHours;
            Pivot = 0;
        }

        public void CalculationDetails()
        {
            ReplacementItemsToViewModels();
            OnPropertyChanged("ReplacementItems");
            CalculationDetailsVisible = true;
        }

        public void CloseCalculationDetails()
        {
            CalculationDetailsVisible = false;
        }

        public void CloseCalculationDetailsAlternatives()
        {
            CalculationDetailsAlternativesVisible = false;
        }

        public void CloseCalculationDetailsÓrAlternativesOnBackButton()
        {
            if (CalculationDetailsAlternativesVisible)
            {
                CloseCalculationDetailsAlternatives();
                return;
            }
            if (CalculationDetailsVisible)
            {
                CloseCalculationDetails();
                return;
            }
        }

        public void ReturnedFromNavigation()
        {
            if (wentToSettings)
            {
                wentToSettings = false;
                MealScores.Invalidate();
            }
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
            MakeMealScores();
            base.MakeViewModel();
        }

        protected override string Validate()
        {
            return string.Empty;
        }

        protected override void TombstoneModel()
        {
            TombstoneInsulin(modelCopy, INSULIN);
        }

        protected override void UntombstoneModel()
        {
            UntombstoneInsulin(modelCopy, INSULIN);
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

        protected override void OnCommonUiReady()
        {
            Calculated = new InsulinViewModel(CreateEmptyCalculated(), factories, allCircumstances: Circumstances);
            SugarChart = new ObservableCollection<SugarChartItemViewModel>();
            ReplacementItems = new ObservableCollection<ReplacementItemViewModel>();
            CalculationDetailsAlternatives = new List<PatternViewModel>();
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

        private void DeleteNewSugar()
        {
            if (sugarIsNew)
                factories.Sugars.Remove(sugarSource);
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
            var loader = new JournalViewModel.JournalLoader(factories, sortCircumstances: true, sortNames: false,
                workerFactory: workerFactory);
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

        private void MakeMealScores()
        {
            if (meal == null)
                MealScores = new EmptyScoreSelector(factories);
            else
            {
                var mealViewModel = new MealViewModel(meal, factories);
                MealScores = mealViewModel.Scores;
                MealScoresVisible = true;
            }
        }

        private void TombstoneInsulin(Insulin insulin, string key)
        {
            var state = StateProvider.State;
            var dto = DTOFactory.InsulinToDTO(insulin);
            state[key] = dto.Serialize(string.Empty);
        }

        private void UntombstoneInsulin(Insulin insulin, string key)
        {
            var state = StateProvider.State;
            if (state.ContainsKey(key))
            {
                var dtoState = (string)state[key];
                var dto = dtoState.Deserialize<InsulinDTO>(string.Empty);
                if (dto.Id == insulin.Id)
                    DTOReader.DTOToInsulin(dto, insulin);
            }
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
            state[IS_CALCULATION_INCOMPLETE] = IsCalculationIncomplete;
            TombstoneInsulin(Calculated.Insulin, CALCULATED);
            var sugars = new List<Sugar>();
            foreach (var item in SugarChart)
                item.AddModelTo(sugars);
            state[SUGAR_CHART] = sugars;
            if (replacementItems != null && replacementItems.All(replacementItem => replacementItem.Pattern != null))
            {
                TombstoneReplacementItems();
                TombstoneCalculationDetailsAlternatives();
            }
            state[CALCULATION_DETAILS_VISIBLE] = CalculationDetailsVisible;
            state[CALCULATION_DETAILS_ALTERNATIVES_VISIBLE] = CalculationDetailsAlternativesVisible;
        }

        private void UntombstoneCalculation()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(IS_CALCULATED))
                IsCalculated = (bool)state[IS_CALCULATED];
            if (state.ContainsKey(IS_CALCULATION_INCOMPLETE))
                IsCalculationIncomplete = (bool)state[IS_CALCULATION_INCOMPLETE];
            if (state.ContainsKey(CALCULATED))
            {
                var calculated = Calculated.Insulin;
                UntombstoneInsulin(calculated, CALCULATED);
                ChangeCalculated(calculated);
            }
            if (state.ContainsKey(SUGAR_CHART))
            {
                var sugars = state[SUGAR_CHART] as List<Sugar>;
                if (sugars != null)
                    SugarChart = new ObservableCollection<SugarChartItemViewModel>(
                        sugars.Select(sugar => new SugarChartItemViewModel(sugar)).ToList());
            }
            if (state.ContainsKey(REPLACEMENT_ITEMS))
                UntombstoneReplacementItems();
            if (state.ContainsKey(CALCULATION_DETAILS_ALTERNATIVES_INDEX))
                UntombstoneCalculationDetailsAlternatives();
            if (state.ContainsKey(CALCULATION_DETAILS_VISIBLE))
                CalculationDetailsVisible = (bool)state[CALCULATION_DETAILS_VISIBLE];
            if (state.ContainsKey(CALCULATION_DETAILS_ALTERNATIVES_VISIBLE))
                CalculationDetailsAlternativesVisible = (bool)state[CALCULATION_DETAILS_ALTERNATIVES_VISIBLE];
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

        private void StartCalculation()
        {
            var mealPresent = meal != null;
            var sugarEntered = sugarCopy.BloodSugar != 0;
            if (mealPresent && sugarEntered)
                StartCalculationInternal();
            NoMealPresent = !mealPresent;
            NoSugarEntered = !sugarEntered;
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
            replacementItems = replacement.Items;
        }

        private void ShowCalculation(ReplacementAndEstimatedSugars replacementAndEstimatedSugars)
        {
            var replacement = replacementAndEstimatedSugars.Replacement;
            var insulin = replacement.InsulinTotal;
            ChangeCalculated(insulin);
            IsCalculated = true;
            IsCalculationIncomplete = !replacement.IsComplete;
            IsCalculationEmpty = false;
            PopulateSugarChart(replacementAndEstimatedSugars.EstimatedSugars);
        }

        private void ShowNoCalculation()
        {
            ChangeCalculated(CreateEmptyCalculated());
            IsCalculated = false;
            IsCalculationIncomplete = false;
            IsCalculationEmpty = true;
            ClearSugarChart();
        }

        private void ChangeCalculated(Insulin calculated)
        {
            Calculated.ChangeModel(calculated);
            OnPropertyChanged("Calculated");
        }

        private void PopulateSugarChart(IList<Sugar> estimatedSugars)
        {
            var currentSugar = sugarCopy.GetCopy();
            currentSugar.DateTime = meal.DateTime;
            SugarChart = new ObservableCollection<SugarChartItemViewModel>(
                new Sugar[] { currentSugar }
                .Concat(estimatedSugars)
                .OrderBy(sugar => sugar.DateTime)
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

        private Insulin CreateEmptyCalculated()
        {
            return new Insulin();
        }

        private void TombstoneReplacementItems()
        {
            var state = StateProvider.State;
            var dtos = replacementItems.Select(DTOFactory.ReplacementItemToDTO).ToList();
            state[REPLACEMENT_ITEMS] = dtos.Serialize(string.Empty);
        }

        private void UntombstoneReplacementItems()
        {
            var state = StateProvider.State;
            var dtosState = (string)state[REPLACEMENT_ITEMS];
            var dtos = dtosState.Deserialize<List<ReplacementItemDTO>>(string.Empty);
            replacementItems = dtos.Select(dto => DTOReader.DTOToReplacementItem(dto, factories)).ToList();
            ReplacementItemsToViewModels();
        }

        private void TombstoneCalculationDetailsAlternatives()
        {
            var state = StateProvider.State;
            var index = ReplacementItems.IndexOf(ReplacementItems
                .FirstOrDefault(replacementItem => replacementItem.Alternatives == CalculationDetailsAlternatives));
            if (index != -1)
                state[CALCULATION_DETAILS_ALTERNATIVES_INDEX] = index;
        }

        private void UntombstoneCalculationDetailsAlternatives()
        {
            var state = StateProvider.State;
            var index = (int)state[CALCULATION_DETAILS_ALTERNATIVES_INDEX];
            if (index >= 0 && index < ReplacementItems.Count)
                CalculationDetailsAlternatives = ReplacementItems[index].Alternatives;
        }

        private void ReplacementItemsToViewModels()
        {
            CheckReplacementItems();
            LoadNames();
            ReplacementItems = new ObservableCollection<ReplacementItemViewModel>(replacementItems
                .Select(replacementItem => new ReplacementItemViewModel(
                    replacementItem, factories, allCircumstances: Circumstances, names: names, defaultName: defaultName,
                    navigator: Navigator, save: SaveWithUpdatedTime,
                    showAlternatives: ShowCalculationDetailsAlternatives)));
        }

        private void CheckReplacementItems()
        {
            if (replacementItems == null)
                throw new InvalidOperationException("No replacement items");
        }

        private void LoadNames()
        {
            var loaded = names != null && defaultName != null;
            if (loaded)
                return;
            var loader = new JournalViewModel.JournalLoader(factories, sortCircumstances: false, sortNames: true,
                workerFactory: workerFactory);
            names = loader.Names;
            defaultName = loader.DefaultName;
        }

        private void ShowCalculationDetailsAlternatives(IList<PatternViewModel> alternatives)
        {
            CalculationDetailsAlternatives = alternatives;
            OnPropertyChanged("CalculationDetailsAlternatives");
            CalculationDetailsAlternativesVisible = true;
        }

        public enum CanDeleteCircumstanceResult { Yes, NoCircumstanceChoosen, NoThereIsOnlyOneCircumstance };

        public class SugarChartItemViewModel
        {
            private readonly Sugar sugar;

            public SugarChartItemViewModel(Sugar sugar)
            {
                this.sugar = sugar;
            }

            public DateTime DateTime { get { return sugar.DateTime.ToLocalTime(); } }
            public decimal BloodSugar { get { return (decimal)sugar.BloodSugar; } }

            public void AddModelTo(List<Sugar> target)
            {
                target.Add(sugar);
            }
        }
    }
}
