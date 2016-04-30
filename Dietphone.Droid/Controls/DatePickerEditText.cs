// Idea from: http://benjaminhysell.com/archive/2014/04/mvvmcross-xamarin-android-popup-datepicker-on-edittext-click/
using System;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;

namespace Dietphone.Controls
{
    public sealed class DatePickerEditText : EditText
    {
        public string Title { get; set; }
        public event EventHandler ValueChanged;
        private DateTime value;
        private string dateFormat;
        private DateTime dateSet;
        private bool cancelled;

        public DatePickerEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Click += DatePickerEditText_Click;
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

        public string DateFormat
        {
            get
            {
                return dateFormat;
            }
            set
            {
                dateFormat = value;
                SetText();
            }
        }

        private void DatePickerEditText_Click(object sender, EventArgs e)
        {
            var dialog = new DatePickerDialog(Context, Dialog_DateSet, year: Value.Year, monthOfYear: Value.Month - 1,
                dayOfMonth: Value.Day);
            dialog.CancelEvent += Dialog_CancelEvent;
            dialog.DismissEvent += Dialog_DismissEvent;
            if (!string.IsNullOrEmpty(Title))
                dialog.SetTitle(Title);
            dateSet = Value;
            cancelled = false;
            dialog.Show();
        }

        private void SetText()
        {
            var text = string.IsNullOrEmpty(DateFormat) ? Value.ToShortDateString() : value.ToString(DateFormat);
            if (Text != text)
                Text = text;
        }

        private void Dialog_DateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            dateSet = new DateTime(e.Date.Date.Ticks + Value.TimeOfDay.Ticks, Value.Kind);
        }

        private void Dialog_CancelEvent(object sender, EventArgs e)
        {
            cancelled = true;
        }

        private void Dialog_DismissEvent(object sender, EventArgs e)
        {
            if (cancelled)
                return;
            if (Value == dateSet)
                return;
            Value = dateSet;
            OnValueChanged(EventArgs.Empty);
        }

        private void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
        }
    }
}