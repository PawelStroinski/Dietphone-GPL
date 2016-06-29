using System;
using System.Collections;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Util;

namespace Dietphone.Controls
{
    public sealed class MultiplePickerEditText : ListPickerEditText
    {
        public event EventHandler SelectedItemsChanged;
        public string DoneText { get; set; }
        public string CancelText { get; set; }
        private IList selectedItems, tempSelectedItems;
        private bool keepTempSelectedItems;

        public MultiplePickerEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public IList SelectedItems
        {
            get
            {
                return selectedItems;
            }
            set
            {
                selectedItems = value;
                SetText();
            }
        }

        protected override void InitializePositiveAndNegativeButtons(AlertDialog.Builder builder)
        {
            builder.SetPositiveButton(DoneText, delegate { Done(); });
            builder.SetNegativeButton(CancelText, delegate { });
        }

        protected override void InitializeDialogItems(AlertDialog.Builder builder, string[] items)
        {
            if (!keepTempSelectedItems)
                tempSelectedItems = CopyGenericList(SelectedItems);
            var checkedItems = ItemsSource.Cast<object>().Select(item => tempSelectedItems.Contains(item)).ToArray();
            builder.SetMultiChoiceItems(items, checkedItems, Dialog_Click);
        }

        protected override string GetText()
        {
            return string.Join(", ", SelectedItems.Cast<object>().Select(item => item.ToString()));
        }

        protected override void Edited()
        {
            RemoveDeletedFromTempSelectedItems();
            keepTempSelectedItems = true;
            try
            {
                ShowDialog();
            }
            finally
            {
                keepTempSelectedItems = false;
            }
        }

        private void Done()
        {
            if (!tempSelectedItems.Cast<object>().SequenceEqual(SelectedItems.Cast<object>()))
            {
                SelectedItems = tempSelectedItems;
                OnSelectedItemsChanged(EventArgs.Empty);
            }
        }

        private IList CopyGenericList(IList source)
        {
            var type = source.GetType();
            var typeArguments = type.GetGenericArguments();
            var enumerable = typeof(Enumerable);
            var toList = enumerable.GetMethod("ToList");
            var toListGeneric = toList.MakeGenericMethod(typeArguments);
            return (IList)toListGeneric.Invoke(enumerable, new[] { source });
        }

        private void Dialog_Click(object sender, DialogMultiChoiceClickEventArgs e)
        {
            var item = ItemsSource.Cast<object>().ElementAtOrDefault(e.Which);
            var contains = tempSelectedItems.Contains(item);
            if (item == null || e.IsChecked == contains)
                return;
            if (e.IsChecked)
                tempSelectedItems.Add(item);
            else
                tempSelectedItems.Remove(item);
        }

        private void RemoveDeletedFromTempSelectedItems()
        {
            for (var i = tempSelectedItems.Count - 1; i >= 0; i--)
                if (!ItemsSource.Contains(tempSelectedItems[i]))
                    tempSelectedItems.RemoveAt(i);
        }

        private void OnSelectedItemsChanged(EventArgs e)
        {
            if (SelectedItemsChanged != null)
            {
                SelectedItemsChanged(this, e);
            }
        }
    }
}