using Dietphone.Models;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;

namespace Dietphone.ViewModels
{
    public class TelerikInsulinAndSugarListingViewModel : InsulinAndSugarListingViewModel
    {
        public ObservableCollection<DataDescriptor> GroupDescriptors { private get; set; }
        public ObservableCollection<DataDescriptor> FilterDescriptors { private get; set; }

        public TelerikInsulinAndSugarListingViewModel(Factories factories, BackgroundWorkerFactory workerFactory)
            : base(factories, workerFactory)
        {
        }

        public void UpdateGroupDescriptors()
        {
            GroupDescriptors.Clear();
            var groupByDate = new GenericGroupDescriptor<ViewModelWithDateAndText, DateViewModel>(vm => vm.Date);
            GroupDescriptors.Add(groupByDate);
        }

        protected override void UpdateFilterDescriptors()
        {
            FilterDescriptors.Clear();
            if (!string.IsNullOrEmpty(search))
            {
                var filterIn = new GenericFilterDescriptor<ViewModelWithDateAndText>(vm => vm.FilterIn(search));
                FilterDescriptors.Add(filterIn);
            }
        }
    }
}
