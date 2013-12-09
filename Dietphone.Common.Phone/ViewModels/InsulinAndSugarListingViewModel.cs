using Dietphone.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Dietphone.ViewModels
{
    public class InsulinAndSugarListingViewModel : SearchSubViewModel
    {
        public ObservableCollection<ViewModelWithDateAndText> InsulinsAndSugars { get; protected set; }
        public ObservableCollection<DateViewModel> Dates { get; protected set; }
        private readonly Factories factories;
        private readonly BackgroundWorkerFactory workerFactory;

        public InsulinAndSugarListingViewModel(Factories factories, BackgroundWorkerFactory workerFactory)
        {
            this.factories = factories;
            this.workerFactory = workerFactory;
        }

        public override void Load()
        {
            if (Dates == null && InsulinsAndSugars == null)
            {
                var loader = new CircumstancesAndInsulinsAndSugarsLoader(this);
                loader.Loaded += delegate { OnLoaded(); };
                loader.LoadAsync();
            }
        }

        public override void Refresh()
        {
            if (Dates != null && InsulinsAndSugars != null)
            {
                var loader = new CircumstancesAndInsulinsAndSugarsLoader(this);
                loader.Loaded += delegate { OnRefreshed(); };
                loader.LoadAsync();
            }
        }

        public class CircumstancesAndInsulinsAndSugarsLoader : LoaderBaseWithDates
        {
            private ObservableCollection<InsulinCircumstanceViewModel> circumstances;
            private List<InsulinViewModel> unsortedInsulins;
            private List<SugarViewModel> unsortedSugars;
            private ObservableCollection<ViewModelWithDateAndText> sortedInsulinsAndSugars;
            private readonly bool sortCircumstances;

            public CircumstancesAndInsulinsAndSugarsLoader(InsulinAndSugarListingViewModel viewModel)
                : base(viewModel.workerFactory)
            {
                this.viewModel = viewModel;
                factories = viewModel.factories;
            }

            public CircumstancesAndInsulinsAndSugarsLoader(Factories factories, bool sortCircumstances,
                BackgroundWorkerFactory workerFactory)
                : base(workerFactory)
            {
                this.factories = factories;
                this.sortCircumstances = sortCircumstances;
            }

            public ObservableCollection<InsulinCircumstanceViewModel> Circumstances
            {
                get
                {
                    if (circumstances == null)
                    {
                        LoadCircumstances();
                    }
                    return circumstances;
                }
            }

            protected override void DoWork()
            {
                LoadCircumstances();
                LoadUnsortedInsulins();
                LoadUnsortedSugars();
                MakeDatesAndSortInsulins();
            }

            protected override void WorkCompleted()
            {
                AssignDates();
                AssignSortedInsulins();
                base.WorkCompleted();
            }

            private void LoadCircumstances()
            {
                var models = factories.InsulinCircumstances;
                var unsortedViewModels = new ObservableCollection<InsulinCircumstanceViewModel>();
                foreach (var model in models)
                {
                    var viewModel = new InsulinCircumstanceViewModel(model, factories);
                    unsortedViewModels.Add(viewModel);
                }
                if (sortCircumstances)
                {
                    var sortedViewModels = unsortedViewModels.OrderBy(circumstance => circumstance.Name);
                    circumstances = new ObservableCollection<InsulinCircumstanceViewModel>();
                    foreach (var viewModel in sortedViewModels)
                    {
                        circumstances.Add(viewModel);
                    }
                }
                else
                {
                    circumstances = unsortedViewModels;
                }
            }

            private void LoadUnsortedInsulins()
            {
                var models = factories.Insulins;
                unsortedInsulins = new List<InsulinViewModel>();
                foreach (var model in models)
                {
                    var viewModel = new InsulinViewModel(model, factories, circumstances);
                    unsortedInsulins.Add(viewModel);
                }
            }

            private void LoadUnsortedSugars()
            {
                var models = factories.Sugars;
                unsortedSugars = new List<SugarViewModel>();
                foreach (var model in models)
                {
                    var viewModel = new SugarViewModel(model, factories);
                    unsortedSugars.Add(viewModel);
                }
            }

            private void MakeDatesAndSortInsulins()
            {
                var unsortedInsulinsAndSugars = new List<ViewModelWithDateAndText>();
                unsortedInsulinsAndSugars.AddRange(unsortedInsulins.Cast<ViewModelWithDateAndText>());
                unsortedInsulinsAndSugars.AddRange(unsortedSugars.Cast<ViewModelWithDateAndText>());
                sortedInsulinsAndSugars = MakeDatesAndSortItems(unsortedInsulinsAndSugars, ThenBy);
            }

            private void AssignSortedInsulins()
            {
                GetViewModel().InsulinsAndSugars = sortedInsulinsAndSugars;
                GetViewModel().OnPropertyChanged("InsulinsAndSugars");
            }

            private void AssignDates()
            {
                GetViewModel().Dates = dates;
                GetViewModel().OnPropertyChanged("Dates");
            }

            private int ThenBy(ViewModelWithDateAndText item)
            {
                return item is SugarViewModel ? 1 : 2;
            }

            private InsulinAndSugarListingViewModel GetViewModel()
            {
                return viewModel as InsulinAndSugarListingViewModel;
            }
        }
    }
}
