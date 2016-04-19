using Android.App;
using Android.Content;
using Android.OS;
using Dietphone.Tools;
using Dietphone.ViewModels;
using MvvmCross.Platform;

namespace Dietphone.Views
{
    [Activity]
    public class MealEditingView : ActivityBase<MealEditingViewModel>
    {
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
    }
}