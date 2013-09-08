// Round method from https://github.com/mono/mono
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dietphone
{
    public static class AsReadOnlyShim
    {
        public static ReadOnlyCollection<T> AsReadOnly<T>(this List<T> list)
        {
            return new ReadOnlyCollection<T>(list);
        }
    }

    public enum MidpointRounding
    {
        ToEven = 0,
        AwayFromZero = 1
    }

    public static class MathShim
    {
        public static double Round(double value, MidpointRounding mode)
        {
            if (mode == MidpointRounding.ToEven)
                return Math.Round(value);
            if (value > 0)
                return Math.Floor(value + 0.5);
            else
                return Math.Ceiling(value - 0.5);
        }
    }
}
