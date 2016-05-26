using Android.App;
using Android.OS;
using Dietphone.Tools;
using Dietphone.ViewModels;
using Android.Widget;
using System;
using System.IO;
using Android.Text;
using System.Text;

namespace Dietphone.Views
{
    [Activity]
    public class EmbeddedAboutView : ActivityBase<EmbeddedAboutViewModel>
    {
        private const string LICENSE_PATH = "documents/license.{0}.txt";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EmbeddedAboutView);
            Title = Translations.About.Capitalize();
            AddLinkHandler();
            AddEmailLinkHandler();
            FormatLicense(LoadLicense());
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
    }
}