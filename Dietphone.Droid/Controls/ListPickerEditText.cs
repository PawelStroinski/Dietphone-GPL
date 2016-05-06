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
        private AlertDialog dialog;

        public ListPickerEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Click += View_Click;
            Focusable = false;
            SetCursorVisible(false);
            SetSingleLine(true);
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
            InitializeDialogItems(builder, items);
            dialog = builder.Create();
            dialog.Show();
            var neutralButton = dialog.GetButton((int)DialogButtonType.Neutral);
            neutralButton.SetOnClickListener(new ClickListener(ShowEditingDialog));
        }

        protected abstract void Confirm();

        protected abstract void InitializeDialogItems(AlertDialog.Builder builder, string[] items);

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

        private void OnEdit(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
                SetText();
            }
            dialog.Dismiss();
        }

        protected void SetText()
        {
            var text = GetText();
            if (Text != text)
                Text = text;
        }

        protected abstract string GetText();
    }
}