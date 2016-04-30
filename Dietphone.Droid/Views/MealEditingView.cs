using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Dietphone.Tools;
using Dietphone.ViewModels;
using MvvmCross.Platform;

namespace Dietphone.Views
{
    [Activity]
    public class MealEditingView : ActivityBase<MealEditingViewModel>
    {
        private IMenuItem save, cancel, insulin, delete;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            InitializeViewModel();
            SetContentView(Resource.Layout.MealEditingView);
            Title = Translations.Meal.Capitalize();
            this.InitializeTabs(Translations.Ingredients, Translations.General);
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            ViewModel.ReturnedFromNavigation();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.mealeditingview_menu, menu);
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
            else
                ViewModel.ReturnedFromNavigation();
        }

        private void GetMenu(IMenu menu)
        {
            save = menu.FindItem(Resource.Id.mealeditingview_save);
            cancel = menu.FindItem(Resource.Id.mealeditingview_cancel);
            insulin = menu.FindItem(Resource.Id.mealeditingview_insulin);
            delete = menu.FindItem(Resource.Id.mealeditingview_delete);
        }

        private void TranslateMenu()
        {
            save.SetTitleCapitalized(Translations.Save);
            cancel.SetTitleCapitalized(Translations.Cancel);
            insulin.SetTitleCapitalized(Translations.Insulin);
            delete.SetTitleCapitalized(Translations.Delete);
        }

        private void BindMenuActions()
        {
            save.SetOnMenuItemClick(ViewModel.SaveAndReturn);
            cancel.SetOnMenuItemClick(ViewModel.CancelAndReturn);
            insulin.SetOnMenuItemClick(ViewModel.GoToInsulinEditing);
            delete.SetOnMenuItemClick(ViewModel.DeleteAndSaveAndReturn);
        }

        private void BindMenuEnabled()
        {
            save.BindSaveEnabled(ViewModel);
        }
    }
}