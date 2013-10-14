using System;
using Dietphone.Models;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dietphone.ViewModels
{
    public abstract class LoaderBase
    {
        public event EventHandler Loaded;
        protected Factories factories;
        protected bool isLoading;
        protected SubViewModel viewModel;

        public void LoadAsync()
        {
            if (viewModel == null)
            {
                throw new InvalidOperationException("Pass ViewModel in constructor for this operation.");
            }
            if (isLoading)
            {
                return;
            }
            var worker = new BackgroundWorker();
            worker.DoWork += delegate { DoWork(); };
            worker.RunWorkerCompleted += delegate { WorkCompleted(); };
            viewModel.IsBusy = true;
            isLoading = true;
            worker.RunWorkerAsync();
        }

        protected abstract void DoWork();

        protected virtual void WorkCompleted()
        {
            viewModel.IsBusy = false;
            isLoading = false;
            OnLoaded();
        }

        protected void OnLoaded()
        {
            if (Loaded != null)
            {
                Loaded(this, EventArgs.Empty);
            }
        }
    }

    public abstract class LoaderBaseWithDates : LoaderBase
    {
        protected ObservableCollection<DateViewModel> dates;
        private const byte DATES_MAX_COUNT = 14 * 3;

        protected ObservableCollection<T> MakeDatesAndSortItems<T>(List<T> unsortedItems)
            where T : ViewModelWithDate
        {
            var itemDatesDescending = from item in unsortedItems
                                      group item by item.DateOnly into date
                                      orderby date.Key descending
                                      select date;
            var newerCount = DATES_MAX_COUNT;
            if (itemDatesDescending.Count() > newerCount)
            {
                newerCount--;
            }
            var newer = itemDatesDescending.Take(newerCount);
            var older = from date in itemDatesDescending.Skip(newerCount)
                        from meal in date
                        orderby meal.DateTime descending
                        select meal;
            dates = new ObservableCollection<DateViewModel>();
            var sortedItems = new ObservableCollection<T>();
            foreach (var date in newer)
            {
                var normalDate = DateViewModel.CreateNormalDate(date.Key);
                dates.Add(normalDate);
                var dateAscending = date.OrderBy(meal => meal.DateTime);
                foreach (var meal in dateAscending)
                {
                    meal.Date = normalDate;
                    sortedItems.Add(meal);
                }
            }
            if (older.Count() > 0)
            {
                var groupOfOlder = DateViewModel.CreateGroupOfOlder();
                dates.Add(groupOfOlder);
                foreach (var meal in older)
                {
                    meal.Date = groupOfOlder;
                    sortedItems.Add(meal);
                }
            }
            return sortedItems;
        }
    }
}
