using System;
using System.Collections.Generic;
using System.Linq;

namespace Dietphone.ViewModels
{
    public class GroupingViewModel<T, TKey> : ViewModelBase
        where T : class
    {
        public List<IGrouping<TKey, T>> Groups { get; private set; }
        private readonly SearchSubViewModel viewModel;
        private readonly Func<IEnumerable<T>> items;
        private readonly Func<T, TKey> keySelector;
        private readonly Func<T, bool> predicate;
        private readonly Action<T> choose;

        public GroupingViewModel(SearchSubViewModel viewModel, Func<IEnumerable<T>> items, Func<T, TKey> keySelector,
            Func<T, bool> predicate, Action<T> choose)
        {
            this.viewModel = viewModel;
            this.items = items;
            this.keySelector = keySelector;
            this.predicate = predicate;
            this.choose = choose;
            viewModel.Loaded += delegate { Invalidate(); };
            viewModel.Refreshed += delegate { Invalidate(); };
            viewModel.UpdateFilterDescriptors += delegate { Invalidate(); };
        }

        public T Choice
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                {
                    choose(value);
                    RaisePropertyChanged("Choice");
                }
            }
        }

        private void Invalidate()
        {
            if (this.items() == null)
                return;
            var doSearch = !string.IsNullOrEmpty(viewModel.Search);
            var items = doSearch ? this.items().Where(predicate) : this.items();
            items = ProcessItems(items);
            var groups = items.GroupBy(keySelector);
            groups = ProcessGroups(groups);
            Groups = groups.ToList();
            OnPropertyChanged("Groups");
        }

        protected virtual IEnumerable<T> ProcessItems(IEnumerable<T> items)
        {
            return items;
        }

        protected virtual IEnumerable<IGrouping<TKey, T>> ProcessGroups(IEnumerable<IGrouping<TKey, T>> groups)
        {
            return groups;
        }
    }

    public class SortedGroupingViewModel<T, TKey, TItemSort, TGroupSort> : GroupingViewModel<T, TKey>
        where T : class
    {
        private readonly Func<T, TItemSort> itemSort;
        private readonly Func<IGrouping<TKey, T>, TGroupSort> groupSort;

        public SortedGroupingViewModel(SearchSubViewModel viewModel, Func<IEnumerable<T>> items,
                Func<T, TKey> keySelector, Func<T, bool> predicate, Action<T> choose,
                Func<T, TItemSort> itemSort, Func<IGrouping<TKey, T>, TGroupSort> groupSort)
            : base(viewModel, items, keySelector, predicate, choose)
        {
            this.itemSort = itemSort;
            this.groupSort = groupSort;
        }

        protected override IEnumerable<T> ProcessItems(IEnumerable<T> items)
        {
            return items.OrderBy(itemSort);
        }

        protected override IEnumerable<IGrouping<TKey, T>> ProcessGroups(IEnumerable<IGrouping<TKey, T>> groups)
        {
            return groups.OrderBy(groupSort);
        }
    }
}
