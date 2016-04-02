using System;
using System.Collections;
using Android.OS;
using Dietphone.ViewModels;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    public abstract class ListingView<TViewModel> : MvxActivity<TViewModel>
         where TViewModel : SearchSubViewModel
    {
        protected MvxExpandableListView listView;
        private bool[] expandState;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Listing);
            listView = FindViewById<MvxExpandableListView>(Resource.Id.ListView);
            ViewModel.DescriptorsUpdating += ViewModel_DescriptorsUpdating;
            ViewModel.DescriptorsUpdated += ViewModel_DescriptorsUpdated;
        }

        private void ViewModel_DescriptorsUpdating(object sender, EventArgs e)
        {
            if (IsSearching)
                SaveExpandState();
        }

        private void ViewModel_DescriptorsUpdated(object sender, EventArgs e)
        {
            if (IsSearching)
                ExpandAll();
            else
                RestoreExpandState();
        }

        private void SaveExpandState()
        {
            if (expandState != null)
                return;
            expandState = new bool[Count];
            for (int i = 0; i < Count; i++)
                expandState[i] = listView.IsGroupExpanded(i);
        }

        private void ExpandAll()
        {
            for (int i = 0; i < Count; i++)
                listView.ExpandGroup(i);
        }

        private void RestoreExpandState()
        {
            if (expandState == null)
                return;
            for (int i = 0; i < Count; i++)
                if (i < expandState.Length && expandState[i])
                    listView.ExpandGroup(i);
                else
                    listView.CollapseGroup(i);
            expandState = null;
        }

        private bool IsSearching => !string.IsNullOrEmpty(ViewModel.Search);

        private int Count
        {
            get
            {
                var groups = (ICollection)listView.ItemsSource;
                return groups != null ? groups.Count : 0;
            }
        }
    }
}