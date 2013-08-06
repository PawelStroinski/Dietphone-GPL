using System;
using System.Linq;

namespace Dietphone.Models
{
    public class Replacement
    {
        public MealItem For { get; set; }
        public Pattern Pattern { get; set; }
        public double PatternFactor { get; set; }
    }
}
