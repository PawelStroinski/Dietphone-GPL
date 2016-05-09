using Android.App;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Dietphone.Tools;
using Dietphone.ViewModels;
using MvvmCross.Platform;

namespace Dietphone.Views
{
    [Activity]
    public class InsulinEditingView : ActivityBase<InsulinEditingViewModel>
    {
        private IMenuItem save, cancel, meal, copy, delete;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            InitializeViewModel();
            SetContentView(Resource.Layout.InsulinEditingView);
            Title = Translations.Insulin.Capitalize();
            this.InitializeTabs(Translations.General, Translations.Suggestion, Translations.Date);
            FormatSuggestedInsulinHeader();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            ViewModel.ReturnedFromNavigation();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.insulineditingview_menu, menu);
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

        private void FormatSuggestedInsulinHeader()
        {
            var html = $@"
                <font color='#{this.ResourceColorToHex(Resource.Color.extreme_foreground)}'>
                    {Translations.SuggestedInsulinHeaderWarning}
                </font>
                {Translations.SuggestedInsulinHeader}
                <font color='#ff0000'>
                    {Translations.SuggestedInsulinHeaderWarning2}
                </font>";
            var target = FindViewById<TextView>(Resource.Id.suggested_insulin_header);
            target.TextFormatted = Html.FromHtml(html);
        }

        private void GetMenu(IMenu menu)
        {
            save = menu.FindItem(Resource.Id.insulineditingview_save);
            cancel = menu.FindItem(Resource.Id.insulineditingview_cancel);
            meal = menu.FindItem(Resource.Id.insulineditingview_meal);
            copy = menu.FindItem(Resource.Id.insulineditingview_copy);
            delete = menu.FindItem(Resource.Id.insulineditingview_delete);
        }

        private void TranslateMenu()
        {
            save.SetTitleCapitalized(Translations.Save);
            cancel.SetTitleCapitalized(Translations.Cancel);
            meal.SetTitleCapitalized(Translations.Meal);
            copy.SetTitleCapitalized(Translations.Copy);
            delete.SetTitleCapitalized(Translations.Delete);
        }

        private void BindMenuActions()
        {
            save.SetOnMenuItemClick(ViewModel.SaveAndReturn);
            cancel.SetOnMenuItemClick(ViewModel.CancelAndReturn);
            meal.SetOnMenuItemClick(ViewModel.GoToMealEditing);
            copy.SetOnMenuItemClick(ViewModel.CopyAsText);
            delete.SetOnMenuItemClick(ViewModel.DeleteAndSaveAndReturn);
        }

        private void BindMenuEnabled()
        {
            save.BindSaveEnabled(ViewModel);
        }
    }
}