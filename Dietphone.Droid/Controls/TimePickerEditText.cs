// Idea from: http://benjaminhysell.com/archive/2014/04/mvvmcross-xamarin-android-popup-datepicker-on-edittext-click/
using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Util;

namespace Dietphone.Controls
{
    public sealed class TimePickerEditText : DateTimePickerEditText
    {
        public TimePickerEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        protected override AlertDialog CreateDialog()
        {
            return new TimePickerDialog(Context, Dialog_TimeSet, hourOfDay: Value.Hour, minute: Value.Minute,
                is24HourView: Is24HourClock());
        }

        protected override string GetText()
        {
            return Value.ToShortTimeString();
        }

        private void Dialog_TimeSet(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            var time = new TimeSpan(hours: e.HourOfDay, minutes: e.Minute, seconds: 0);
            dateTimeSet = new DateTime(Value.Date.Ticks + time.Ticks, Value.Kind);
        }

        private bool Is24HourClock()
        {
            var culture = CultureInfo.CurrentCulture;
            var format = culture.DateTimeFormat;
            var pattern = format.ShortTimePattern;
            return pattern.Contains("H");
        }
    }
}