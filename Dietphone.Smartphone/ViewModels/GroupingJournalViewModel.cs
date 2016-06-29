using Dietphone.Models;

namespace Dietphone.ViewModels
{
    public class GroupingJournalViewModel : JournalViewModel
    {
        public GroupingViewModel<JournalItemViewModel, DateViewModel> Grouping { get; private set; }

        public GroupingJournalViewModel(Factories factories, BackgroundWorkerFactory workerFactory,
                SugarEditingViewModel sugarEditing)
            : base(factories, workerFactory, sugarEditing)
        {
            Grouping = new GroupingViewModel<JournalItemViewModel, DateViewModel>(this, () => Items,
                keySelector: item => item.Date,
                predicate: item => item.FilterIn(search),
                choose: Choose);
        }
    }
}
