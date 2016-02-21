// The Input method inspired by the article http://dotnet.dzone.com/articles/input-box-windows-phone-7
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace Dietphone.Tools
{
    public class MessageDialogImpl : MessageDialog
    {
        public void Show(string text)
        {
            if (Dispatcher.CheckAccess())
            {
                ShowDo(text);
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ShowDo(text);
                });
            }
        }

        public void Show(string text, string caption)
        {
            if (Dispatcher.CheckAccess())
            {
                ShowDo(text, caption);
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ShowDo(text, caption);
                });
            }
        }

        public bool Confirm(string text, string caption)
        {
            if (Dispatcher.CheckAccess())
            {
                return ConfirmDo(text, caption);
            }
            else
            {
                return DispatchedConfirm(text, caption);
            }
        }

        public string Input(string text, string caption)
        {
            return Input(text, caption, value: string.Empty);
        }

        public string Input(string text, string caption, string value)
        {
            var task = Task<string>.Factory.FromAsync(
                Guide.BeginShowKeyboardInput(PlayerIndex.One, caption, text, value, null, null),
                Guide.EndShowKeyboardInput);
            return task.Result;
        }

        private Dispatcher Dispatcher
        {
            get
            {
                return Deployment.Current.Dispatcher;
            }
        }

        private bool DispatchedConfirm(string text, string caption)
        {
            var signal = new ManualResetEvent(false);
            var confirmed = false;
            Dispatcher.BeginInvoke(() =>
            {
                confirmed = ConfirmDo(text, caption);
                signal.Set();
            });
            signal.WaitOne();
            return confirmed;
        }

        private void ShowDo(string text)
        {
            MessageBox.Show(text);
        }

        private void ShowDo(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButton.OK);
        }

        private bool ConfirmDo(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButton.OKCancel)
                == MessageBoxResult.OK;
        }
    }
}
