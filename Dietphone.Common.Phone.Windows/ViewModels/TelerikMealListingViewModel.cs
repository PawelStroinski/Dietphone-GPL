using Dietphone.Models;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;

namespace Dietphone.ViewModels
{
    public class TelerikMealListingViewModel : MealListingViewModel
    {
        public ObservableCollection<DataDescriptor> GroupDescriptors { private get; set; }
        public ObservableCollection<DataDescriptor> FilterDescriptors { private get; set; }

        public TelerikMealListingViewModel(Factories factories, BackgroundWorkerFactory workerFactory)
            : base(factories, workerFactory)
        {
        }

        public void UpdateGroupDescriptors()
        {
            GroupDescriptors.Clear();
            var groupByDate = new GenericGroupDescriptor<MealViewModel, DateViewModel>(meal => meal.Date);
            GroupDescriptors.Add(groupByDate);
        }

        protected override void UpdateFilterDescriptors()
        {
            FilterDescriptors.Clear();
            if (!string.IsNullOrEmpty(search))
            {
                var filterIn = new GenericFilterDescriptor<MealViewModel>(meal => meal.FilterIn(search));
                FilterDescriptors.Add(filterIn);
            }
        }
    }
}
