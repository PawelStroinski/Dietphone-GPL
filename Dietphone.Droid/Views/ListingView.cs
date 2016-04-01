using Android.OS;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    public abstract class ListingView<TViewModel> : MvxActivity<TViewModel>
         where TViewModel : class, IMvxViewModel
    {
        protected MvxExpandableListView listView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Listing);
            listView = FindViewById<MvxExpandableListView>(Resource.Id.ListView);
        }
    }
}