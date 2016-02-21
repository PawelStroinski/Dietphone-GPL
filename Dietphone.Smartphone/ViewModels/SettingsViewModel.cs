// Note: enabling property from Score* group may disable other properties from this group automatically.
using System;
using System.Collections.Generic;
using Dietphone.Models;
using System.Globalization;
using System.Windows;
using Dietphone.Tools;
using System.Linq;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;

namespace Dietphone.ViewModels
{
    public class SettingsViewModel : PivotTombstoningViewModel
    {
        public List<string> UiCultures { get; private set; }
        public List<string> ProductCultures { get; private set; }
        private readonly Settings settings;
        private readonly LearningCuAndFpu learningCuAndFpu;
        private static readonly Constrains number = new Constrains { Min = 0.1, Max = 100 };
        private static readonly Constrains hours = new Constrains { Min = 1, Max = 12 };
        private const byte MAX_SCORES = 4;

        public SettingsViewModel(Factories factories, LearningCuAndFpu learningCuAndFpu)
        {
            settings = factories.Settings;
            this.learningCuAndFpu = learningCuAndFpu;
            UiCultures = new List<string>();
            ProductCultures = new List<string>();
            BuildUiCulturesAndProductCultures();
        }

        public bool ScoreEnergy
        {
            get
            {
                return settings.ScoreEnergy;
            }
            set
            {
                if (settings.ScoreEnergy != value)
                {
                    settings.ScoreEnergy = value;
                    OnPropertyChanged("ScoreEnergy");
                    if (value)
                    {
                        DisableFpuAndCuIfTooManyScores();
                    }
                }
            }
        }

        public bool ScoreProtein
        {
            get
            {
                return settings.ScoreProtein;
            }
            set
            {
                if (settings.ScoreProtein != value)
                {
                    settings.ScoreProtein = value;
                    OnPropertyChanged("ScoreProtein");
                    if (value)
                    {
                        DisableFpuAndCuIfTooManyScores();
                    }
                }
            }
        }

        public bool ScoreDigestibleCarbs
        {
            get
            {
                return settings.ScoreDigestibleCarbs;
            }
            set
            {
                if (settings.ScoreDigestibleCarbs != value)
                {
                    settings.ScoreDigestibleCarbs = value;
                    OnPropertyChanged("ScoreDigestibleCarbs");
                    if (value)
                    {
                        DisableFpuAndCuIfTooManyScores();
                    }
                }
            }
        }

        public bool ScoreFat
        {
            get
            {
                return settings.ScoreFat;
            }
            set
            {
                if (settings.ScoreFat != value)
                {
                    settings.ScoreFat = value;
                    OnPropertyChanged("ScoreFat");
                    if (value)
                    {
                        DisableFpuAndCuIfTooManyScores();
                    }
                }
            }
        }

        public bool ScoreCu
        {
            get
            {
                return settings.ScoreCu;
            }
            set
            {
                if (settings.ScoreCu != value)
                {
                    settings.ScoreCu = value;
                    OnPropertyChanged("ScoreCu");
                    if (value)
                    {
                        DisableNutrientsIfTooManyScores();
                    }
                }
            }
        }

        public bool ScoreFpu
        {
            get
            {
                return settings.ScoreFpu;
            }
            set
            {
                if (settings.ScoreFpu != value)
                {
                    settings.ScoreFpu = value;
                    OnPropertyChanged("ScoreFpu");
                    if (value)
                    {
                        DisableNutrientsIfTooManyScores();
                    }
                }
            }
        }

        public string UiCulture
        {
            get
            {
                var cultureName = settings.NextUiCulture;
                return GetUiCultureFromCultureName(cultureName);
            }
            set
            {
                var cultureName = FindCultureNameByUiCulture(value);
                if (settings.NextUiCulture != cultureName)
                {
                    settings.NextUiCulture = cultureName;
                    OnPropertyChanged("CultureChanged");
                }
            }
        }

        public string ProductCulture
        {
            get
            {
                var cultureName = settings.NextProductCulture;
                return GetProductCultureFromCultureName(cultureName);
            }
            set
            {
                var cultureName = FindCultureNameByProductCulture(value);
                if (settings.NextProductCulture != cultureName)
                {
                    settings.NextProductCulture = cultureName;
                    OnPropertyChanged("CultureChanged");
                }
            }
        }

        public bool CultureChanged
        {
            get
            {
                var uiCultureChanged = settings.CurrentUiCulture != settings.NextUiCulture;
                var productCultureChanged = settings.CurrentProductCulture != settings.NextProductCulture;
                var cultureChanged = uiCultureChanged || productCultureChanged;
                return cultureChanged;
            }
        }

        public List<string> AllSugarUnits
        {
            get
            {
                return MyEnum.GetValues<SugarUnit>()
                    .Select(unit => unit.GetAbbreviation())
                    .ToList();
            }
        }

        public string SugarUnit
        {
            get
            {
                var result = settings.SugarUnit;
                return result.GetAbbreviation();
            }
            set
            {
                var newValue = MyEnum.GetValues<SugarUnit>()
                    .Where(unit => value == unit.GetAbbreviation());
                if (newValue.Any())
                    settings.SugarUnit = newValue.Single();
                OnPropertyChanged("SugarUnit");
            }
        }

        public List<string> AllUnits
        {
            get
            {
                return MyEnum.GetValues<Unit>()
                    .Where(unit => unit.IsConvertibleToGram())
                    .Select(unit => unit.GetAbbreviation())
                    .ToList();
            }
        }

        public string Unit
        {
            get
            {
                var result = settings.Unit;
                return result.GetAbbreviation();
            }
            set
            {
                var newValue = MyEnum.GetValues<Unit>()
                    .Where(unit => value == unit.GetAbbreviation());
                if (newValue.Any())
                    settings.Unit = newValue.Single();
                OnPropertyChanged("Unit");
            }
        }

        public string MaxBolus
        {
            get
            {
                var result = settings.MaxBolus;
                return result.ToString();
            }
            set
            {
                var oldValue = settings.MaxBolus;
                var newValue = oldValue.TryGetValueOf(value);
                settings.MaxBolus = number.Constraint(newValue);
                OnPropertyChanged("MaxBolus");
            }
        }

        public string MruProductMaxCount
        {
            get
            {
                var result = settings.MruProductMaxCount;
                return result.ToString();
            }
            set
            {
                var oldValue = settings.MruProductMaxCount;
                var newValue = ((short)oldValue).TryGetValueOf(value);
                settings.MruProductMaxCount = (byte)number.Constraint(newValue);
                OnPropertyChanged("MruProductMaxCount");
            }
        }

        public string SugarsAfterInsulinHours
        {
            get
            {
                var result = settings.SugarsAfterInsulinHours;
                return result.ToString();
            }
            set
            {
                var oldValue = settings.SugarsAfterInsulinHours;
                var newValue = ((short)oldValue).TryGetValueOf(value);
                settings.SugarsAfterInsulinHours = hours.Constraint(newValue);
                OnPropertyChanged("SugarsAfterInsulinHours");
            }
        }

        public ICommand LearnCuAndFpu
        {
            get
            {
                return new MvxCommand(() =>
                {
                    learningCuAndFpu.LearnCuAndFpu();
                });
            }
        }

        private void DisableFpuAndCuIfTooManyScores()
        {
            if (IsTooManyScores)
            {
                ScoreFpu = false;
            }
            if (IsTooManyScores)
            {
                ScoreCu = false;
            }
        }

        private void DisableNutrientsIfTooManyScores()
        {
            if (IsTooManyScores)
            {
                ScoreProtein = false;
            }
            if (IsTooManyScores)
            {
                ScoreDigestibleCarbs = false;
            }
            if (IsTooManyScores)
            {
                ScoreFat = false;
            }
        }

        private bool IsTooManyScores
        {
            get
            {
                var scoresCount = Convert.ToByte(ScoreEnergy) + Convert.ToByte(ScoreProtein) +
                    Convert.ToByte(ScoreDigestibleCarbs) + Convert.ToByte(ScoreFat) +
                    Convert.ToByte(ScoreCu) + Convert.ToByte(ScoreFpu);
                return scoresCount > MAX_SCORES;
            }
        }

        private void BuildUiCulturesAndProductCultures()
        {
            var cultures = new Cultures();
            foreach (var cultureName in cultures.SupportedUiCultures)
            {
                var uiCulture = GetUiCultureFromCultureName(cultureName);
                UiCultures.Add(uiCulture);
            }
            foreach (var cultureName in cultures.SupportedProductCultures)
            {
                var productCulture = GetProductCultureFromCultureName(cultureName);
                ProductCultures.Add(productCulture);
            }
        }

        private string GetUiCultureFromCultureName(string cultureName)
        {
            var result = GetProductCultureFromCultureName(cultureName);
            var bracketPos = result.IndexOf('(');
            if (bracketPos != -1)
            {
                result = result.Remove(bracketPos);
                result = result.Trim();
            }
            return result;
        }

        private string GetProductCultureFromCultureName(string cultureName)
        {
            var culture = new CultureInfo(cultureName);
            return culture.DisplayName;
        }

        private string FindCultureNameByUiCulture(string uiCulture)
        {
            var index = UiCultures.IndexOf(uiCulture);
            var cultures = new Cultures();
            return cultures.SupportedUiCultures[index];
        }

        private string FindCultureNameByProductCulture(string productCulture)
        {
            var index = ProductCultures.IndexOf(productCulture);
            var cultures = new Cultures();
            return cultures.SupportedProductCultures[index];
        }
    }
}
