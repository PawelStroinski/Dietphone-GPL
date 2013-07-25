using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dietphone.Models
{
    public class Pattern
    {
        public byte RightnessPoints { get; set; }
        public MealItem Match { get; set; }
        public Meal From { get; set; }
        public Insulin Insulin { get; set; }
        public Sugar Before { get; set; }
        public IEnumerable<Sugar> After { get; set; }
    }
}
