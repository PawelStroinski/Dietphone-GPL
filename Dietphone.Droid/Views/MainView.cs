using Android.App;
using Android.OS;
using Dietphone.ViewModels;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    [Activity]
    public class MainView : MvxActivity<MainViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MainView);
            Title = Translations.DiabetesSpy;
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            ViewModel.Untombstone();
            ViewModel.UiRendered();
        }
    }
}
