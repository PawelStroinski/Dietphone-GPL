using Dietphone.Models;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;

namespace Dietphone.ViewModels
{
    public class TelerikJournalViewModel : JournalViewModel
    {
        public ObservableCollection<DataDescriptor> GroupDescriptors { private get; set; }
        public ObservableCollection<DataDescriptor> FilterDescriptors { private get; set; }

        public TelerikJournalViewModel(Factories factories, BackgroundWorkerFactory workerFactory,
            SugarEditingViewModel sugarEditing)
            : base(factories, workerFactory, sugarEditing)
        {
        }

        public void UpdateGroupDescriptors()
        {
            GroupDescriptors.Clear();
            var groupByDate = new GenericGroupDescriptor<JournalItemViewModel, DateViewModel>(vm => vm.Date);
            GroupDescriptors.Add(groupByDate);
        }

        protected override void UpdateFilterDescriptors()
        {
            FilterDescriptors.Clear();
            if (!string.IsNullOrEmpty(search))
            {
                var filterIn = new GenericFilterDescriptor<JournalItemViewModel>(vm => vm.FilterIn(search));
                FilterDescriptors.Add(filterIn);
            }
        }
    }
}
