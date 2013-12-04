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
        protected readonly BackgroundWorkerFactory workerFactory;

        public LoaderBase(BackgroundWorkerFactory workerFactory)
        {
            this.workerFactory = workerFactory;
        }

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
            var worker = workerFactory.Create();
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

        public LoaderBaseWithDates(BackgroundWorkerFactory workerFactory) : base(workerFactory)
        {
        }

        protected ObservableCollection<T> MakeDatesAndSortItems<T>(List<T> unsortedItems, Func<T, int> thenBy = null)
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
            var older = (from date in itemDatesDescending.Skip(newerCount)
                        from item in date
                        select item)
                        .OrderByDescending(item => item.DateAndTime);
            if (thenBy != null)
                older = older.ThenBy(thenBy);
            dates = new ObservableCollection<DateViewModel>();
            var sortedItems = new ObservableCollection<T>();
            foreach (var date in newer)
            {
                var normalDate = DateViewModel.CreateNormalDate(date.Key);
                dates.Add(normalDate);
                var dateAscending = date.OrderBy(item => item.DateTime);
                if (thenBy != null)
                    dateAscending = dateAscending.ThenBy(thenBy);
                foreach (var item in dateAscending)
                {
                    item.Date = normalDate;
                    sortedItems.Add(item);
                }
            }
            if (older.Count() > 0)
            {
                var groupOfOlder = DateViewModel.CreateGroupOfOlder();
                dates.Add(groupOfOlder);
                foreach (var item in older)
                {
                    item.Date = groupOfOlder;
                    sortedItems.Add(item);
                }
            }
            return sortedItems;
        }
    }
}
