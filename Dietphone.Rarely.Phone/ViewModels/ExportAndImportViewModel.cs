using System;
using Dietphone.Models;
using System.ComponentModel;
using Dietphone.Tools;
using System.Net;
using System.Text;
using System.Windows;

namespace Dietphone.ViewModels
{
    public class ExportAndImportViewModel : ViewModelBase
    {
        public string Email { private get; set; }
        public string Url { private get; set; }
        public event EventHandler ExportAndSendSuccessful;
        public event EventHandler DownloadAndImportSuccessful;
        public event EventHandler SendingFailedDuringExport;
        public event EventHandler DownloadingFailedDuringImport;
        public event EventHandler ReadingFailedDuringImport;
        public event EventHandler<string> NavigateInBrowser;
        public event EventHandler<ConfirmEventArgs> ConfirmExportToCloudDeactivation;
        public event EventHandler ExportToCloudActivationSuccessful;
        private string data;
        private bool isBusy;
        private bool browserVisible { get; set; }
        private bool readingFailedDuringImport;
        private CloudProvider cloudProvider;
        private readonly Factories factories;
        private readonly ExportAndImport exportAndImport;
        private readonly CloudProviderFactory cloudProviderFactory;
        private const string MAILEXPORT_URL = "http://www.bizmaster.pl/varia/dietphone/MailExport.aspx";
        private const string MAILEXPORT_SUCCESS_RESULT = "Success!";
        internal const string TOKEN_ACQUIRING_CALLBACK_URL = "http://localhost/HelloTestingSuccess";

        public ExportAndImportViewModel(Factories factories, CloudProviderFactory cloudProviderFactory)
        {
            this.factories = factories;
            exportAndImport = new ExportAndImportImpl(factories);
            this.cloudProviderFactory = cloudProviderFactory;
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
            var settings = factories.Settings;
            if (settings.CloudSecret == string.Empty && settings.CloudToken == string.Empty)
                ActivateExportToCloud();
            else
                DeactivateExportToCloud();
        }

        public void BrowserIsNavigating(string url)
        {
            if (!IsThisUrlTheTokenAcquiringCallbackUrl(url))
                return;
            CheckCloudProvider();
            var worker = new BackgroundWorker();
            worker.DoWork += delegate { StoreAcquiredToken(); };
            worker.RunWorkerCompleted += delegate
            {
                BrowserVisible = false;
                OnExportToCloudActivationSuccessful();
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

        private void ActivateExportToCloud()
        {
            var worker = new BackgroundWorker();
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
            }
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
                throw new InvalidOperationException("ExportToCloud should be invoked first.");
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
    }

    public class ConfirmEventArgs : EventArgs
    {
        public bool Confirm { get; set; }
    }
}
