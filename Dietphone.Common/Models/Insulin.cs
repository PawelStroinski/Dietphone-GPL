using System;

namespace Dietphone.Models
{
    public class Insulin : EntityWithId
    {
        public DateTime DateTime { get; set; }
        public string Note { get; set; }
        public float NormalBolus { get; set; }
        public float SquareWaveBolus { get; set; }
        public float SquareWaveBolusHours { get; set; }
    }
}
