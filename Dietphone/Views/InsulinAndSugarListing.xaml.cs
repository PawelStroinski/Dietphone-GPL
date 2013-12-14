using System;
using System.Linq;
using System.Windows.Controls;
using Dietphone.ViewModels;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using Dietphone.Tools;

namespace Dietphone.Views
{
    public partial class InsulinAndSugarListing : UserControl
    {
        public StateProvider StateProvider { private get; set; }
        public TelerikInsulinAndSugarListingViewModel ViewModel { get; private set; }
        public event EventHandler DatesPoppedUp;
        private bool isTopItemInsulinAndSugar;
        private bool isTopItemDate;
        private DateTime topItemInsulinAndSugarDate;
        private DateTime topItemDate;
        private const string IS_TOP_ITEM_INSULIN_AND_SUGAR = "IS_TOP_ITEM_INSULIN_AND_SUGAR";
        private const string IS_TOP_ITEM_DATE = "IS_TOP_ITEM_DATE";
        private const string TOP_ITEM_INSULIN_AND_SUGAR_DATE = "TOP_ITEM_INSULIN_AND_SUGAR_DATE";
        private const string TOP_ITEM_DATE = "TOP_ITEM_DATE";

        public InsulinAndSugarListing()
        {
            InitializeComponent();
            ViewModel = new TelerikInsulinAndSugarListingViewModel(MyApp.Factories,
                new BackgroundWorkerWrapperFactory(), SugarEditing.ViewModel);
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
            SugarEditing.ViewModel.StateProvider = StateProvider;
        }

        public void Tombstone()
        {
            SaveTopItem();
            var state = StateProvider.State;
            state[IS_TOP_ITEM_INSULIN_AND_SUGAR] = isTopItemInsulinAndSugar;
            state[IS_TOP_ITEM_DATE] = isTopItemDate;
            state[TOP_ITEM_INSULIN_AND_SUGAR_DATE] = topItemInsulinAndSugarDate;
            state[TOP_ITEM_DATE] = topItemDate;
        }

        private void Untombstone()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(IS_TOP_ITEM_INSULIN_AND_SUGAR))
            {
                isTopItemInsulinAndSugar = (bool)state[IS_TOP_ITEM_INSULIN_AND_SUGAR];
                isTopItemDate = (bool)state[IS_TOP_ITEM_DATE];
                topItemInsulinAndSugarDate = (DateTime)state[TOP_ITEM_INSULIN_AND_SUGAR_DATE];
                topItemDate = (DateTime)state[TOP_ITEM_DATE];
                RestoreTopItem();
            }
        }

        private void SaveTopItem()
        {
            isTopItemInsulinAndSugar = false;
            isTopItemDate = false;
            var topItem = List.TopVisibleItem;
            if (topItem != null)
            {
                if (topItem is ViewModelWithDate)
                {
                    var vm = topItem as ViewModelWithDate;
                    topItemInsulinAndSugarDate = vm.DateTime;
                    isTopItemInsulinAndSugar = true;
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
            if (isTopItemInsulinAndSugar)
            {
                topItem = ViewModel.FindInsulinOrSugar(topItemInsulinAndSugarDate);
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

        private void List_ItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            var vm = List.SelectedItem as ViewModelWithDateAndText;
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
