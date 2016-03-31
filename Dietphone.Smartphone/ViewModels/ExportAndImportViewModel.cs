using System;
using Dietphone.Models;
using System.ComponentModel;
using Dietphone.Tools;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using Dietphone.Views;

namespace Dietphone.ViewModels
{
    public class ExportAndImportViewModel : ViewModelBase
    {
        public string Email { private get; set; }
        public string Url { private get; set; }
        public List<string> ImportFromCloudItems { get; private set; }
        public string ImportFromCloudSelectedItem { get; set; }
        public event EventHandler<string> NavigateInBrowser;
        private string data;
        private bool isBusy;
        private bool browserVisible;
        private bool importFromCloudVisible;
        private bool readingFailedDuringImportFromAddress;
        private bool browserIsNavigatingDoWorkWentOkay;
        private bool exportToCloudNowWentOkay;
        private bool importFromCloudWentOkay;
        private Task<HttpResponseMessage> downloadFromAdress;
        private CloudProvider cloudProvider;
        private BrowserIsNavigatingHint browserIsNavigatingHint;
        private readonly Factories factories;
        private readonly ExportAndImport exportAndImport;
        private readonly CloudProviderFactory cloudProviderFactory;
        private readonly Vibration vibration;
        private readonly Cloud cloud;
        private readonly MessageDialog messageDialog;
        private readonly CloudMessages cloudMessages;
        private const string MAILEXPORT_URL = "http://www.bizmaster.pl/varia/dietphone/MailExport.aspx";
        private const string MAILEXPORT_SUCCESS_RESULT = "Success!";
        internal const string TOKEN_ACQUIRING_CALLBACK_URL = "http://localhost/HelloTestingSuccess";
        internal const string TOKEN_ACQUIRING_NAVIGATE_AWAY_URL = "about:blank";
        public const string INITIAL_URL = "http://";

        public ExportAndImportViewModel(Factories factories, CloudProviderFactory cloudProviderFactory,
            Vibration vibration, Cloud cloud, MessageDialog messageDialog, CloudMessages cloudMessages)
        {
            this.factories = factories;
            exportAndImport = new ExportAndImportImpl(factories);
            this.cloudProviderFactory = cloudProviderFactory;
            this.vibration = vibration;
            this.cloud = cloud;
            this.messageDialog = messageDialog;
            this.cloudMessages = cloudMessages;
        }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public bool BrowserVisible
        {
            get
            {
                return browserVisible;
            }
            set
            {
                browserVisible = value;
                OnPropertyChanged("BrowserVisible");
            }
        }

        public bool ImportFromCloudVisible
        {
            get
            {
                return importFromCloudVisible;
            }
            set
            {
                importFromCloudVisible = value;
                OnPropertyChanged("ImportFromCloudVisible");
            }
        }

        public bool IsExportToCloudActive
        {
            get
            {
                var settings = factories.Settings;
                return settings.CloudSecret != string.Empty || settings.CloudToken != string.Empty;
            }
        }

        public ICommand AskToExportToEmail
        {
            get
            {
                return new MvxCommand(() =>
                {
                    Email = messageDialog.Input(string.Empty, caption: Translations.SendToAnEMailAddress,
                        value: string.Empty, type: InputType.Email);
                    if (!string.IsNullOrEmpty(Email))
                        ExportToEmail();
                });
            }
        }

        public void ExportToEmail()
        {
            if (IsBusy)
            {
                return;
            }
            if (!Email.IsValidEmail())
            {
                OnSendingFailedDuringExportToEmail();
                return;
            }
            var worker = new VerboseBackgroundWorker();
            worker.DoWork += delegate
            {
                data = exportAndImport.Export();
            };
            worker.RunWorkerCompleted += delegate
            {
                SendByEmail();
            };
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        public ICommand AskToImportFromAddress
        {
            get
            {
                return new MvxCommand(() =>
                {
                    Url = messageDialog.Input(string.Empty, caption: Translations.DownloadFileFromAddress,
                        value: INITIAL_URL, type: InputType.Url);
                    if (!string.IsNullOrEmpty(Url))
                        ImportFromAddress();
                });
            }
        }

        public void ImportFromAddress()
        {
            if (IsBusy)
            {
                return;
            }
            if (!Url.IsValidUri())
            {
                OnDownloadingFailedDuringImportFromAddress();
                return;
            }
            var worker = new VerboseBackgroundWorker();
            worker.DoWork += delegate
            {
                DownloadFromAddress();
            };
            worker.RunWorkerCompleted += delegate
            {
                DownloadFromAddressCompleted();
            };
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        public ICommand ExportToCloud
        {
            get
            {
                return new MvxCommand(() =>
                {
                    vibration.VibrateOnButtonPress();
                    if (IsExportToCloudActive)
                        DeactivateExportToCloud();
                    else
                        ActivateExportToCloud(BrowserIsNavigatingHint.Export);
                    NotifyIsExportToCloudActive();
                });
            }
        }

        public ICommand ImportFromCloud
        {
            get
            {
                return new MvxCommand(() =>
                {
                    if (IsExportToCloudActive)
                        ShowImportFromCloudItems();
                    else
                        ActivateExportToCloud(BrowserIsNavigatingHint.Import);
                });
            }
        }

        public ICommand ImportFromCloudWithSelection
        {
            get
            {
                return new MvxCommand(() =>
                {
                    var worker = new VerboseBackgroundWorker();
                    var hasSelection = !string.IsNullOrEmpty(ImportFromCloudSelectedItem);
                    worker.DoWork += delegate
                    {
                        if (hasSelection)
                            CatchedImportFromCloud();
                    };
                    worker.RunWorkerCompleted += delegate
                    {
                        if (hasSelection && importFromCloudWentOkay)
                            OnImportFromCloudSuccessful();
                        IsBusy = false;
                    };
                    ImportFromCloudVisible = false;
                    IsBusy = true;
                    worker.RunWorkerAsync();
                });
            }
        }

        public void BrowserIsNavigating(string url)
        {
            if (!IsThisUrlTheTokenAcquiringCallbackUrl(url))
                return;
            CheckCloudProvider();
            var worker = new VerboseBackgroundWorker();
            worker.DoWork += delegate { CatchedBrowserIsNavigatingDoWork(); };
            worker.RunWorkerCompleted += delegate
            {
                BrowserVisible = false;
                NotifyIsExportToCloudActive();
                if (!browserIsNavigatingDoWorkWentOkay)
                    return;
                if (browserIsNavigatingHint == BrowserIsNavigatingHint.Export)
                    OnExportToCloudActivationSuccessful();
                if (browserIsNavigatingHint == BrowserIsNavigatingHint.Import)
                    ShowImportFromCloudItems();
            };
            worker.RunWorkerAsync();
        }

        public void ExportToCloudNow()
        {
            CheckIsExportToCloudActive();
            var worker = new VerboseBackgroundWorker();
            worker.DoWork += delegate { CatchedExportToCloudNow(); };
            worker.RunWorkerCompleted += delegate
            {
                IsBusy = false;
                if (exportToCloudNowWentOkay)
                    OnExportToCloudSuccessful();
            };
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        private void SendByEmail()
        {
            var sender = new PostSender(MAILEXPORT_URL);
            sender.Inputs["address"] = Email;
            sender.Inputs["data"] = data;
            sender.Completed += SendByEmail_Completed;
            sender.SendAsync();
        }

        private void DownloadFromAddress()
        {
            var client = new HttpClient();
            downloadFromAdress = client.GetAsync(Url);
            downloadFromAdress.Wait();
        }

        private void SendByEmail_Completed(object sender, PostSenderCompletedEventArgs e)
        {
            IsBusy = false;
            if (e.Success && e.Result == MAILEXPORT_SUCCESS_RESULT)
            {
                OnExportToEmailSuccessful();
            }
            else
            {
                OnSendingFailedDuringExportToEmail();
            }
        }

        private void DownloadFromAddressCompleted()
        {
            if (downloadFromAdress.IsGeneralSuccess() && downloadFromAdress.Result.IsSuccessStatusCode)
            {
                var bytes = downloadFromAdress.Result.Content.ReadAsByteArrayAsync().Result;
                data = Encoding.Unicode.GetString(bytes, 0, bytes.Length);
                ImportDownloadedFromAddress();
            }
            else
            {
                IsBusy = false;
                OnDownloadingFailedDuringImportFromAddress();
            }
        }

        private void ImportDownloadedFromAddress()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                CatchedImportDownloadedFromAddress();
            };
            worker.RunWorkerCompleted += delegate
            {
                IsBusy = false;
                NotifyAfterImportFromAddress();
            };
            readingFailedDuringImportFromAddress = false;
            worker.RunWorkerAsync();
        }

        private void CatchedImportDownloadedFromAddress()
        {
            try
            {
                exportAndImport.Import(data);
            }
            catch (Exception)
            {
                readingFailedDuringImportFromAddress = true;
            }
        }

        private void ActivateExportToCloud(BrowserIsNavigatingHint browserIsNavigatingHint)
        {
            var worker = new VerboseBackgroundWorker();
            var url = string.Empty;
            worker.DoWork += delegate
            {
                cloudProvider = cloudProviderFactory.Create();
                url = CatchedGetTokenAcquiringUrl(url);
            };
            worker.RunWorkerCompleted += delegate
            {
                if (url != string.Empty)
                {
                    OnNavigateInBrowser(url);
                    BrowserVisible = true;
                }
                IsBusy = false;
            };
            IsBusy = true;
            worker.RunWorkerAsync();
            this.browserIsNavigatingHint = browserIsNavigatingHint;
        }

        private void DeactivateExportToCloud()
        {
            var confirm = OnConfirmExportToCloudDeactivation();
            if (confirm)
            {
                var settings = factories.Settings;
                settings.CloudSecret = string.Empty;
                settings.CloudToken = string.Empty;
                settings.CloudExportDue = DateTime.MinValue;
            }
        }

        private void ShowImportFromCloudItems()
        {
            List<string> imports = null;
            var worker = new VerboseBackgroundWorker();
            worker.DoWork += delegate
            {
                imports = CatchedListImports(imports);
            };
            worker.RunWorkerCompleted += delegate
            {
                IsBusy = false;
                if (imports != null)
                {
                    ImportFromCloudItems = imports;
                    OnPropertyChanged("ImportFromCloudItems");
                    ImportFromCloudVisible = true;
                }
            };
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        private void StoreAcquiredToken()
        {
            var token = cloudProvider.GetAcquiredToken();
            var settings = factories.Settings;
            settings.CloudSecret = token.Secret;
            settings.CloudToken = token.Token;
        }

        private bool IsThisUrlTheTokenAcquiringCallbackUrl(string url)
        {
            return url != null
                && url.ToUpper().StartsWith(TOKEN_ACQUIRING_CALLBACK_URL.ToUpper());
        }

        private void CheckCloudProvider()
        {
            if (cloudProvider == null)
                throw new InvalidOperationException("ExportToCloud or ImportFromCloud should be invoked first.");
        }

        private void CheckIsExportToCloudActive()
        {
            if (!IsExportToCloudActive)
                throw new InvalidOperationException("ExportToCloud or ImportFromCloud should be invoked first.");
        }

        private void CatchedImportFromCloud()
        {
            try
            {
                cloud.Import(ImportFromCloudSelectedItem);
                importFromCloudWentOkay = true;
            }
            catch (Exception)
            {
                OnCloudError();
                importFromCloudWentOkay = false;
            }
        }

        private void CatchedBrowserIsNavigatingDoWork()
        {
            try
            {
                BrowserIsNavigatingDoWork();
                browserIsNavigatingDoWorkWentOkay = true;
            }
            catch (Exception)
            {
                OnCloudError();
                browserIsNavigatingDoWorkWentOkay = false;
            }
        }

        private void CatchedExportToCloudNow()
        {
            try
            {
                cloud.MakeItExport();
                cloud.Export();
                exportToCloudNowWentOkay = true;
            }
            catch (Exception)
            {
                OnCloudError();
                exportToCloudNowWentOkay = false;
            }
        }

        private string CatchedGetTokenAcquiringUrl(string url)
        {
            try
            {
                url = cloudProvider.GetTokenAcquiringUrl(TOKEN_ACQUIRING_CALLBACK_URL);
            }
            catch (Exception)
            {
                OnCloudError();
            }
            return url;
        }

        private List<string> CatchedListImports(List<string> imports)
        {
            try
            {
                imports = cloud.ListImports();
            }
            catch (Exception)
            {
                OnCloudError();
            }
            return imports;
        }

        private void BrowserIsNavigatingDoWork()
        {
            OnNavigateInBrowser(TOKEN_ACQUIRING_NAVIGATE_AWAY_URL);
            StoreAcquiredToken();
            if (browserIsNavigatingHint == BrowserIsNavigatingHint.Export)
                cloud.Export();
        }

        private void NotifyAfterImportFromAddress()
        {
            if (readingFailedDuringImportFromAddress)
            {
                OnReadingFailedDuringImportFromAddress();
            }
            else
            {
                OnImportFromAddressSuccesful();
            }
        }

        protected void OnExportToEmailSuccessful()
        {
            messageDialog.Show(Translations.ExportCompletedSuccessfully);
        }

        protected void OnImportFromAddressSuccesful()
        {
            messageDialog.Show(Translations.ImportCompletedSuccessfully);
        }

        protected void OnSendingFailedDuringExportToEmail()
        {
            messageDialog.Show(Translations.AnErrorOccurredWhileSendingTheExportedData);
        }

        protected void OnDownloadingFailedDuringImportFromAddress()
        {
            messageDialog.Show(Translations.AnErrorOccurredWhileRetrievingTheImportedData);
        }

        protected void OnReadingFailedDuringImportFromAddress()
        {
            messageDialog.Show(Translations.AnErrorOccurredDuringImport);
        }

        protected void OnNavigateInBrowser(string url)
        {
            if (NavigateInBrowser != null)
            {
                NavigateInBrowser(this, url);
            }
        }

        protected bool OnConfirmExportToCloudDeactivation()
        {
            return messageDialog.Confirm(cloudMessages.ConfirmExportToCloudDeactivation, string.Empty);
        }

        protected void OnExportToCloudActivationSuccessful()
        {
            messageDialog.Show(cloudMessages.ExportToCloudActivationSuccessful);
        }

        protected void OnExportToCloudSuccessful()
        {
            messageDialog.Show(cloudMessages.ExportToCloudSuccessful);
        }

        protected void OnImportFromCloudSuccessful()
        {
            messageDialog.Show(cloudMessages.ImportFromCloudSuccessful);
        }

        protected void OnCloudError()
        {
            messageDialog.Show(cloudMessages.CloudError);
        }

        private void NotifyIsExportToCloudActive()
        {
            OnPropertyChanged("IsExportToCloudActive");
        }

        private enum BrowserIsNavigatingHint { Unknown, Import, Export }
    }
}
