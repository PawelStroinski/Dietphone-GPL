using System;
using System.Xml.Serialization;
using Dietphone.Views;

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

    public static class SugarUnitAbbreviations
    {
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
    }

    public enum SugarUnit
    {
        mgdL,
        mmolL
    }
}
