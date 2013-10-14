using Dietphone.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Dietphone.ViewModels
{
    public class InsulinListingViewModel : SubViewModel
    {
        public ObservableCollection<InsulinViewModel> Insulins { get; private set; }
        public ObservableCollection<DateViewModel> Dates { get; private set; }
        private Factories factories;

        public InsulinListingViewModel(Factories factories)
        {
            this.factories = factories;
        }

        public override void Load()
        {
            throw new NotImplementedException();
        }

        public override void Refresh()
        {
            throw new NotImplementedException();
        }

        protected override void OnSearchChanged()
        {
            throw new NotImplementedException();
        }

        public class CircumstancesAndInsulinsLoader : LoaderBaseWithDates
        {
            private ObservableCollection<InsulinCircumstanceViewModel> circumstances;
            private List<InsulinViewModel> unsortedInsulins;
            private ObservableCollection<InsulinViewModel> sortedInsulins;
            private readonly bool sortCircumstances;

            public CircumstancesAndInsulinsLoader(InsulinListingViewModel viewModel)
            {
                this.viewModel = viewModel;
                factories = viewModel.factories;
            }

            public CircumstancesAndInsulinsLoader(Factories factories, bool sortCircumstances)
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

            private void MakeDatesAndSortInsulins()
            {
                sortedInsulins = MakeDatesAndSortItems(unsortedInsulins);
            }

            private void AssignSortedInsulins()
            {
                GetViewModel().Insulins = sortedInsulins;
                GetViewModel().OnPropertyChanged("Insulins");
            }

            private void AssignDates()
            {
                GetViewModel().Dates = dates;
                GetViewModel().OnPropertyChanged("Dates");
            }

            private InsulinListingViewModel GetViewModel()
            {
                return viewModel as InsulinListingViewModel;
            }
        }
    }
}
