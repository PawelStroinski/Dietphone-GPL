using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Dietphone.ViewModels;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    [Activity]
    public class MainView : MvxTabActivity<MainViewModel>
    {
        private IMenuItem add, meal, sugar, insulin, search;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MainView);
            Title = string.Empty;
            AddTabs();
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            ViewModel.Untombstone();
            ViewModel.UiRendered();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.mainview_menu, menu);
            add = menu.FindItem(Resource.Id.mainview_add);
            meal = menu.FindItem(Resource.Id.mainview_meal);
            sugar = menu.FindItem(Resource.Id.mainview_sugar);
            insulin = menu.FindItem(Resource.Id.mainview_insulin);
            search = menu.FindItem(Resource.Id.mainview_search);
            meal.SetTitle(Translations.Meal);
            sugar.SetTitle(Translations.Sugar);
            insulin.SetTitle(Translations.Insulin);
            search.SetTitle(Translations.Search);
            var searchView = (SearchView)search.ActionView;
            searchView.QueryTextChange += SearchView_QueryTextChange;
            //search.SetOnActionExpandListener(new SearchExpandListener(menu));
            return true;
        }

        private void AddTabs()
        {
            AddJournalTab();
            AddProductListingTab();
        }

        private void SearchView_QueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            ViewModel.Search = e.NewText;
            e.Handled = true;
        }

        private void AddJournalTab()
        {
            var tab = TabHost.NewTabSpec("journal");
            tab.SetIndicator(Translations.Journal);
            tab.SetContent(this.CreateIntentFor(ViewModel.Journal));
            TabHost.AddTab(tab);
        }

        private void AddProductListingTab()
        {
            var tab = TabHost.NewTabSpec("productListing");
            tab.SetIndicator(Translations.Products);
            tab.SetContent(this.CreateIntentFor(ViewModel.ProductListing));
            TabHost.AddTab(tab);
        }
    }
}
