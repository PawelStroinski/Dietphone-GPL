﻿using System;
using Dietphone.Models;
using System.ComponentModel;
using Dietphone.Tools;
using System.Net;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Dietphone.ViewModels
{
    public class ExportAndImportViewModel : ViewModelBase
    {
        public string Email { private get; set; }
        public string Url { private get; set; }
        public List<string> ImportFromCloudItems { get; private set; }
        public string ImportFromCloudSelectedItem { get; set; }
        public event EventHandler ExportToEmailSuccessful;
        public event EventHandler ImportFromAddressSuccessful;
        public event EventHandler SendingFailedDuringExportToEmail;
        public event EventHandler DownloadingFailedDuringImportFromAddress;
        public event EventHandler ReadingFailedDuringImportFromAddress;
        public event EventHandler<string> NavigateInBrowser;
        public event EventHandler<ConfirmEventArgs> ConfirmExportToCloudDeactivation;
        public event EventHandler ExportToCloudActivationSuccessful;
        public event EventHandler ExportToCloudSuccessful;
        public event EventHandler ImportFromCloudSuccessful;
        public event EventHandler CloudError;
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
            if (ExportToEmailSuccessful != null)
            {
                ExportToEmailSuccessful(this, EventArgs.Empty);
            }
        }

        protected void OnImportFromAddressSuccesful()
        {
            if (ImportFromAddressSuccessful != null)
            {
                ImportFromAddressSuccessful(this, EventArgs.Empty);
            }
        }

        protected void OnSendingFailedDuringExportToEmail()
        {
            if (SendingFailedDuringExportToEmail != null)
            {
                SendingFailedDuringExportToEmail(this, EventArgs.Empty);
            }
        }

        protected void OnDownloadingFailedDuringImportFromAddress()
        {
            if (DownloadingFailedDuringImportFromAddress != null)
            {
                DownloadingFailedDuringImportFromAddress(this, EventArgs.Empty);
            }
        }

        protected void OnReadingFailedDuringImportFromAddress()
        {
            if (ReadingFailedDuringImportFromAddress != null)
            {
                ReadingFailedDuringImportFromAddress(this, EventArgs.Empty);
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

        protected void OnExportToCloudSuccessful()
        {
            if (ExportToCloudSuccessful != null)
            {
                ExportToCloudSuccessful(this, EventArgs.Empty);
            }
        }

        protected void OnImportFromCloudSuccessful()
        {
            if (ImportFromCloudSuccessful != null)
            {
                ImportFromCloudSuccessful(this, EventArgs.Empty);
            }
        }

        protected void OnCloudError()
        {
            if (CloudError != null)
            {
                CloudError(this, EventArgs.Empty);
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