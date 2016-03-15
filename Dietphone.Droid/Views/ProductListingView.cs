using Android.App;
using Android.OS;
using Dietphone.ViewModels;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    [Activity]
    public class ProductListingView : MvxActivity<ProductListingViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ProductListingView);
        }
    }
}