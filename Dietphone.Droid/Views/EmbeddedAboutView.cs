using Android.App;
using Android.OS;
using Dietphone.Tools;
using Dietphone.ViewModels;
using Android.Widget;
using System;
using System.IO;
using Android.Text;
using System.Text;
using Android.Views;

namespace Dietphone.Views
{
    [Activity]
    public class EmbeddedAboutView : ActivityBase<EmbeddedAboutViewModel>
    {
        private IMenuItem changelog;
        private const string LICENSE_PATH = "documents/license.{0}.txt";
        private const string CHANGELOG_URL = "http://www.pabloware.com/mobile/diabetes-spy.changelog.{0}.html";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EmbeddedAboutView);
            Title = Translations.About.Capitalize();
            AddLinkHandler();
            AddEmailLinkHandler();
            FormatLicense(LoadLicense());
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.embeddedaboutview_menu, menu);
            GetMenu(menu);
            BindMenuActions();
            return true;
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

        private void FormatLicense(string text)
        {
            var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var html = new StringBuilder();
            var newParagraph = true;
            foreach (var line in lines)
            {
                html.AppendLine(newParagraph
                    ? $"<font color='#{this.ResourceColorToHex(Resource.Color.extreme_foreground)}'>{line}</font><br>"
                    : $"{line}<br>");
                newParagraph = line == string.Empty;
            }
            var target = FindViewById<TextView>(Resource.Id.license_text);
            target.TextFormatted = Html.FromHtml(html.ToString());
        }

        private string LoadLicense()
        {
            var path = string.Format(LICENSE_PATH, MyApp.CurrentUiCulture);
            using (var stream = Assets.Open(path))
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        private void GetMenu(IMenu menu)
        {
            changelog = menu.FindItem(Resource.Id.embeddedaboutview_changelog);
        }

        private void BindMenuActions()
        {
            changelog.SetOnMenuItemClick(ChangelogClick);
        }

        private void ChangelogClick()
        {
            var url = string.Format(CHANGELOG_URL, MyApp.CurrentUiCulture);
            this.LaunchBrowser(url);
        }
    }
}