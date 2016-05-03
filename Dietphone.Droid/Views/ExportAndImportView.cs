using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Dietphone.Tools;
using Dietphone.ViewModels;
using Dietphone.Adapters;

namespace Dietphone.Views
{
    [Activity]
    public class ExportAndImportView : ActivityBase<ExportAndImportViewModel>
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
            BindMenuActions();
            SetExportToCloudNowEnabledDependingOnIsExportToCloudActive();
            return true;
        }

        public override void OnBackPressed()
        {
            if (ViewModel.BrowserVisible)
                ViewModel.BrowserVisible = false;
            else
                base.OnBackPressed();
        }

        private void InitializeBrowser()
        {
            browser = FindViewById<WebView>(Resource.Id.browser);
            var settings = browser.Settings;
            settings.JavaScriptEnabled = true;
            var client = new WebViewClientImpl(ViewModel.BrowserIsNavigating);
            browser.SetWebViewClient(client);
        }

        private void InitializeViewModel()
        {
            ViewModel.NavigateInBrowser += ViewModel_NavigateInBrowser;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void GetMenu(IMenu menu)
        {
            exportToCloudNow = menu.FindItem(Resource.Id.exportandimportview_exporttocloudnow);
        }

        private void TranslateMenu()
        {
            exportToCloudNow.SetTitleCapitalized(Translations.ExportToDropboxNow);
        }

        private void BindMenuActions()
        {
            exportToCloudNow.SetOnMenuItemClick(() => ViewModel.ExportToCloudNow());
        }

        private void SetExportToCloudNowEnabledDependingOnIsExportToCloudActive()
        {
            exportToCloudNow.SetEnabled(ViewModel.IsExportToCloudActive);
        }

        private void ViewModel_NavigateInBrowser(object sender, string e)
        {
            browser.LoadUrl(e);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsExportToCloudActive")
                SetExportToCloudNowEnabledDependingOnIsExportToCloudActive();
            if (e.PropertyName == "ImportFromCloudVisible")
                ViewModel_ImportFromCloudVisibleChanged();
        }

        private void ViewModel_ImportFromCloudVisibleChanged()
        {
            if (ViewModel.ImportFromCloudVisible)
                ShowImportFromCloud();
        }

        private void ShowImportFromCloud()
        {
            new AlertDialog.Builder(this)
                .SetTitle(Translations.AvailableForImport)
                .SetItems(ViewModel.ImportFromCloudItems.ToArray(), ImportFromCloud_Click)
                .Create()
                .Show();
        }

        private void ImportFromCloud_Click(object sender, DialogClickEventArgs e)
        {
            CheckImportFromCloudClickArgs(e);
            ViewModel.ImportFromCloudSelectedItem = ViewModel.ImportFromCloudItems[e.Which];
            ViewModel.ImportFromCloudWithSelection.Execute(null);
        }

        private void CheckImportFromCloudClickArgs(DialogClickEventArgs e)
        {
            if (e.Which < 0 || e.Which >= ViewModel.ImportFromCloudItems.Count)
                throw new ArgumentOutOfRangeException("e.Which");
        }
    }
}