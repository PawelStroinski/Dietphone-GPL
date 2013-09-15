using System;
using Dietphone.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using Dietphone.Tools;
using System.Collections.Generic;
using Dietphone.Views;

namespace Dietphone.ViewModels
{
    public class InsulinViewModel : ViewModelBase
    {
        public Insulin Insulin { get; private set; }
        private readonly Factories factories;
        private static readonly Constrains maxHours = new Constrains { Max = 8 };

        public InsulinViewModel(Insulin insulin, Factories factories)
        {
            Insulin = insulin;
            this.factories = factories;
        }

        public Guid Id
        {
            get
            {
                return Insulin.Id;
            }
        }

        public DateTime DateTime
        {
            get
            {
                var universal = Insulin.DateTime;
                return universal.ToLocalTime();
            }
            set
            {
                var universal = value.ToUniversalTime();
                if (Insulin.DateTime != universal)
                {
                    Insulin.DateTime = universal;
                    OnPropertyChanged("DateTime");
                    OnPropertyChanged("DateOnly");
                    OnPropertyChanged("DateAndTime");
                    OnPropertyChanged("Time");
                }
            }
        }

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

        public string Time
        {
            get
            {
                return DateTime.ToShortTimeString();
            }
        }

        public string Note
        {
            get
            {
                return Insulin.Note;
            }
            set
            {
                if (value != Insulin.Note)
                {
                    Insulin.Note = value;
                    OnPropertyChanged("Note");
                }
            }
        }

        public string NormalBolus
        {
            get
            {
                var result = Insulin.NormalBolus;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Insulin.NormalBolus;
                var newValue = oldValue.TryGetValueOf(value);
                var settings = factories.Settings;
                var maxBolus = new Constrains { Max = settings.MaxBolus };
                Insulin.NormalBolus = maxBolus.Constraint(newValue);
                OnPropertyChanged("NormalBolus");
            }
        }

        public string SquareWaveBolus
        {
            get
            {
                var result = Insulin.SquareWaveBolus;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Insulin.SquareWaveBolus;
                var newValue = oldValue.TryGetValueOf(value);
                var settings = factories.Settings;
                var maxBolus = new Constrains { Max = settings.MaxBolus };
                Insulin.SquareWaveBolus = maxBolus.Constraint(newValue);
                OnPropertyChanged("SquareWaveBolus");
            }
        }

        public string SquareWaveBolusHours
        {
            get
            {
                var result = Insulin.SquareWaveBolusHours;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Insulin.SquareWaveBolusHours;
                var newValue = oldValue.TryGetValueOf(value);
                Insulin.SquareWaveBolusHours = maxHours.Constraint(newValue);
                OnPropertyChanged("SquareWaveBolusHours");
            }
        }
    }
}
