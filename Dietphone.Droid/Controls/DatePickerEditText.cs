// Idea from: http://benjaminhysell.com/archive/2014/04/mvvmcross-xamarin-android-popup-datepicker-on-edittext-click/
using System;
using Android.App;
using Android.Content;
using Android.Util;

namespace Dietphone.Controls
{
    public sealed class DatePickerEditText : DateTimePickerEditText
    {
        private string dateFormat;

        public DatePickerEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
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

        protected override AlertDialog CreateDialog()
        {
            return new DatePickerDialog(Context, Dialog_DateSet, year: Value.Year, monthOfYear: Value.Month - 1,
                dayOfMonth: Value.Day);
        }

        protected override string GetText()
        {
            return string.IsNullOrEmpty(DateFormat) ? Value.ToShortDateString() : Value.ToString(DateFormat);
        }

        private void Dialog_DateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            dateTimeSet = new DateTime(e.Date.Date.Ticks + Value.TimeOfDay.Ticks, Value.Kind);
        }
    }
}