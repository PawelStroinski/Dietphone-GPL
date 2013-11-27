using Dietphone.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Dietphone.ViewModels
{
    public class MealListingViewModel : SubViewModel
    {
        public ObservableCollection<MealViewModel> Meals { get; private set; }
        public ObservableCollection<DateViewModel> Dates { get; private set; }
        public event EventHandler DescriptorsUpdating;
        public event EventHandler DescriptorsUpdated;
        private readonly Factories factories;
        private readonly BackgroundWorkerFactory workerFactory;

        public MealListingViewModel(Factories factories, BackgroundWorkerFactory workerFactory)
        {
            this.factories = factories;
            this.workerFactory = workerFactory;
        }

        public override void Load()
        {
            if (Dates == null && Meals == null)
            {
                var loader = new NamesAndMealsLoader(this);
                loader.LoadAsync();
                loader.Loaded += delegate { OnLoaded(); };
            }
        }

        public override void Refresh()
        {
            if (Dates != null && Meals != null)
            {
                var loader = new NamesAndMealsLoader(this);
                loader.LoadAsync();
                loader.Loaded += delegate { OnRefreshed(); };
            }
        }

        public void Choose(MealViewModel meal)
        {
            Navigator.GoToMealEditing(meal.Id);
        }

        public override void Add()
        {
            var meal = factories.CreateMeal();
            Navigator.GoToMealEditing(meal.Id);
        }

        public MealViewModel FindMeal(Guid mealId)
        {
            var result = from meal in Meals
                         where meal.Id == mealId
                         select meal;
            return result.FirstOrDefault();
        }

        public DateViewModel FindDate(DateTime value)
        {
            var result = from date in Dates
                         where date.Date == value
                         select date;
            return result.FirstOrDefault();
        }

        protected override void OnSearchChanged()
        {
            OnDescriptorsUpdating();
            UpdateFilterDescriptors();
            OnDescriptorsUpdated();
        }

        protected virtual void UpdateFilterDescriptors()
        {
        }

        protected void OnDescriptorsUpdating()
        {
            if (DescriptorsUpdating != null)
            {
                DescriptorsUpdating(this, EventArgs.Empty);
            }
        }

        protected void OnDescriptorsUpdated()
        {
            if (DescriptorsUpdated != null)
            {
                DescriptorsUpdated(this, EventArgs.Empty);
            }
        }

        public class NamesAndMealsLoader : LoaderBaseWithDates
        {
            private ObservableCollection<MealNameViewModel> names;
            private List<MealViewModel> unsortedMeals;
            private ObservableCollection<MealViewModel> sortedMeals;
            private MealNameViewModel defaultName;
            private readonly bool sortNames;

            public NamesAndMealsLoader(MealListingViewModel viewModel)
                : base(viewModel.workerFactory)
            {
                this.viewModel = viewModel;
                factories = viewModel.factories;
            }

            public NamesAndMealsLoader(Factories factories, bool sortNames,
                BackgroundWorkerFactory workerFactory)
                : base(workerFactory)
            {
                this.factories = factories;
                this.sortNames = sortNames;
            }

            public ObservableCollection<MealNameViewModel> Names
            {
                get
                {
                    if (names == null)
                    {
                        LoadNames();
                    }
                    return names;
                }
            }

            public MealNameViewModel DefaultName
            {
                get
                {
                    if (defaultName == null)
                    {
                        MakeDefaultName();
                    }
                    return defaultName;
                }
            }

            protected override void DoWork()
            {
                LoadNames();
                MakeDefaultName();
                LoadUnsortedMeals();
                MakeDatesAndSortMeals();
            }

            protected override void WorkCompleted()
            {
                AssignDates();
                AssignSortedMeals();
                base.WorkCompleted();
            }

            private void LoadNames()
            {
                var models = factories.MealNames;
                var unsortedViewModels = new ObservableCollection<MealNameViewModel>();
                foreach (var model in models)
                {
                    var viewModel = new MealNameViewModel(model, factories);
                    unsortedViewModels.Add(viewModel);
                }
                if (sortNames)
                {
                    var sortedViewModels = unsortedViewModels.OrderBy(mealName => mealName.Name);
                    names = new ObservableCollection<MealNameViewModel>();
                    foreach (var viewModel in sortedViewModels)
                    {
                        names.Add(viewModel);
                    }
                }
                else
                {
                    names = unsortedViewModels;
                }
            }

            private void MakeDefaultName()
            {
                var defaultEntities = factories.DefaultEntities;
                var model = defaultEntities.MealName;
                defaultName = new MealNameViewModel(model, factories);
            }

            private void LoadUnsortedMeals()
            {
                var models = factories.Meals;
                unsortedMeals = new List<MealViewModel>();
                foreach (var model in models)
                {
                    var viewModel = new MealViewModel(model, factories)
                    {
                        Names = names,
                        DefaultName = defaultName
                    };
                    unsortedMeals.Add(viewModel);
                }
            }

            private void MakeDatesAndSortMeals()
            {
                sortedMeals = MakeDatesAndSortItems(unsortedMeals);
            }

            private void AssignSortedMeals()
            {
                GetViewModel().Meals = sortedMeals;
                GetViewModel().OnPropertyChanged("Meals");
            }

            private void AssignDates()
            {
                GetViewModel().Dates = dates;
                GetViewModel().OnPropertyChanged("Dates");
            }

            private MealListingViewModel GetViewModel()
            {
                return viewModel as MealListingViewModel;
            }
        }
    }
}
