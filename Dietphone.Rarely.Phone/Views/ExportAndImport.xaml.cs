// The GetSystemTrayHeight method is from http://stackoverflow.com/a/24059163
using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Dietphone.ViewModels;
using System.Windows.Media;
using System.Windows.Input;
using Dietphone.Tools;
using Dietphone.Models;

namespace Dietphone.Views
{
    public partial class ExportAndImport : PhoneApplicationPage
    {
        public ExportAndImportViewModel ViewModel { get; private set; }
        private bool exportMode;

        public ExportAndImport()
        {
            InitializeComponent();
            ViewModel = new ExportAndImportViewModel(MyApp.Factories,
                new DropboxProviderFactory(MyApp.Factories),
                new VibrationImpl(),
                new CloudImpl(new DropboxProviderFactory(MyApp.Factories),
                    MyApp.Factories,
                    new ExportAndImportImpl(MyApp.Factories)));
            ViewModel.ExportAndSendSuccessful += ViewModel_ExportAndSendSuccessful;
            ViewModel.DownloadAndImportSuccessful += ViewModel_DownloadAndImportSuccessful;
            ViewModel.SendingFailedDuringExport += ViewModel_SendingFailedDuringExport;
            ViewModel.DownloadingFailedDuringImport += ViewModel_DownloadingFailedDuringImport;
            ViewModel.ReadingFailedDuringImport += ViewModel_ReadingFailedDuringImport;
            ViewModel.NavigateInBrowser += ViewModel_NavigateInBrowser;
            ViewModel.ConfirmExportToCloudDeactivation += ViewModel_ConfirmExportToCloudDeactivation;
            ViewModel.ExportToCloudActivationSuccessful += ViewModel_ExportToCloudActivationSuccessful;
            DataContext = ViewModel;
            SetWindowBackground();
            SetWindowSize();
            TranslateButtons();
        }

        private void ViewModel_ExportAndSendSuccessful(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.ExportCompletedSuccessfully);
            });
        }

        private void ViewModel_DownloadAndImportSuccessful(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.ImportCompletedSuccessfully);
            });
        }

        private void ViewModel_SendingFailedDuringExport(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.AnErrorOccurredWhileSendingTheExportedData);
            });
        }

        private void ViewModel_DownloadingFailedDuringImport(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(Translations.AnErrorOccurredWhileRetrievingTheImportedData);
            });
        }

        private void ViewModel_ReadingFailedDuringImport(object sender, EventArgs e)
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

        private void ExportToCloud_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ExportToCloud();
        }

        private void ImportFromCloud_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ExportByEmail_Click(object sender, RoutedEventArgs e)
        {
            exportMode = true;
            Info.Text = Translations.SendToAnEMailAddress;
            Input.Text = string.Empty;
            Input.InputScope = InputScopeNameValue.EmailSmtpAddress.GetInputScope();
            Window.IsOpen = true;
        }

        private void ImportFromAddress_Click(object sender, RoutedEventArgs e)
        {
            exportMode = false;
            Info.Text = Translations.DownloadFileFromAddress;
            Input.Text = "http://";
            Input.InputScope = InputScopeNameValue.Url.GetInputScope();
            Window.IsOpen = true;
        }

        private void WindowAnimation_Ended(object sender, EventArgs e)
        {
            if (Window.IsOpen)
            {
                Input.Focus();
                if (!exportMode)
                {
                    var text = Input.Text;
                    Input.Select(text.Length, 0);
                }
            }
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            if (exportMode)
            {
                ViewModel.Email = Input.Text;
                ViewModel.ExportAndSend();
            }
            else
            {
                ViewModel.Url = Input.Text;
                ViewModel.DownloadAndImport();
            }
            Window.IsOpen = false;
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
            Window.Background = brush;
            BrowserWindow.Background = brush;
        }

        private void SetWindowSize()
        {
            Loaded += (sender, args) =>
            {
                var renderSize = Application.Current.RootVisual.RenderSize;
                var windowSize = new Size(renderSize.Width, double.NaN);
                var browserWindowSize = new Size(renderSize.Width, renderSize.Height - GetSystemTrayHeight());
                Window.WindowSize = windowSize;
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

        private double GetSystemTrayHeight()
        {
            GeneralTransform transform = LayoutRoot.TransformToVisual(Application.Current.RootVisual as UIElement);
            Point offset = transform.Transform(new Point(0, 0));
            return offset.Y;
        }
    }
}