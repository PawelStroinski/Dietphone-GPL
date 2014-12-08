using System;
using Dietphone.Models;
using System.ComponentModel;
using Dietphone.Tools;
using System.Net;
using System.Text;
using System.Windows;
using System.Collections.Generic;

namespace Dietphone.ViewModels
{
    public class ExportAndImportViewModel : ViewModelBase
    {
        public string Email { private get; set; }
        public string Url { private get; set; }
        public List<string> ImportFromCloudItems { get; private set; }
        public string ImportFromCloudSelectedItem { get; set; }
        public event EventHandler ExportAndSendSuccessful;
        public event EventHandler DownloadAndImportSuccessful;
        public event EventHandler SendingFailedDuringExport;
        public event EventHandler DownloadingFailedDuringImport;
        public event EventHandler ReadingFailedDuringImport;
        public event EventHandler<string> NavigateInBrowser;
        public event EventHandler<ConfirmEventArgs> ConfirmExportToCloudDeactivation;
        public event EventHandler ExportToCloudActivationSuccessful;
        public event EventHandler ImportFromCloudSuccessful;
        private string data;
        private bool isBusy;
        private bool browserVisible;
        private bool importFromCloudVisible;
        private bool readingFailedDuringImport;
        private CloudProvider cloudProvider;
        private BrowserIsNavigatingHint browserIsNavigatingHint;
        private readonly Factories factories;
        private readonly ExportAndImport exportAndImport;
        private readonly CloudProviderFactory cloudProviderFactory;
        private readonly Vibration vibration;
        private readonly Cloud cloud;
        private const string MAILEXPORT_URL = "http://www.bizmaster.pl/varia/dietphone/MailExport.aspx";
        private const string MAILEXPORT_SUCCESS_RESULT = "Success!";
        internal const string TOKEN_ACQUIRING_CALLBACK_URL = "http://localhost/HelloTestingSuccess";
        internal const string TOKEN_ACQUIRING_NAVIGATE_AWAY_URL = "about:blank";

        public ExportAndImportViewModel(Factories factories, CloudProviderFactory cloudProviderFactory,
            Vibration vibration, Cloud cloud)
        {
            this.factories = factories;
            exportAndImport = new ExportAndImportImpl(factories);
            this.cloudProviderFactory = cloudProviderFactory;
            this.vibration = vibration;
            this.cloud = cloud;
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
                OnPropertyChanged("IsBusyAsVisibility");
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

        public Visibility IsBusyAsVisibility
        {
            get
            {
                return IsBusy.ToVisibility();
            }
        }

        public void ExportAndSend()
        {
            if (IsBusy)
            {
                return;
            }
            if (!Email.IsValidEmail())
            {
                OnSendingFailedDuringExport();
                return;
            }
            var worker = new VerboseBackgroundWorker();
            worker.DoWork += delegate
            {
                data = exportAndImport.Export();
            };
            worker.RunWorkerCompleted += delegate
            {
                Send();
            };
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        public void DownloadAndImport()
        {
            if (IsBusy)
            {
                return;
            }
            Download();
        }

        public void ExportToCloud()
        {
            vibration.VibrateOnButtonPress();
            if (IsExportToCloudActive)
                DeactivateExportToCloud();
            else
                ActivateExportToCloud(BrowserIsNavigatingHint.Export);
            NotifyIsExportToCloudActive();
        }

        public void ImportFromCloud()
        {
            if (IsExportToCloudActive)
                ShowImportFromCloudItems();
            else
                ActivateExportToCloud(BrowserIsNavigatingHint.Import);
        }

        public void ImportFromCloudWithSelection()
        {
            var worker = new VerboseBackgroundWorker();
            var hasSelection = !string.IsNullOrEmpty(ImportFromCloudSelectedItem);
            worker.DoWork += delegate
            {
                if (hasSelection)
                    cloud.Import(ImportFromCloudSelectedItem);
            };
            worker.RunWorkerCompleted += delegate
            {
                if (hasSelection)
                    OnImportFromCloudSuccessful();
                IsBusy = false;
            };
            ImportFromCloudVisible = false;
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        public void BrowserIsNavigating(string url)
        {
            if (!IsThisUrlTheTokenAcquiringCallbackUrl(url))
                return;
            CheckCloudProvider();
            var worker = new VerboseBackgroundWorker();
            worker.DoWork += delegate
            {
                OnNavigateInBrowser(TOKEN_ACQUIRING_NAVIGATE_AWAY_URL);
                StoreAcquiredToken();
                if (browserIsNavigatingHint == BrowserIsNavigatingHint.Export)
                    cloud.Export();
            };
            worker.RunWorkerCompleted += delegate
            {
                BrowserVisible = false;
                if (browserIsNavigatingHint == BrowserIsNavigatingHint.Export)
                    OnExportToCloudActivationSuccessful();
                if (browserIsNavigatingHint == BrowserIsNavigatingHint.Import)
                    ShowImportFromCloudItems();
                NotifyIsExportToCloudActive();
            };
            worker.RunWorkerAsync();
        }

        private void Send()
        {
            var sender = new PostSender(MAILEXPORT_URL);
            sender.Inputs["address"] = Email;
            sender.Inputs["data"] = data;
            sender.Completed += Send_Completed;
            sender.SendAsync();
        }

        private void Download()
        {
            if (!Url.IsValidUri())
            {
                OnDownloadingFailedDuringImport();
                return;
            }
            IsBusy = true;
            var web = new WebClient();
            web.Encoding = Encoding.Unicode;
            web.DownloadStringCompleted += Download_Completed;
            web.DownloadStringAsync(new Uri(Url));
        }

        private void Send_Completed(object sender, UploadStringCompletedEventArgs e)
        {
            IsBusy = false;
            if (e.IsGeneralSuccess() && e.Result == MAILEXPORT_SUCCESS_RESULT)
            {
                OnExportAndSendSuccessful();
            }
            else
            {
                OnSendingFailedDuringExport();
            }
        }

        private void Download_Completed(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.IsGeneralSuccess())
            {
                data = e.Result;
                Import();
            }
            else
            {
                IsBusy = false;
                OnDownloadingFailedDuringImport();
            }
        }

        private void Import()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                CatchedImport();
            };
            worker.RunWorkerCompleted += delegate
            {
                IsBusy = false;
                NotifyAfterImport();
            };
            readingFailedDuringImport = false;
            worker.RunWorkerAsync();
        }

        private void CatchedImport()
        {
            try
            {
                exportAndImport.Import(data);
            }
            catch (Exception)
            {
                readingFailedDuringImport = true;
            }
        }

        private void ActivateExportToCloud(BrowserIsNavigatingHint browserIsNavigatingHint)
        {
            var worker = new VerboseBackgroundWorker();
            var url = string.Empty;
            worker.DoWork += delegate
            {
                cloudProvider = cloudProviderFactory.Create();
                url = cloudProvider.GetTokenAcquiringUrl(TOKEN_ACQUIRING_CALLBACK_URL);
            };
            worker.RunWorkerCompleted += delegate
            {
                OnNavigateInBrowser(url);
                BrowserVisible = true;
                IsBusy = false;
            };
            IsBusy = true;
            worker.RunWorkerAsync();
            this.browserIsNavigatingHint = browserIsNavigatingHint;
        }

        private void DeactivateExportToCloud()
        {
            var eventArgs = new ConfirmEventArgs();
            OnConfirmExportToCloudDeactivation(eventArgs);
            if (eventArgs.Confirm)
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
                imports = cloud.ListImports();
            };
            worker.RunWorkerCompleted += delegate
            {
                IsBusy = false;
                ImportFromCloudItems = imports;
                ImportFromCloudVisible = true;
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

        private void NotifyAfterImport()
        {
            if (readingFailedDuringImport)
            {
                OnReadingFailedDuringImport();
            }
            else
            {
                OnDownloadAndImportSuccesful();
            }
        }

        protected void OnExportAndSendSuccessful()
        {
            if (ExportAndSendSuccessful != null)
            {
                ExportAndSendSuccessful(this, EventArgs.Empty);
            }
        }

        protected void OnDownloadAndImportSuccesful()
        {
            if (DownloadAndImportSuccessful != null)
            {
                DownloadAndImportSuccessful(this, EventArgs.Empty);
            }
        }

        protected void OnSendingFailedDuringExport()
        {
            if (SendingFailedDuringExport != null)
            {
                SendingFailedDuringExport(this, EventArgs.Empty);
            }
        }

        protected void OnDownloadingFailedDuringImport()
        {
            if (DownloadingFailedDuringImport != null)
            {
                DownloadingFailedDuringImport(this, EventArgs.Empty);
            }
        }

        protected void OnReadingFailedDuringImport()
        {
            if (ReadingFailedDuringImport != null)
            {
                ReadingFailedDuringImport(this, EventArgs.Empty);
            }
        }

        protected void OnNavigateInBrowser(string url)
        {
            if (NavigateInBrowser != null)
            {
                NavigateInBrowser(this, url);
            }
        }

        protected void OnConfirmExportToCloudDeactivation(ConfirmEventArgs e)
        {
            if (ConfirmExportToCloudDeactivation != null)
            {
                ConfirmExportToCloudDeactivation(this, e);
            }
        }

        protected void OnExportToCloudActivationSuccessful()
        {
            if (ExportToCloudActivationSuccessful != null)
            {
                ExportToCloudActivationSuccessful(this, EventArgs.Empty);
            }
        }

        protected void OnImportFromCloudSuccessful()
        {
            if (ImportFromCloudSuccessful != null)
            {
                ImportFromCloudSuccessful(this, EventArgs.Empty);
            }
        }

        private void NotifyIsExportToCloudActive()
        {
            OnPropertyChanged("IsExportToCloudActive");
        }

        private enum BrowserIsNavigatingHint { Unknown, Import, Export }
    }

    public class ConfirmEventArgs : EventArgs
    {
        public bool Confirm { get; set; }
    }
}
