using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Dietphone.Models
{
    public class Settings : Entity
    {
        public bool ScoreEnergy { get; set; }
        public bool ScoreProtein { get; set; }
        public bool ScoreDigestibleCarbs { get; set; }
        public bool ScoreFat { get; set; }
        public bool ScoreCu { get; set; }
        public bool ScoreFpu { get; set; }
        public int SugarsAfterInsulinHours { get; set; }
        public SugarUnit SugarUnit { get; set; }
        public float MaxBolus { get; set; }
        public List<Guid> MruProductIds { get; set; }
        public byte MruProductMaxCount { get; set; }
        public string CloudSecret { get; set; }
        public string CloudToken { get; set; }
        public DateTime CloudExportDue { get; set; }
        public Unit Unit { get; set; }
        public byte TrialCounter { get; set; }
        public bool ShowWelcomeScreen { get; set; }
        private string currentUiCulture;
        private string currentProductCulture;
        private string nextUiCulture;
        private string nextProductCulture;
        private readonly object currentUiCultureLock = new object();
        private readonly object currentProductCultureLock = new object();
        private readonly object nextUiCultureLock = new object();
        private readonly object nextProductCultureLock = new object();

        public Settings()
        {
            MruProductIds = new List<Guid>();
            MruProductMaxCount = 10;
            SugarsAfterInsulinHours = 4;
            SugarUnit = GetDefaultSugarUnit();
            MaxBolus = 5;
            CloudSecret = string.Empty;
            CloudToken = string.Empty;
            Unit = GetDefaultUnit();
            ShowWelcomeScreen = true;
        }

        public string CurrentUiCulture
        {
            get
            {
                lock (currentUiCultureLock)
                {
                    if (string.IsNullOrEmpty(currentUiCulture))
                    {
                        currentUiCulture = NextUiCulture;
                    }
                    return currentUiCulture;
                }
            }
        }

        public string CurrentProductCulture
        {
            get
            {
                lock (currentProductCultureLock)
                {
                    if (string.IsNullOrEmpty(currentProductCulture))
                    {
                        currentProductCulture = NextProductCulture;
                    }
                    return currentProductCulture;
                }
            }
        }

        public string NextUiCulture
        {
            get
            {
                lock (nextUiCultureLock)
                {
                    if (string.IsNullOrEmpty(nextUiCulture))
                    {
                        nextUiCulture = GetDefaultUiCulture();
                    }
                    return nextUiCulture;
                }
            }
            set
            {
                nextUiCulture = value;
            }
        }

        public string NextProductCulture
        {
            get
            {
                lock (nextProductCultureLock)
                {
                    if (string.IsNullOrEmpty(nextProductCulture))
                    {
                        nextProductCulture = GetDefaultProductCulture();
                    }
                    return nextProductCulture;
                }
            }
            set
            {
                nextProductCulture = value;
            }
        }

        private string GetDefaultUiCulture()
        {
            var cultures = new Cultures();
            return cultures.DefaultUiCulture;
        }

        private string GetDefaultProductCulture()
        {
            var cultures = new Cultures();
            return cultures.DefaultProductCulture;
        }

        private SugarUnit GetDefaultSugarUnit()
        {
            var cultures = new Cultures();
            var systemCulture = cultures.SystemCulture;
            return systemCulture.GetSugarUnitForCulture();
        }

        private Unit GetDefaultUnit()
        {
            var cultures = new Cultures();
            var systemCulture = cultures.SystemCulture;
            return systemCulture == "en-US" ? Unit.Ounce : Unit.Gram;
        }
    }

    public class Cultures
    {
        public string[] SupportedUiCultures
        {
            get
            {
                return new string[] { "en-US", "pl-PL" };
            }
        }

        public string[] SupportedProductCultures
        {
            get
            {
                return new string[] { "en-US", "en-GB", "pl-PL" };
            }
        }

        public Dictionary<string, string> SupportedProductCulturesBackup
        {
            get
            {
                return new Dictionary<string, string> { { "en-IE", "en-GB" } };
            }
        }

        public string DefaultUiCulture
        {
            get
            {
                var systemCulture = SystemCulture;
                if (SupportedUiCultures.Contains(systemCulture))
                {
                    return systemCulture;
                }
                else
                {
                    return SupportedUiCultures.FirstOrDefault();
                }
            }
        }

        public string DefaultProductCulture
        {
            get
            {
                var systemCulture = SystemCulture;
                if (SupportedProductCultures.Contains(systemCulture))
                {
                    return systemCulture;
                }
                else
                {
                    if (SupportedProductCulturesBackup.ContainsKey(systemCulture)
                        && SupportedProductCultures.Contains(SupportedProductCulturesBackup[systemCulture]))
                    {
                        return SupportedProductCulturesBackup[systemCulture];
                    }
                    return SupportedProductCultures.FirstOrDefault();
                }
            }
        }

        public string SystemCulture
        {
            get
            {
                var thread = Thread.CurrentThread;
                var culture = thread.CurrentCulture;
                return culture.Name;
            }
        }
    }
}
