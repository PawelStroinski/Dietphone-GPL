using System;
using System.Linq;

namespace Dietphone.Models
{
    public interface HourDifference
    {
        int GetDifference(TimeSpan left, TimeSpan right);
    }

    public class HourDifferenceImpl : HourDifference
    {
        public int GetDifference(TimeSpan left, TimeSpan right)
        {
            return RoundHour(GetDifference(left.TotalHours, right.TotalHours));
        }

        private double GetDifference(double left, double right)
        {
            var larger = left > right ? left : right;
            var smaller = left > right ? right : left;
            if (smaller < 12 && larger > 12)
            {
                var proposition = 24 - larger + smaller;
                if (proposition < 12)
                    return proposition;
            }
            return larger - smaller;
        }

        private int RoundHour(double hour)
        {
            if (Math.Round(hour - (int)hour, 3) > 0.5)
                return (int)hour + 1;
            return (int)hour;
        }
    }
}
