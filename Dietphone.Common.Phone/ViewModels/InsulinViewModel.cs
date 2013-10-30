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
        private IList<InsulinCircumstanceViewModel> circumstances;
        private IList<InsulinCircumstanceViewModel> allCircumstances;
        private readonly object circumstancesLock = new object();
        private readonly Factories factories;
        private static readonly Constrains maxHours = new Constrains { Max = 8 };

        public InsulinViewModel(Insulin insulin, Factories factories,
            IList<InsulinCircumstanceViewModel> allCircumstances)
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

        public IList<InsulinCircumstanceViewModel> Circumstances
        {
            get
            {
                lock (circumstancesLock)
                {
                    if (circumstances == null)
                    {
                        var circumtanceIds = Insulin.ReadCircumstances();
                        circumstances = allCircumstances
                            .Where(circumstance => circumtanceIds.Contains(circumstance.Id))
                            .ToList();
                    }
                    return circumstances;
                }
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value");
                if (value == Circumstances)
                    throw new ArgumentException("value");
                lock (circumstancesLock)
                {
                    var newItems = value.Except(Circumstances);
                    foreach (var item in newItems)
                        Insulin.AddCircumstance(item.Model);
                    var removedItems = Circumstances.Except(value);
                    foreach (var item in removedItems)
                        Insulin.RemoveCircumstance(item.Model);
                    if (newItems.Any() || removedItems.Any())
                        circumstances = null;
                }
                OnPropertyChanged("Circumstances");
            }
        }

        public IEnumerable<InsulinCircumstanceViewModel> AllCircumstances()
        {
            return allCircumstances;
        }

        public void InvalidateCircumstances(IList<InsulinCircumstanceViewModel> allCircumstances)
        {
            lock (circumstancesLock)
            {
                this.allCircumstances = allCircumstances;
                this.circumstances = null;
            }
        }
    }
}
