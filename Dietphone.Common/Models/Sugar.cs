using System;
using System.Xml.Serialization;

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
    }

    public enum SugarUnit
    {
        mgdL,
        mmolL
    }
}
