using System;
using Dietphone.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using Dietphone.Tools;
using System.Collections.Generic;
using Dietphone.Views;
using System.Collections;

namespace Dietphone.ViewModels
{
    public class InsulinViewModel : ViewModelWithDate
    {
        public Insulin Insulin { get; private set; }
        //private ObservableCollection<InsulinCircumstanceViewModel> circumstances;
        //private readonly object circumstancesLock = new object();
        private readonly Factories factories;
        private readonly IEnumerable<InsulinCircumstanceViewModel> allCircumstances;
        private static readonly Constrains maxHours = new Constrains { Max = 8 };

        public InsulinViewModel(Insulin insulin, Factories factories, IEnumerable<InsulinCircumstanceViewModel> allCircumstances)
        {
            Insulin = insulin;
            this.factories = factories;
            this.allCircumstances = allCircumstances;
        }

        public Guid Id
        {
            get
            {
                return Insulin.Id;
            }
        }

        public override DateTime DateTime
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

        //public ObservableCollection<InsulinCircumstanceViewModel> Circumstances
        //{
        //    get
        //    {
        //        lock (circumstancesLock)
        //        {
        //            if (circumstances == null)
        //            {
        //                var model = Insulin.Circumstances.ToList();
        //                var viewModels = model.Select(circumstance => new InsulinCircumstanceViewModel(
        //                    circumstance, factories)).ToList();
        //                circumstances = new ObservableCollection<InsulinCircumstanceViewModel>(viewModels);
        //            }
        //            return circumstances;
        //        }
        //    }
        //    set
        //    {
        //        if (value == null)
        //            throw new NullReferenceException("value");
        //        var newItems = value.Except(Circumstances);
        //        foreach (var item in newItems)
        //            Insulin.AddCircumstance(item.Model);
        //        var removedItems = Circumstances.Except(value);
        //        foreach (var item in removedItems)
        //            Insulin.RemoveCircumstance(item.Model);
        //    }
        //}

        public IEnumerable<InsulinCircumstanceViewModel> AllCircumstances()
        {
            return allCircumstances;
        }
    }
}
