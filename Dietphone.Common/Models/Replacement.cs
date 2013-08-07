using System;
using System.Linq;
using System.Collections.Generic;

namespace Dietphone.Models
{
    public class Replacements
    {
        public IList<Replacement> Items { get; set; }
        public bool Complete { get; set; }
    }

    public class Replacement
    {
        public Pattern Pattern { get; set; }
        public double PatternFactor { get; set; }
    }
}
