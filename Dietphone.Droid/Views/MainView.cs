using System.ComponentModel;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Dietphone.Tools;
using Dietphone.ViewModels;
using Dietphone.Views.Adapters;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    [Activity]
    public class MainView : TabActivityBase<MainViewModel>
    {
        private SubViewModelConnector subConnector;
        private IMenuItem meal, sugar, insulin, add, search, settings, exportAndImportData, about, welcomeScreen;
        private SearchView searchView;
        private bool searchExpanded;
        private const string JOURNAL_TAB = "journal";
        private const string PRODUCTS_TAB = "products";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MainView);
            Title = string.Empty;
            InitializeTabs();
            InitializeViewModel();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.mainview_menu, menu);
            GetMenu(menu);
            TranslateMenu();
            InitializeSearchMenu();
            BindMenuActions();
            return true;
        }

        protected override Rect GetGlobalVisibleRect(View view)
        {
            var rect = new Rect();
            if (searchView.IsParentOf(view))
                searchView.GetGlobalVisibleRect(rect);
            else
                view.GetGlobalVisibleRect(rect);
            return rect;
        }

        private void InitializeTabs()
        {
            AddJournalTab();
            AddProductsTab();
            TabHost.CurrentTab = ViewModel.Pivot;
            TabHost.TabChanged += TabHost_TabChanged;
        }

        private void InitializeViewModel()
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            subConnector = new SubViewModelConnector(ViewModel);
            subConnector.Loaded += delegate { ViewModel.UiRendered(); };
            ViewModel.Untombstone();
            var navigator = new NavigatorImpl(new NavigationServiceImpl());
            subConnector.Navigator = navigator;
            subConnector.Refresh();
            ViewModel.Navigator = navigator;
            SetSubViewModel();
        }

        private void GetMenu(IMenu menu)
        {
            meal = menu.FindItem(Resource.Id.mainview_meal);
            sugar = menu.FindItem(Resource.Id.mainview_sugar);
            insulin = menu.FindItem(Resource.Id.mainview_insulin);
            add = menu.FindItem(Resource.Id.mainview_add);
            search = menu.FindItem(Resource.Id.mainview_search);
            settings = menu.FindItem(Resource.Id.mainview_settings);
            exportAndImportData = menu.FindItem(Resource.Id.mainview_exportandimportdata);
            about = menu.FindItem(Resource.Id.mainview_about);
            welcomeScreen = menu.FindItem(Resource.Id.mainview_welcomescreen);
        }

        private void TranslateMenu()
        {
            meal.SetTitleCapitalized(Translations.Meal);
            sugar.SetTitleCapitalized(Translations.Sugar);
            insulin.SetTitleCapitalized(Translations.Insulin);
            add.SetTitleCapitalized(Translations.Add);
            search.SetTitleCapitalized(Translations.Search);
            settings.SetTitleCapitalized(Translations.Settings);
            exportAndImportData.SetTitleCapitalized(Translations.ExportAndImportData);
            about.SetTitleCapitalized(Translations.About);
            welcomeScreen.SetTitleCapitalized(Translations.WelcomeScreen);
        }

        private void InitializeSearchMenu()
        {
            searchView = (SearchView)search.ActionView;
            searchView.QueryTextChange += SearchView_QueryTextChange;
            search.SetOnActionExpandListener(new ActionExpandListener(() => Search_Expand(), () => Search_Collapse()));
        }

        private void BindMenuActions()
        {
            exportAndImportData.SetOnMenuItemClick(() => ViewModel.ExportAndImport());
        }

        private void AddJournalTab()
        {
            var tab = TabHost.NewTabSpec(JOURNAL_TAB);
            tab.SetIndicator(Translations.Journal);
            tab.SetContent(this.CreateIntentFor(ViewModel.Journal));
            TabHost.AddTab(tab);
        }

        private void AddProductsTab()
        {
            var tab = TabHost.NewTabSpec(PRODUCTS_TAB);
            tab.SetIndicator(Translations.Products);
            tab.SetContent(this.CreateIntentFor(ViewModel.ProductListing));
            TabHost.AddTab(tab);
        }

        private void TabHost_TabChanged(object sender, TabHost.TabChangeEventArgs e)
        {
            ViewModel.Pivot = TabHost.CurrentTab;
            SetMenuVisisiblity();
            SetSubViewModel();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Pivot")
                TabHost.CurrentTab = ViewModel.Pivot;
        }

        private void SetSubViewModel()
        {
            if (TabHost.CurrentTabTag == JOURNAL_TAB)
                subConnector.SubViewModel = ViewModel.Journal;
            else if (TabHost.CurrentTabTag == PRODUCTS_TAB)
                subConnector.SubViewModel = ViewModel.ProductListing;
        }

        private void SearchView_QueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            ViewModel.Search = e.NewText;
            e.Handled = true;
        }

        private void Search_Expand()
        {
            searchExpanded = true;
            SetMenuVisisiblity();
        }

        private void Search_Collapse()
        {
            searchExpanded = false;
            SetMenuVisisiblity();
        }

        private void SetMenuVisisiblity()
        {
            var inJournalTab = TabHost.CurrentTabTag == JOURNAL_TAB;
            meal.SetVisible(inJournalTab && !searchExpanded);
            sugar.SetVisible(inJournalTab && !searchExpanded);
            insulin.SetVisible(inJournalTab && !searchExpanded);
            add.SetVisible(!inJournalTab && !searchExpanded);
        }
    }
}
