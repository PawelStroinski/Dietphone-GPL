using Android.App;
using Android.OS;
using Dietphone.Tools;
using Dietphone.ViewModels;
using Android.Widget;

namespace Dietphone.Views
{
    [Activity]
    public class EmbeddedAboutView : ActivityBase<EmbeddedAboutViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EmbeddedAboutView);
            Title = Translations.About.Capitalize();
            AddLinkHandler();
            AddEmailLinkHandler();
        }

        private void AddLinkHandler()
        {
            var button = FindViewById<Button>(Resource.Id.about_link);
            button.Click += delegate { this.LaunchBrowser(button.Text); };
        }

        private void AddEmailLinkHandler()
        {
            var button = FindViewById<Button>(Resource.Id.about_link_email);
            button.Click += delegate { this.LaunchEmail(button.Text); };
        }
    }
}