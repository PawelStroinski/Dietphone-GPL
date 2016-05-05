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
        public event EventHandler MultiChoiceChanged;
        public event EventHandler AddClick;
        public event EventHandler EditClick;
        public event EventHandler DeleteClick;
        private object selectedItem, clickedItem;
        private IList multiChoice, tempMultiChoice;
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


        public IList MultiChoice
        {
            get
            {
                return multiChoice;
            }
            set
            {
                multiChoice = value;
                SetText();
            }
        }

        private void View_Click(object sender, EventArgs e)
        {
            var okText = Context.GetString(Android.Resource.String.Ok);
            var cancelText = Context.GetString(Android.Resource.String.Cancel);
            var builder = new AlertDialog.Builder(Context)
                .SetTitle(Title)
                .SetPositiveButton(okText, delegate { Confirm(); })
                .SetNegativeButton(cancelText, delegate { })
                .SetNeutralButton(EDIT, delegate { });
            if (ItemsSource == null)
                throw new NullReferenceException("ItemsSource");
            var items = ItemsSource.Cast<object>().Select(item => item.ToString()).ToArray();
            if (SingleChoice)
                InitializeSingleChoiceItems(builder, items);
            else
                InitializeMultiChoiceItems(builder, items);
            dialog = builder.Create();
            dialog.Show();
            var neutralButton = dialog.GetButton((int)DialogButtonType.Neutral);
            neutralButton.SetOnClickListener(new ClickListener(ShowEditingDialog));
        }

        private void SetText()
        {
            var text = GetText();
            if (Text != text)
                Text = text;
        }

        private void Confirm()
        {
            if (SingleChoice)
                ConfirmSingleChoice();
            else
                ConfirmMultiChoice();
        }

        private void InitializeSingleChoiceItems(AlertDialog.Builder builder, string[] items)
        {
            var selectedItem = ItemsSource.IndexOf(SelectedItem);
            builder.SetSingleChoiceItems(items, selectedItem, Dialog_ItemClick);
            clickedItem = null;
        }

        private void InitializeMultiChoiceItems(AlertDialog.Builder builder, string[] items)
        {
            tempMultiChoice = CopyGenericList(MultiChoice);
            var multiChoice = ItemsSource.Cast<object>().Select(item => MultiChoice.Contains(item)).ToArray();
            builder.SetMultiChoiceItems(items, multiChoice, Dialog_MultiChoiceClick);
        }

        private void ShowEditingDialog()
        {
            Confirm();
            var builder = new AlertDialog.Builder(Context)
                .SetPositiveButton(ADD, delegate { OnEdit(AddClick); })
                .SetNeutralButton(EDIT, delegate { OnEdit(EditClick); })
                .SetNegativeButton(DELETE, delegate { OnEdit(DeleteClick); });
            var dialog = builder.Create();
            dialog.Show();
        }

        private string GetText()
        {
            if (SingleChoice)
                return SelectedItem?.ToString() ?? string.Empty;
            else
                return string.Join(", ", MultiChoice.Cast<object>().Select(item => item.ToString()));
        }

        private void ConfirmSingleChoice()
        {
            if (clickedItem != null && clickedItem != SelectedItem)
            {
                SelectedItem = clickedItem;
                OnSelectedItemChanged(EventArgs.Empty);
            }
        }

        private void ConfirmMultiChoice()
        {
            if (!tempMultiChoice.Cast<object>().SequenceEqual(MultiChoice.Cast<object>()))
            {
                MultiChoice = tempMultiChoice;
                OnMultiChoiceChanged(EventArgs.Empty);
            }
        }

        private void Dialog_ItemClick(object sender, DialogClickEventArgs e)
        {
            clickedItem = ItemsSource.Cast<object>().ElementAtOrDefault(e.Which);
        }

        private void Dialog_MultiChoiceClick(object sender, DialogMultiChoiceClickEventArgs e)
        {
            var item = ItemsSource.Cast<object>().ElementAtOrDefault(e.Which);
            var contains = tempMultiChoice.Contains(item);
            if (item == null || e.IsChecked == contains)
                return;
            if (e.IsChecked)
                tempMultiChoice.Add(item);
            else
                tempMultiChoice.Remove(item);
        }

        public IList CopyGenericList(IList source)
        {
            var type = source.GetType();
            var typeArguments = type.GetGenericArguments();
            var enumerable = typeof(Enumerable);
            var toList = enumerable.GetMethod("ToList");
            var toListGeneric = toList.MakeGenericMethod(typeArguments);
            return (IList)toListGeneric.Invoke(enumerable, new[] { source });
        }

        private bool SingleChoice => MultiChoice == null;

        private void OnSelectedItemChanged(EventArgs e)
        {
            if (SelectedItemChanged != null)
            {
                SelectedItemChanged(this, e);
            }
        }

        private void OnMultiChoiceChanged(EventArgs e)
        {
            if (MultiChoiceChanged != null)
            {
                MultiChoiceChanged(this, e);
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