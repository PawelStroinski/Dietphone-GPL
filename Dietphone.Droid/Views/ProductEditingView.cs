using Android.App;
using Android.OS;
using Android.Views;
using Dietphone.Tools;
using Dietphone.ViewModels;
using MvvmCross.Platform;

namespace Dietphone.Views
{
    [Activity]
    public class ProductEditingView : ActivityBase<ProductEditingViewModel>
    {
        private IMenuItem save, cancel, delete;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            InitializeViewModel();
            SetContentView(Resource.Layout.ProductEditingView);
            Title = Translations.Product.Capitalize();
            this.InitializeTabs(Translations.General, "100g", Translations.Serving);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.producteditingview_menu, menu);
            GetMenu(menu);
            TranslateMenu();
            BindMenuActions();
            BindMenuEnabled();
            return true;
        }

        private void InitializeViewModel()
        {
            if (ViewModel.Navigator == null)
            {
                ViewModel.Navigator = Mvx.Resolve<Navigator>();
                ViewModel.Load();
            }
        }

        private void GetMenu(IMenu menu)
        {
            save = menu.FindItem(Resource.Id.producteditingview_save);
            cancel = menu.FindItem(Resource.Id.producteditingview_cancel);
            delete = menu.FindItem(Resource.Id.producteditingview_delete);
        }

        private void TranslateMenu()
        {
            save.SetTitleCapitalized(Translations.Save);
            cancel.SetTitleCapitalized(Translations.Cancel);
            delete.SetTitleCapitalized(Translations.Delete);
        }

        private void BindMenuActions()
        {
            save.SetOnMenuItemClick(ViewModel.SaveAndReturn);
            cancel.SetOnMenuItemClick(ViewModel.CancelAndReturn);
            delete.SetOnMenuItemClick(ViewModel.DeleteAndSaveAndReturn);
        }

        private void BindMenuEnabled()
        {
            save.BindSaveEnabled(ViewModel);
        }
    }
}