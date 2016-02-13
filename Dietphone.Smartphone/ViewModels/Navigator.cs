using System;
using System.Collections.Generic;
using Dietphone.Tools;
using Dietphone.Views;
using MvvmCross.Core.ViewModels;
using Pabloware.About;

namespace Dietphone.ViewModels
{
    public interface Navigator
    {
        void GoBack();
        void GoToMealEditing(Guid mealId);
        void GoToProductEditing(Guid productId);
        void GoToInsulinEditing(Guid insulinId);
        void GoToNewInsulin();
        void GoToNewInsulinRelatedToMeal(Guid mealId);
        void GoToInsulinEditingRelatedToMeal(Guid insulinId, Guid mealId);
        void GoToMain();
        void GoToMainToAddMealItem();
        void GoToAbout();
        void GoToExportAndImport();
        void GoToSettings();
        Guid GetProductIdToEdit();
    }

    public enum Assembly { Default, Sometimes, Rarely };

    public class NavigatorImpl : MvxNavigatingObject, Navigator
    {
        private string path;
        private string idName;
        private Guid idValue;
        private Assembly assembly;
        private readonly NavigationService service;
        private readonly IDictionary<string, string> passedQueryString;
        private readonly GoingToAbout about;
        private const string PRODUCT_ID_TO_EDIT = "ProductIdToEdit";
        private const string ABOUT_MAIL = "wp7@pabloware.com";
        private const string ABOUT_PATH_TO_LICENSE = "/Dietphone.Phone.Rarely;component/documents/license.{0}.txt";
        private const string ABOUT_CHANGELOG_URL = "http://www.pabloware.com/wp7/diabetes-spy.changelog.{0}.xaml";
        private const string ABOUT_URL = "http://www.pabloware.com/wp7";
        private const string ABOUT_PUBLISHER = "Pabloware";

        public NavigatorImpl(NavigationService service, NavigationContext context)
        {
            this.service = service;
            passedQueryString = context.QueryString;
            about = new GoingToAbout(service);
        }

        public void GoBack()
        {
            if (service.CanGoBack)
            {
                service.GoBack();
            }
        }

        public void GoToMealEditing(Guid mealId)
        {
            var navigation = new MealEditingViewModel.Navigation { MealIdToEdit = mealId };
            ShowViewModel<MealEditingViewModel>(navigation);
        }

        public void GoToProductEditing(Guid productId)
        {
            idName = PRODUCT_ID_TO_EDIT;
            idValue = productId;
            path = "/Views/ProductEditing.xaml";
            assembly = Assembly.Sometimes;
            NavigateWithId();
        }

        public void GoToInsulinEditing(Guid insulinId)
        {
            var navigation = new InsulinEditingViewModel.Navigation { InsulinIdToEdit = insulinId };
            ShowViewModel<InsulinEditingViewModel>(navigation);
        }

        public void GoToNewInsulin()
        {
            var navigation = new InsulinEditingViewModel.Navigation();
            ShowViewModel<InsulinEditingViewModel>(navigation);
        }

        public void GoToNewInsulinRelatedToMeal(Guid mealId)
        {
            var navigation = new InsulinEditingViewModel.Navigation { RelatedMealId = mealId };
            ShowViewModel<InsulinEditingViewModel>(navigation);
        }

        public void GoToInsulinEditingRelatedToMeal(Guid insulinId, Guid mealId)
        {
            var navigation = new InsulinEditingViewModel.Navigation
            {
                InsulinIdToEdit = insulinId,
                RelatedMealId = mealId
            };
            ShowViewModel<InsulinEditingViewModel>(navigation);
        }

        public void GoToMain()
        {
            path = "/Views/Main.xaml";
            assembly = Assembly.Default;
            Navigate();
        }

        public void GoToMainToAddMealItem()
        {
            var navigation = new MainViewModel.Navigation { ShouldAddMealItem = true };
            ShowViewModel<MainViewModel>(navigation);
        }

        public void GoToAbout()
        {
            FillAboutDto();
            about.Go();
        }

        public void GoToExportAndImport()
        {
            path = "/Views/ExportAndImport.xaml";
            assembly = Assembly.Rarely;
            Navigate();
        }

        public void GoToSettings()
        {
            path = "/Views/Settings.xaml";
            assembly = Assembly.Rarely;
            Navigate();
        }

        public Guid GetProductIdToEdit()
        {
            idName = PRODUCT_ID_TO_EDIT;
            return GetId();
        }

        private Guid GetId()
        {
            if (passedQueryString.ContainsKey(idName))
            {
                return new Guid(passedQueryString[idName]);
            }
            else
            {
                return Guid.Empty;
            }
        }

        private void Navigate()
        {
            var destination = new UriBuilder();
            destination.Path = path;
            Navigate(destination);
        }

        private void NavigateWithId()
        {
            var destination = new UriBuilder();
            destination.Path = path;
            destination.Query = String.Format("{0}={1}", idName, idValue);
            Navigate(destination);
        }

        private void Navigate(UriBuilder destination)
        {
            destination.Scheme = string.Empty;
            destination.Host = GetAssemblyPrefix();
            var uri = new Uri(destination.ToString(), UriKind.Relative);
            service.Navigate(uri);
        }

        private string GetAssemblyPrefix()
        {
            switch (assembly)
            {
                case Assembly.Sometimes:
                    return "/Dietphone.Phone.Sometimes;component";
                case Assembly.Rarely:
                    return "/Dietphone.Phone.Rarely;component";
                default:
                    return string.Empty;
            }
        }

        private void FillAboutDto()
        {
            var dto = about.Dto;
            var appVersion = new AppVersion();
            dto.AppName = Translations.DiabetesSpyTitleCase;
            dto.Version = appVersion.GetAppVersion();
            dto.Mail = ABOUT_MAIL;
            dto.Url = ABOUT_URL;
            dto.Publisher = ABOUT_PUBLISHER;
            dto.PathToLicense = ABOUT_PATH_TO_LICENSE;
            dto.ChangelogUrl = ABOUT_CHANGELOG_URL;
            dto.UiCulture = MyApp.CurrentUiCulture;
        }
    }
}