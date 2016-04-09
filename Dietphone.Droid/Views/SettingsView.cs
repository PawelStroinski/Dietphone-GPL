using Android.App;
using Android.OS;
using Dietphone.Tools;
using Dietphone.ViewModels;

namespace Dietphone.Views
{
    [Activity]
    public class SettingsView : ActivityBase<SettingsViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SettingsView);
            Title = Translations.Settings.Capitalize();
        }
    }
}