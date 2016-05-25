using System;
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
        void GoToEmbeddedAbout();
        void GoToExportAndImport();
        void GoToSettings();
    }

    public class NavigatorImpl : MvxNavigatingObject, Navigator
    {
        private readonly NavigationService service;
        private readonly GoingToAbout about;
        private const string ABOUT_MAIL = "wp7@pabloware.com";
        private const string ABOUT_PATH_TO_LICENSE = "/Dietphone.Phone.Rarely;component/documents/license.{0}.txt";
        private const string ABOUT_CHANGELOG_URL = "http://www.pabloware.com/wp7/diabetes-spy.changelog.{0}.xaml";
        private const string ABOUT_URL = "http://www.pabloware.com/wp7";
        private const string ABOUT_PUBLISHER = "Pabloware";

        public NavigatorImpl(NavigationService service)
        {
            this.service = service;
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
            var navigation = new ProductEditingViewModel.Navigation { ProductIdToEdit = productId };
            ShowViewModel<ProductEditingViewModel>(navigation);
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
            var navigation = new MainViewModel.Navigation();
            ShowViewModel<MainViewModel>(navigation);
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

        public void GoToEmbeddedAbout()
        {
            ShowViewModel<EmbeddedAboutViewModel>();
        }

        public void GoToExportAndImport()
        {
            ShowViewModel<ExportAndImportViewModel>();
        }

        public void GoToSettings()
        {
            ShowViewModel<SettingsViewModel>();
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