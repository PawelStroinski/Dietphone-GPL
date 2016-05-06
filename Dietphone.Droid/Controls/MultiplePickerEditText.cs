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
        private IList selectedItems, tempSelectedItems;

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

        protected override void Confirm()
        {
            if (!tempSelectedItems.Cast<object>().SequenceEqual(SelectedItems.Cast<object>()))
            {
                SelectedItems = tempSelectedItems;
                OnSelectedItemsChanged(EventArgs.Empty);
            }
        }

        protected override void InitializeDialogItems(AlertDialog.Builder builder, string[] items)
        {
            tempSelectedItems = CopyGenericList(SelectedItems);
            var checkedItems = ItemsSource.Cast<object>().Select(item => SelectedItems.Contains(item)).ToArray();
            builder.SetMultiChoiceItems(items, checkedItems, Dialog_Click);
        }

        protected override string GetText()
        {
            return string.Join(", ", SelectedItems.Cast<object>().Select(item => item.ToString()));
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

        private void OnSelectedItemsChanged(EventArgs e)
        {
            if (SelectedItemsChanged != null)
            {
                SelectedItemsChanged(this, e);
            }
        }
    }
}