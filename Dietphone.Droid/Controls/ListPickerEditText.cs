using System;
using System.Collections;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;
using Dietphone.Adapters;

namespace Dietphone.Controls
{
    public sealed class ListPickerEditText : EditText
    {
        private const string ADD = "+";
        private const string EDIT = "✎";
        private const string DELETE = "✘";
        public string Title { get; set; }
        public IList ItemsSource { get; set; }
        public event EventHandler SelectedItemChanged;
        public event EventHandler AddClick;
        public event EventHandler EditClick;
        public event EventHandler DeleteClick;
        private object selectedItem;
        private object clickedItem;
        private AlertDialog dialog;

        public ListPickerEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Click += View_Click;
            Focusable = false;
            SetCursorVisible(false);
        }

        public object SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                SetText();
            }
        }

        private void View_Click(object sender, EventArgs e)
        {
            var okText = Context.GetString(Android.Resource.String.Ok);
            var cancelText = Context.GetString(Android.Resource.String.Cancel);
            var builder = new AlertDialog.Builder(Context)
                .SetTitle(Title)
                .SetPositiveButton(okText, delegate { SetSelectedItem(); })
                .SetNegativeButton(cancelText, delegate { })
                .SetNeutralButton(EDIT, delegate { });
            if (ItemsSource == null)
                throw new NullReferenceException("ItemsSource");
            var items = ItemsSource.Cast<object>().Select(item => item.ToString()).ToArray();
            var selectedItem = ItemsSource.IndexOf(SelectedItem);
            builder.SetSingleChoiceItems(items, selectedItem, Dialog_ItemClick);
            clickedItem = null;
            dialog = builder.Create();
            dialog.Show();
            var neutralButton = dialog.GetButton((int)DialogButtonType.Neutral);
            neutralButton.SetOnClickListener(new ClickListener(ShowEditingDialog));
        }

        private void SetText()
        {
            var text = SelectedItem?.ToString() ?? string.Empty;
            if (Text != text)
                Text = text;
        }

        private void SetSelectedItem()
        {
            if (clickedItem != null && clickedItem != selectedItem)
            {
                SelectedItem = clickedItem;
                OnSelectedItemChanged(EventArgs.Empty);
            }
        }

        private void Dialog_ItemClick(object sender, DialogClickEventArgs e)
        {
            clickedItem = ItemsSource.Cast<object>().ElementAtOrDefault(e.Which);
        }

        private void ShowEditingDialog()
        {
            SetSelectedItem();
            var builder = new AlertDialog.Builder(Context)
                .SetPositiveButton(ADD, delegate { OnEdit(AddClick); })
                .SetNeutralButton(EDIT, delegate { OnEdit(EditClick); })
                .SetNegativeButton(DELETE, delegate { OnEdit(DeleteClick); });
            var dialog = builder.Create();
            dialog.Show();
        }

        private void OnSelectedItemChanged(EventArgs e)
        {
            if (SelectedItemChanged != null)
            {
                SelectedItemChanged(this, e);
            }
        }

        private void OnEdit(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
                SetText();
            }
            dialog.Dismiss();
        }
    }
}