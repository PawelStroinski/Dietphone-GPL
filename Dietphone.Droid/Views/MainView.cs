using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Dietphone.Tools;
using Dietphone.ViewModels;
using Dietphone.Adapters;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;

namespace Dietphone.Views
{
    [Activity]
    public class MainView : MvxTabActivity<MainViewModel>
    {
        private SubViewModelConnector subConnector;
        private IMenuItem meal, sugar, insulin, add, search, settings, exportAndImportData, about, welcomeScreen;
        private SearchView searchView;
        private bool searchExpanded, showProductsOnly;
        private const string JOURNAL_TAB = "journal";
        private const string PRODUCTS_TAB = "products";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MainView);
            Title = string.Empty;
            InitializeViewModel();
            InitializeTabs();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            InitializeViewModel();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.mainview_menu, menu);
            GetMenu(menu);
            TranslateMenu();
            InitializeSearchMenu();
            BindMenuActions();
            SetMenuVisisiblity();
            return true;
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            this.HideSoftInputOnTouchOutside(ev, GetGlobalVisibleRect);
            return base.DispatchTouchEvent(ev);
        }

        private void InitializeViewModel()
        {
            var fullInitialization = subConnector == null;
            if (fullInitialization)
            {
                subConnector = new SubViewModelConnector(ViewModel);
                subConnector.Loaded += delegate { ViewModel.UiRendered(); };
                var welcome = ViewModel.WelcomeScreen;
                welcome.LaunchBrowser += delegate { this.LaunchBrowser(Translations.WelcomeScreenLinkDroid); };
            }
            ViewModel.ShowProductsOnly += delegate { showProductsOnly = true; };
            ViewModel.Untombstone();
            var navigator = Mvx.Resolve<Navigator>();
            subConnector.Navigator = navigator;
            subConnector.Refresh();
            ViewModel.Navigator = navigator;
        }

        private void InitializeTabs()
        {
            if (!showProductsOnly)
                AddJournalTab();
            AddProductsTab();
            TabHost.TabChanged += TabHost_TabChanged;
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
            settings.SetOnMenuItemClick(() => ViewModel.Settings());
            var welcome = ViewModel.WelcomeScreen;
            welcomeScreen.SetOnMenuItemClick(welcome.Show);
            about.SetOnMenuItemClick(ViewModel.EmbeddedAbout);
            BindAddMenuAction(meal, new JournalViewModel.AddMealCommand());
            BindAddMenuAction(sugar, new JournalViewModel.AddSugarCommand());
            BindAddMenuAction(insulin, new JournalViewModel.AddInsulinCommand());
            BindAddMenuAction(add, new DefaultAddCommand());
        }

        private Rect GetGlobalVisibleRect(View view)
        {
            var rect = new Rect();
            if (searchView.IsParentOf(view))
                searchView.GetGlobalVisibleRect(rect);
            else
                view.GetGlobalVisibleRect(rect);
            return rect;
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
            SetMenuVisisiblity();
            SetSubViewModel();
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

        private void BindAddMenuAction(IMenuItem item, AddCommand command)
        {
            item.SetOnMenuItemClick(() => subConnector.Add(command));
        }

        private void SetMenuVisisiblity()
        {
            if (meal == null)
                return;
            var inJournalTab = TabHost.CurrentTabTag == JOURNAL_TAB;
            meal.SetVisible(inJournalTab && !searchExpanded);
            sugar.SetVisible(inJournalTab && !searchExpanded);
            insulin.SetVisible(inJournalTab && !searchExpanded);
            add.SetVisible(!inJournalTab && !searchExpanded);
        }
    }
}
