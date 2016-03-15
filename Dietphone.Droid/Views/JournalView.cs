using Android.App;
using Android.OS;
using Dietphone.ViewModels;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    [Activity]
    public class JournalView : MvxActivity<JournalViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.JournalView);
        }
    }
}