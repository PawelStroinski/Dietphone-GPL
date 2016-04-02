using System;
using System.Collections.Generic;
using System.Linq;

namespace Dietphone.ViewModels
{
    public class GroupingViewModel<T, TKey> : ViewModelBase
    {
        public List<IGrouping<TKey, T>> Groups { get; private set; }
        private readonly SearchSubViewModel viewModel;
        private readonly Func<IEnumerable<T>> items;
        private readonly Func<T, TKey> keySelector;
        private readonly Func<T, bool> predicate;

        public GroupingViewModel(SearchSubViewModel viewModel, Func<IEnumerable<T>> items, Func<T, TKey> keySelector,
            Func<T, bool> predicate)
        {
            this.viewModel = viewModel;
            this.items = items;
            this.keySelector = keySelector;
            this.predicate = predicate;
            viewModel.Loaded += delegate { Invalidate(); };
            viewModel.Refreshed += delegate { Invalidate(); };
            viewModel.UpdateFilterDescriptors += delegate { Invalidate(); };
        }

        private void Invalidate()
        {
            if (this.items() == null)
                return;
            var doSearch = !string.IsNullOrEmpty(viewModel.Search);
            var items = doSearch ? this.items().Where(predicate) : this.items();
            Groups = items.GroupBy(keySelector).ToList();
            OnPropertyChanged("Groups");
        }
    }
}
