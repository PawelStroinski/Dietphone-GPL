using System;
using System.Xml.Serialization;
using Dietphone.Views;
using System.Linq;

namespace Dietphone.Models
{
    public class Sugar : EntityWithId
    {
        private const int UNIT_CONVERSION = 18;
        public DateTime DateTime { get; set; }
        public float BloodSugar { get; set; }

        [XmlIgnore]
        public float BloodSugarInMgdL
        {
            get
            {
                var settings = Owner.Settings;
                if (settings.SugarUnit == SugarUnit.mgdL)
                    return BloodSugar;
                else
                    return (float)Math.Round(BloodSugar * UNIT_CONVERSION);
            }
        }

        public override bool Equals(object obj)
        {
            var sugar = obj as Sugar;
            if (sugar == null
                 || sugar.DateTime != DateTime
                 || sugar.BloodSugar != BloodSugar)
                return false;
            else
                return true;
        }

        public override int GetHashCode()
        {
            return DateTime.GetHashCode() * 2
                 + BloodSugar.GetHashCode() * 3;
        }
    }

    public static class SugarUnitExtensions
    {
        private static string[] mmolLCultures = { "en-AU", "bg-BG", "en-CA", "zh-CN", "hr-HR", "hr-BA", "bs-Latn-BA", 
                                                  "cs-CZ", "da-DK", "et-EE", "fi-FI", "zh-HK", "hu-HU", "is-IS", "en-IE",
                                                  "lv-LV", "lt-LT", "de-LU", "fr-LU", "mk-MK", "mt-MT", "en-NZ", "nn-NO",
                                                  "ru-RU", "sr-Cyrl-BA", "sr-Cyrl-CS", "sr-Latn-BA", "sr-Latn-CS",
                                                  "sk-SK", "sl-SI", "sv-SE", "de-CH", "en-GB" };

        public static string GetAbbreviation(this SugarUnit unit)
        {
            switch (unit)
            {
                case SugarUnit.mgdL:
                    return Translations.MgdL;
                case SugarUnit.mmolL:
                    return Translations.MmolL;
                default:
                    return string.Empty;
            }
        }

        public static SugarUnit GetSugarUnitForCulture(this string culture)
        {
            return mmolLCultures.Contains(culture) ? SugarUnit.mmolL : SugarUnit.mgdL;
        }
    }

    public enum SugarUnit
    {
        mgdL,
        mmolL
    }
}
