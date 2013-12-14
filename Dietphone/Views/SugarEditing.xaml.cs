using System;
using System.Windows;
using System.Windows.Controls;
using Dietphone.ViewModels;
using Dietphone.Tools;
using System.ComponentModel;

namespace Dietphone.Views
{
    public partial class SugarEditing : UserControl
    {
        public SugarEditingViewModel ViewModel { get; private set; }
        private bool controlledClosing;

        public SugarEditing()
        {
            InitializeComponent();
            Delete = Picker.ApplicationBarInfo.Buttons[2];
            ViewModel = new SugarEditingViewModel();
            ViewModel.NeedToShow += delegate
            {
                DataContext = ViewModel.Subject;
                Delete.IsEnabled = ViewModel.CanDelete;
                Picker.IsPopupOpen = true;
                controlledClosing = false;
            };
            TranslateApplicationBar();
        }

        private void Picker_PopupOpened(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                Value.Focus();
                if (Value.Text.Length > 0)
                {
                    Value.Select(Value.Text.Length, 0);
                }
            });
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            ViewModel.Confirm();
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            ViewModel.Cancel();
            Close();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            ViewModel.Delete();
            Close();
        }

        private void Close()
        {
            controlledClosing = true;
            if (Value.IsFocused())
            {
                Focus();
                Dispatcher.BeginInvoke(() =>
                {
                    Picker.IsPopupOpen = false;
                });
            }
            else
            {
                Picker.IsPopupOpen = false;
            }
        }

        private void Picker_PopupClosing(object sender, CancelEventArgs e)
        {
            if (!controlledClosing)
            {
                ViewModel.Cancel();
            }
        }

        private void TranslateApplicationBar()
        {
            var applicationBar = Picker.ApplicationBarInfo;
            var ok = applicationBar.Buttons[0];
            var cancel = applicationBar.Buttons[1];
            ok.Text = Translations.Ok;
            cancel.Text = Translations.Cancel;
            Delete.Text = Translations.Delete;
        }
    }
}
