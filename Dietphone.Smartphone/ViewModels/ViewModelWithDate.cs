using System;
using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public abstract class ViewModelWithDate : ViewModelBase
    {
        public DateViewModel Date { get; set; }

        public abstract DateTime DateTime { get; set; }

        public DateTime DateOnly
        {
            get
            {
                return DateTime.Date;
            }
        }

        public string DateAndTime
        {
            get
            {
                var date = DateTime.ToShortDateInAlternativeFormat();
                return string.Format("{0} {1}", date, Time);
            }
        }

        public string LongDateAndTime
        {
            get
            {
                return string.Format("{0}, {1}", DateTime.ToString("dddd"), DateAndTime);
            }
        }

        public string Time
        {
            get
            {
                return DateTime.ToString("t");
            }
        }

        public bool IsNewer
        {
            get
            {
                if (Date == null)
                {
                    return true;
                }
                else
                {
                    return !Date.IsGroupOfOlder;
                }
            }
        }

        public bool IsOlder
        {
            get
            {
                return !IsNewer;
            }
        }

        protected void NotifyDateTimeChange()
        {
            OnPropertyChanged("DateTime");
            OnPropertyChanged("DateOnly");
            OnPropertyChanged("DateAndTime");
            OnPropertyChanged("Time");
        }
    }
}
