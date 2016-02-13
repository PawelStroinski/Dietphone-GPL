// The GetSystemTrayHeight method is from http://stackoverflow.com/a/24059163
using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Dietphone.ViewModels;
using System.Windows.Media;
using System.Windows.Input;
using Dietphone.Tools;
using System.ComponentModel;

namespace Dietphone.Views
{
    public partial class ExportAndImport : StateProviderPage
    {
        public new ExportAndImportViewModel ViewModel { get { return (ExportAndImportViewModel)base.ViewModel; } }
        private bool exportMode;

        public ExportAndImport()
        {
            InitializeComponent();
        }

        protected override void OnInitializePage()
        {
            ViewModel.ExportToEmailSuccessful += ViewModel_ExportToEmailSuccessful;
            ViewModel.ImportFromAddressSuccessful += ViewModel_ImportFromAddressSuccessful;
            ViewModel.SendingFailedDuringExportToEmail += ViewModel_SendingFailedDuringExportToEmail;
            ViewModel.DownloadingFailedDuringImportFromAddress += ViewModel_DownloadingFailedDuringImportFromAddress;
            ViewModel.ReadingFailedDuringImportFromAddress += ViewModel_ReadingFailedDuringImportFromAddress;
            ViewModel.NavigateInBrowser += ViewModel_NavigateInBrowser;
            ViewModel.ConfirmExportToCloudDeactivation += ViewModel_ConfirmExportToCloudDeactivation;
            ViewModel.ExportToCloudActivationSuccessful += ViewModel_ExportToCloudActivationSuccessful;
            ViewModel.ExportToCloudSuccessful += ViewModel_ExportToCloudSuccessful;
            ViewModel.ImportFromCloudSuccessful += ViewModel_ImportFromCloudSuccessful;
            ViewModel.CloudError += ViewModel_CloudError;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            SetWindowBackground();
            SetWindowSize();
            TranslateButtons();
            TranslateApplicationBar();
            SetMenuItemEnabledDependingOnIsExportToCloudActive();
        }

        private void ViewModel_ExportToEmailSuccessful(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.ExportCompletedSuccessfully);
            });
        }

        private void ViewModel_ImportFromAddressSuccessful(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.ImportCompletedSuccessfully);
            });
        }

        private void ViewModel_SendingFailedDuringExportToEmail(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.AnErrorOccurredWhileSendingTheExportedData);
            });
        }

        private void ViewModel_DownloadingFailedDuringImportFromAddress(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.AnErrorOccurredWhileRetrievingTheImportedData);
            });
        }

        private void ViewModel_ReadingFailedDuringImportFromAddress(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.AnErrorOccurredDuringImport);
            });
        }

        private void ViewModel_NavigateInBrowser(object sender, string e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                Browser.Navigate(new Uri(e));
            });
        }

        private void ViewModel_ConfirmExportToCloudDeactivation(object sender, ConfirmEventArgs e)
        {
            e.Confirm = MessageBox.Show(Translations.ExportToDropboxIsActiveDoYouWantToTurnItOff, string.Empty,
                MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        private void ViewModel_ExportToCloudActivationSuccessful(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.ExportToDropboxActivationWasSuccessful);
            });
        }

        private void ViewModel_ExportToCloudSuccessful(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.ExportToDropboxWasSuccessful);
            });
        }

        private void ViewModel_ImportFromCloudSuccessful(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.ImportCompletedSuccessfully);
            });
        }

        private void ViewModel_CloudError(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.AnErrorOccurredDuringTheDropboxOperation);
            });
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsExportToCloudActive")
                SetMenuItemEnabledDependingOnIsExportToCloudActive();
        }

        private void ExportToCloud_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ExportToCloud();
        }

        private void ImportFromCloud_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ImportFromCloud();
        }

        private void ImportFromCloudWithSelection_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ImportFromCloudWithSelection();
        }

        private void ExportByEmail_Click(object sender, RoutedEventArgs e)
        {
            exportMode = true;
            Info.Text = Translations.SendToAnEMailAddress;
            Input.Text = string.Empty;
            Input.InputScope = InputScopeNameValue.EmailSmtpAddress.GetInputScope();
            EmailAndAddressWindow.IsOpen = true;
        }

        private void ImportFromAddress_Click(object sender, RoutedEventArgs e)
        {
            exportMode = false;
            Info.Text = Translations.DownloadFileFromAddress;
            Input.Text = "http://";
            Input.InputScope = InputScopeNameValue.Url.GetInputScope();
            EmailAndAddressWindow.IsOpen = true;
        }

        private void WindowAnimation_Ended(object sender, EventArgs e)
        {
            if (EmailAndAddressWindow.IsOpen)
            {
                Input.Focus();
                if (!exportMode)
                {
                    var text = Input.Text;
                    Input.Select(text.Length, 0);
                }
            }
        }

        private void EmailAndAddressDone_Click(object sender, RoutedEventArgs e)
        {
            if (exportMode)
            {
                ViewModel.Email = Input.Text;
                ViewModel.ExportToEmail();
            }
            else
            {
                ViewModel.Url = Input.Text;
                ViewModel.ImportFromAddress();
            }
            EmailAndAddressWindow.IsOpen = false;
        }

        private void ExportToCloudNow_Click(object sender, EventArgs e)
        {
            ViewModel.ExportToCloudNow();
        }

        private void Browser_Navigating(object sender, NavigatingEventArgs e)
        {
            ViewModel.BrowserIsNavigating(e.Uri.ToString());
        }

        private void SetWindowBackground()
        {
            Color color;
            if (this.IsDarkTheme())
            {
                color = Color.FromArgb(0xCC, 0, 0, 0);
            }
            else
            {
                color = Color.FromArgb(0xCC, 255, 255, 255);
            }
            var brush = new SolidColorBrush(color);
            EmailAndAddressWindow.Background = brush;
            ImportFromCloudWindow.Background = brush;
            BrowserWindow.Background = brush;
        }

        private void SetWindowSize()
        {
            Loaded += (sender, args) =>
            {
                var renderSize = Application.Current.RootVisual.RenderSize;
                var windowSize = new Size(renderSize.Width, double.NaN);
                var browserWindowSize = new Size(renderSize.Width, renderSize.Height - GetSystemTrayHeight());
                EmailAndAddressWindow.WindowSize = windowSize;
                ImportFromCloudWindow.WindowSize = windowSize;
                BrowserWindow.WindowSize = browserWindowSize;
                Browser.Height = browserWindowSize.Height;
            };
        }

        private void TranslateButtons()
        {
            ExportToCloud.Line1 = Translations.ExportToDropbox;
            ExportToCloud.Line2 = Translations.AutomaticallySavesDataToDropboxOnceAWeek;
            ImportFromCloud.Line1 = Translations.ImportFromDropbox;
            ImportFromCloud.Line2 = Translations.RetrievesAndAddsToApplicationDataPreviouslySavedToDropbox;
            ExportByEmail.Line1 = Translations.ExportByEmail;
            ExportByEmail.Line2 = Translations.AllowsSendingDataAttachedToAnEMail;
            ImportFromAddress.Line1 = Translations.ImportFromAddress;
            ImportFromAddress.Line2 = Translations.AllowsToRetrieveDataFromAFileInXmlFormat;
        }

        private void TranslateApplicationBar()
        {
            this.GetMenuItem(0).Text = Translations.ExportToDropboxNow;
        }

        private void SetMenuItemEnabledDependingOnIsExportToCloudActive()
        {
            this.GetMenuItem(0).IsEnabled = ViewModel.IsExportToCloudActive;
        }

        private double GetSystemTrayHeight()
        {
            GeneralTransform transform = LayoutRoot.TransformToVisual(Application.Current.RootVisual as UIElement);
            Point offset = transform.Transform(new Point(0, 0));
            return offset.Y;
        }
    }
}