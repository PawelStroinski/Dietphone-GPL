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
        private object selectedItem;

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

        protected override void InitializeDialogItems(AlertDialog.Builder builder, string[] items)
        {
            var checkedItem = ItemsSource.IndexOf(SelectedItem);
            builder.SetSingleChoiceItems(items, checkedItem, Dialog_Click);
        }

        protected override string GetText()
        {
            return SelectedItem?.ToString() ?? string.Empty;
        }

        private void Dialog_Click(object sender, DialogClickEventArgs e)
        {
            var clickedItem = ItemsSource.Cast<object>().ElementAtOrDefault(e.Which);
            if (clickedItem != null)
            {
                SelectedItem = clickedItem;
                OnSelectedItemChanged(EventArgs.Empty);
                dialog.Dismiss();
            }
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