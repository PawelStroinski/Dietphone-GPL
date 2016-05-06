using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Util;

namespace Dietphone.Controls
{
    public sealed class SinglePickerEditText : ListPickerEditText
    {
        public event EventHandler SelectedItemChanged;
        private object selectedItem, clickedItem;

        public SinglePickerEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
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

        protected override void Confirm()
        {
            if (clickedItem != null && clickedItem != SelectedItem)
            {
                SelectedItem = clickedItem;
                OnSelectedItemChanged(EventArgs.Empty);
            }
        }

        protected override void InitializeDialogItems(AlertDialog.Builder builder, string[] items)
        {
            var checkedItem = ItemsSource.IndexOf(SelectedItem);
            builder.SetSingleChoiceItems(items, checkedItem, Dialog_Click);
            clickedItem = null;
        }

        protected override string GetText()
        {
            return SelectedItem?.ToString() ?? string.Empty;
        }

        private void Dialog_Click(object sender, DialogClickEventArgs e)
        {
            clickedItem = ItemsSource.Cast<object>().ElementAtOrDefault(e.Which);
        }

        private void OnSelectedItemChanged(EventArgs e)
        {
            if (SelectedItemChanged != null)
            {
                SelectedItemChanged(this, e);
            }
        }
    }
}