using Android.App;
using Android.OS;
using Dietphone.Tools;
using Dietphone.ViewModels;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    [Activity]
    public class ExportAndImportView : MvxActivity<ExportAndImportViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ExportAndImportView);
            Title = Translations.ExportAndImport.Capitalize();
        }
    }
}