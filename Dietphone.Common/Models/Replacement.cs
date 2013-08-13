using System;
using System.Linq;
using System.Collections.Generic;

namespace Dietphone.Models
{
    public class Replacements
    {
        public IList<Replacement> Items { get; set; }
        public bool IsComplete { get; set; }
    }

    public class Replacement
    {
        public Pattern Pattern { get; set; }
    }
}
