using System;
using System.Linq;
using System.Windows.Controls;
using Dietphone.ViewModels;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using Dietphone.Tools;

namespace Dietphone.Views
{
    public partial class Journal : UserControl
    {
        public StateProvider StateProvider { private get; set; }
        public TelerikJournalViewModel ViewModel { get; private set; }
        public event EventHandler DatesPoppedUp;
        private bool isTopItemJournal;
        private bool isTopItemDate;
        private Guid topItemJournalId;
        private DateTime topItemDate;
        private const string IS_TOP_ITEM_JOURNAL = "IS_TOP_ITEM_JOURNAL";
        private const string IS_TOP_ITEM_DATE = "IS_TOP_ITEM_DATE";
        private const string TOP_ITEM_JOURNAL_ID = "TOP_ITEM_JOURNAL_ID";
        private const string TOP_ITEM_DATE = "TOP_ITEM_DATE";

        public Journal()
        {
            InitializeComponent();
            ViewModel = new TelerikJournalViewModel(MyApp.Factories, new BackgroundWorkerWrapperFactory(),
                SugarEditing.ViewModel);
            DataContext = ViewModel;
            ViewModel.GroupDescriptors = List.GroupDescriptors;
            ViewModel.FilterDescriptors = List.FilterDescriptors;
            ViewModel.UpdateGroupDescriptors();
            ViewModel.DescriptorsUpdating += delegate { List.BeginDataUpdate(); };
            ViewModel.DescriptorsUpdated += delegate { List.EndDataUpdate(); };
            ViewModel.Refreshed += delegate { RestoreTopItem(); };
            ViewModel.Loaded += ViewModel_Loaded;
        }

        private void ViewModel_Loaded(object sender, EventArgs e)
        {
            if (StateProvider.IsOpened)
            {
                Untombstone();
            }
        }

        public void Tombstone()
        {
            SaveTopItem();
            var state = StateProvider.State;
            state[IS_TOP_ITEM_JOURNAL] = isTopItemJournal;
            state[IS_TOP_ITEM_DATE] = isTopItemDate;
            state[TOP_ITEM_JOURNAL_ID] = topItemJournalId;
            state[TOP_ITEM_DATE] = topItemDate;
            SetStateProvider();
            ViewModel.Tombstone();
        }

        private void Untombstone()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(IS_TOP_ITEM_JOURNAL))
            {
                isTopItemJournal = (bool)state[IS_TOP_ITEM_JOURNAL];
                isTopItemDate = (bool)state[IS_TOP_ITEM_DATE];
                topItemJournalId = (Guid)state[TOP_ITEM_JOURNAL_ID];
                topItemDate = (DateTime)state[TOP_ITEM_DATE];
                RestoreTopItem();
            }
            SetStateProvider();
            ViewModel.Untombstone();
        }

        private void SaveTopItem()
        {
            isTopItemJournal = false;
            isTopItemDate = false;
            var topItem = List.TopVisibleItem;
            if (topItem != null)
            {
                if (topItem is JournalItemViewModel)
                {
                    var vm = topItem as JournalItemViewModel;
                    topItemJournalId = vm.Id;
                    isTopItemJournal = true;
                }
                else
                    if (topItem is DataGroup)
                    {
                        var dataGroup = topItem as DataGroup;
                        if (dataGroup.Key is DateViewModel)
                        {
                            var date = dataGroup.Key as DateViewModel;
                            topItemDate = date.Date;
                            isTopItemDate = true;
                        }
                    }
            }
        }

        private void RestoreTopItem()
        {
            object topItem = null;
            if (isTopItemJournal)
            {
                topItem = ViewModel.FindItem(topItemJournalId);
            }
            else
                if (isTopItemDate)
                {
                    var date = ViewModel.FindDate(topItemDate);
                    var group = from dataGroup in List.Groups
                                where dataGroup.Key == date
                                select dataGroup;
                    topItem = group.FirstOrDefault();
                }
            if (topItem != null)
            {
                List.BringIntoView(topItem);
            }
        }

        private void SetStateProvider()
        {
            ViewModel.StateProvider = StateProvider;
            SugarEditing.ViewModel.StateProvider = StateProvider;
        }

        private void List_ItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            var vm = List.SelectedItem as JournalItemViewModel;
            if (vm != null)
            {
                ViewModel.Choose(vm);
            }
            Dispatcher.BeginInvoke(() =>
            {
                List.SelectedItem = null;
            });
        }

        private void List_GroupPickerItemTap(object sender, Telerik.Windows.Controls.GroupPickerItemTapEventArgs e)
        {
            (sender as RadJumpList).UniversalGroupPickerItemTap(e);
        }

        private void List_GroupHeaderItemTap(object sender, Telerik.Windows.Controls.GroupHeaderItemTapEventArgs e)
        {
            OnDatesPoppedUp();
        }

        protected void OnDatesPoppedUp()
        {
            if (DatesPoppedUp != null)
            {
                DatesPoppedUp(this, EventArgs.Empty);
            }
        }
    }
}
