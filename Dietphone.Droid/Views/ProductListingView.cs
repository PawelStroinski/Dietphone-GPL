using System;
using Android.App;
using Android.OS;
using Dietphone.ViewModels;

namespace Dietphone.Views
{
    [Activity]
    public class ProductListingView : ListingView<GroupingProductListingViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            listView.ItemTemplateId = Resource.Layout.ProductListing_Item;
            ViewModel.Loaded += ViewModel_Loaded;
        }

        private void ViewModel_Loaded(object sender, EventArgs e)
        {
            if (ViewModel.HasMru)
                listView.ExpandGroup(0);
        }
    }
}