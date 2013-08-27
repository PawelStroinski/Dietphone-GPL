// Inspired by the article http://dotnet.dzone.com/articles/input-box-windows-phone-7
using System;
using System.Windows.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using System.Threading.Tasks;

namespace Dietphone.Tools
{
    public class XnaInputBox
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Text { get; set; }
        public event EventHandler<ConfirmedEventArgs> Confirmed;
        public event EventHandler Cancelled;
        private readonly UserControl sender;

        public XnaInputBox(UserControl sender)
        {
            this.sender = sender;
            Title = "";
            Description = "";
            Text = "";
        }

        public async void Show()
        {
            Text = await Task<string>.Factory.FromAsync(Guide.BeginShowKeyboardInput(PlayerIndex.One, Title, Description, Text, null, null), Guide.EndShowKeyboardInput);
            if (string.IsNullOrEmpty(Text))
                OnCancelled();
            else
                OnConfirmed();
        }

        protected void OnConfirmed()
        {
            if (Confirmed != null)
            {
                var args = new ConfirmedEventArgs();
                args.Text = Text;
                Confirmed(this, args);
            }
        }

        protected void OnCancelled()
        {
            if (Cancelled != null)
            {
                Cancelled(this, EventArgs.Empty);
            }
        }
    }

    public class ConfirmedEventArgs : EventArgs
    {
        public string Text { get; set; }
    }
}