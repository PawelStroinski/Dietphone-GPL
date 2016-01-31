using Dietphone.Models;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public class TelerikProductListingViewModel : ProductListingViewModel
    {
        public ObservableCollection<DataDescriptor> GroupDescriptors { private get; set; }
        public ObservableCollection<DataDescriptor> SortDescriptors { private get; set; }
        public ObservableCollection<DataDescriptor> FilterDescriptors { private get; set; }

        public TelerikProductListingViewModel(Factories factories, BackgroundWorkerFactory workerFactory)
            : base(factories, workerFactory)
        {
        }

        public void UpdateGroupDescriptors()
        {
            GroupDescriptors.Clear();
            var groupByCategory = new GenericGroupDescriptor<ProductViewModel, CategoryViewModel>(product => product.Category);
            GroupDescriptors.Add(groupByCategory);
        }

        public void UpdateSortDescriptors()
        {
            SortDescriptors.Clear();
            var sortByName = new GenericSortDescriptor<ProductViewModel, string>(product => product.Name);
            SortDescriptors.Add(sortByName);
        }

        protected override void UpdateFilterDescriptors()
        {
            FilterDescriptors.Clear();
            if (!string.IsNullOrEmpty(search))
            {
                var filterByName = new GenericFilterDescriptor<ProductViewModel>(product => product.Name.ContainsIgnoringCase(search));
                FilterDescriptors.Add(filterByName);
            }
        }
    }
}
