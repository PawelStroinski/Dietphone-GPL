// Idea from: http://benjaminhysell.com/archive/2014/04/mvvmcross-xamarin-android-popup-datepicker-on-edittext-click/
using System;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;

namespace Dietphone.Controls
{
    public abstract class DateTimePickerEditText : EditText
    {
        public string Title { get; set; }
        public event EventHandler ValueChanged;
        protected DateTime dateTimeSet;
        private DateTime value;
        private bool cancelled;

        public DateTimePickerEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Click += View_Click;
            Focusable = false;
            SetCursorVisible(false);
        }

        public DateTime Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                SetText();
            }
        }

        private void View_Click(object sender, EventArgs e)
        {
            var dialog = CreateDialog();
            dialog.CancelEvent += Dialog_CancelEvent;
            dialog.DismissEvent += Dialog_DismissEvent;
            if (!string.IsNullOrEmpty(Title))
                dialog.SetTitle(Title);
            dateTimeSet = Value;
            cancelled = false;
            dialog.Show();
        }

        protected void SetText()
        {
            var text = GetText();
            if (Text != text)
                Text = text;
        }

        protected abstract AlertDialog CreateDialog();

        private void Dialog_CancelEvent(object sender, EventArgs e)
        {
            cancelled = true;
        }

        private void Dialog_DismissEvent(object sender, EventArgs e)
        {
            if (cancelled)
                return;
            if (Value == dateTimeSet)
                return;
            Value = dateTimeSet;
            OnValueChanged(EventArgs.Empty);
        }

        protected abstract string GetText();

        private void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
        }
    }
}