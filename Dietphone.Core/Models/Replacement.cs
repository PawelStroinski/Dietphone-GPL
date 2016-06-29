using System;
using System.Linq;
using System.Collections.Generic;

namespace Dietphone.Models
{
    public class Replacement
    {
        public IList<ReplacementItem> Items { get; set; }
        public bool IsComplete { get; set; }
        public Insulin InsulinTotal { get; set; }
    }

    public class ReplacementItem
    {
        public ReplacementItem()
        {
        }

        public ReplacementItem(Pattern pattern)
        {
            Pattern = pattern;
        }

        public Pattern Pattern { get; set; }
        public IList<Pattern> Alternatives { get; set; }
    }
}
