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
        }
    }
}