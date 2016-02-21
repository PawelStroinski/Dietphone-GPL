using Dietphone.Views;

namespace Dietphone.ViewModels
{
    public class CloudMessages
    {
        public string ConfirmExportToCloudDeactivation { get; set; }
        public string ExportToCloudActivationSuccessful { get; set; }
        public string ExportToCloudSuccessful { get; set; }
        public string ImportFromCloudSuccessful { get; set; }
        public string CloudError { get; set; }
        public string ExportToCloudError { get; set; }

        public CloudMessages()
        {
            ConfirmExportToCloudDeactivation = Translations.ExportToDropboxIsActiveDoYouWantToTurnItOff;
            ExportToCloudActivationSuccessful = Translations.ExportToDropboxActivationWasSuccessful;
            ExportToCloudSuccessful = Translations.ExportToDropboxWasSuccessful;
            ImportFromCloudSuccessful = Translations.ImportCompletedSuccessfully;
            CloudError = Translations.AnErrorOccurredDuringTheDropboxOperation;
            ExportToCloudError = Translations.ThereWasAnErrorDuringTheExportToDropbox;
        }
    }
}
