using Android.App;
using Android.OS;
using Android.Views;
using Dietphone.Tools;
using Dietphone.ViewModels;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    [Activity]
    public class ExportAndImportView : MvxActivity<ExportAndImportViewModel>
    {
        private IMenuItem exportToCloudNow;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ExportAndImportView);
            Title = Translations.ExportAndImport.Capitalize();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.exportandimportview_menu, menu);
            GetMenu(menu);
            TranslateMenu();
            return true;
        }

        private void GetMenu(IMenu menu)
        {
            exportToCloudNow = menu.FindItem(Resource.Id.exportandimportview_exporttocloudnow);
        }

        private void TranslateMenu()
        {
            exportToCloudNow.SetTitleCapitalized(Translations.ExportToDropboxNow);
        }
    }
}