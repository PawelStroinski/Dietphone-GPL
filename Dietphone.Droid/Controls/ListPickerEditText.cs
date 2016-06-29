using System;
using System.Collections;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Util;
using Android.Views.InputMethods;
using Android.Widget;
using Dietphone.Adapters;

namespace Dietphone.Controls
{
    public abstract class ListPickerEditText : EditText
    {
        private const string ADD = "+";
        private const string EDIT = "✎";
        private const string DELETE = "✘";
        public string Title { get; set; }
        public IList ItemsSource { get; set; }
        public event EventHandler AddClick;
        public event EventHandler EditClick;
        public event EventHandler DeleteClick;
        protected AlertDialog dialog;
        private string[] itemsBeforeEditing;

        public ListPickerEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Click += delegate { ShowDialog(); };
            Focusable = false;
            SetCursorVisible(false);
            SetSingleLine(true);
            InputType = InputTypes.ClassText | InputTypes.TextFlagNoSuggestions;
        }

        protected void ShowDialog()
        {
            var builder = new AlertDialog.Builder(Context)
                .SetTitle(Title)
                .SetNeutralButton(EDIT, delegate { });
            InitializePositiveAndNegativeButtons(builder);
            CheckItemsSource();
            InitializeDialogItems(builder, Items);
            dialog = builder.Create();
            dialog.Show();
            var neutralButton = dialog.GetButton((int)DialogButtonType.Neutral);
            neutralButton.SetOnClickListener(new ClickListener(ShowEditingDialog));
        }

        protected virtual void InitializePositiveAndNegativeButtons(AlertDialog.Builder builder)
        {
        }

        private void CheckItemsSource()
        {
            if (ItemsSource == null)
                throw new NullReferenceException("ItemsSource");
        }

        private string[] Items => ItemsSource.Cast<object>().Select(item => item.ToString()).ToArray();

        protected abstract void InitializeDialogItems(AlertDialog.Builder builder, string[] items);

        private void ShowEditingDialog()
        {
            itemsBeforeEditing = Items;
            var builder = new AlertDialog.Builder(Context)
                .SetPositiveButton(ADD, delegate { OnEdit(AddClick); })
                .SetNeutralButton(EDIT, delegate { OnEdit(EditClick); })
                .SetNegativeButton(DELETE, delegate { OnEdit(DeleteClick); });
            var dialog = builder.Create();
            dialog.Show();
        }

        private void OnEdit(EventHandler handler)
        {
            if (handler != null)
                handler(this, EventArgs.Empty);
            if (!itemsBeforeEditing.SequenceEqual(Items))
            {
                SetText();
                dialog.Dismiss();
                Edited();
            }
        }

        protected void SetText()
        {
            var text = GetText();
            if (Text != text)
                Text = text;
        }

        protected abstract string GetText();

        protected virtual void Edited()
        {
        }
    }
}