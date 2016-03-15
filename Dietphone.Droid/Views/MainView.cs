using System;
using Android.App;
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
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MainView);
            Title = Translations.DiabetesSpy;
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
            AddSearchMenuItem(menu);
            return true;
        }

        private void AddTabs()
        {
            AddJournalTab();
            AddProductListingTab();
        }

        private void AddSearchMenuItem(IMenu menu)
        {
            var item = menu.Add(Translations.Search);
            var searchView = new SearchView(ActionBar.ThemedContext);
            item.SetActionView(searchView);
            item.SetIcon(Resource.Drawable.ic_search_white_24dp);
            item.SetShowAsAction(ShowAsAction.CollapseActionView | ShowAsAction.IfRoom);
            //item.SetOnMenuItemClickListener(new MenuItemClickAdapter(() => SearchMenuItemClick()));
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

        private void SearchMenuItemClick()
        {
        }
    }
}
