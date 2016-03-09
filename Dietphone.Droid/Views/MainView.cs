using Android.App;
using Android.OS;
using Dietphone.ViewModels;

namespace Dietphone.Droid.Views
{
    [Activity(Label = "View for MainViewModel")]
    public class MainView : ActivityBase
    {
        private new MainViewModel ViewModel { get { return (MainViewModel)base.ViewModel; } }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MainView);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            ViewModel.Untombstone();
            ViewModel.UiRendered();
        }
    }
}
