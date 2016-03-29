using Android.App;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Dietphone.Tools;
using Dietphone.ViewModels;
using Dietphone.Views.Adapters;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    [Activity]
    public class ExportAndImportView : MvxActivity<ExportAndImportViewModel>
    {
        private IMenuItem exportToCloudNow;
        private WebView browser;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ExportAndImportView);
            Title = Translations.ExportAndImport.Capitalize();
            InitializeBrowser();
            InitializeViewModel();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.exportandimportview_menu, menu);
            GetMenu(menu);
            TranslateMenu();
            return true;
        }

        private void InitializeBrowser()
        {
            browser = FindViewById<WebView>(Resource.Id.browser);
            var settings = browser.Settings;
            settings.JavaScriptEnabled = true;
            var listener = new WebViewListener(ViewModel.BrowserIsNavigating);
            browser.SetWebViewClient(listener);
        }

        private void InitializeViewModel()
        {
            ViewModel.NavigateInBrowser += ViewModel_NavigateInBrowser;
        }

        private void GetMenu(IMenu menu)
        {
            exportToCloudNow = menu.FindItem(Resource.Id.exportandimportview_exporttocloudnow);
        }

        private void TranslateMenu()
        {
            exportToCloudNow.SetTitleCapitalized(Translations.ExportToDropboxNow);
        }

        private void ViewModel_NavigateInBrowser(object sender, string e)
        {
            browser.LoadUrl(e);
        }
    }
}