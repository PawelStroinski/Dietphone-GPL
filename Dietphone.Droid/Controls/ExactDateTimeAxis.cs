using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot.Axes;

namespace Dietphone.Controls
{
    public class ExactDateTimeAxis : DateTimeAxis
    {
        private readonly Func<IEnumerable<DateTime>> dateTimes;

        public ExactDateTimeAxis(Func<IEnumerable<DateTime>> dateTimes)
        {
            this.dateTimes = dateTimes;
        }

        public override void GetTickValues(out IList<double> majorLabelValues, out IList<double> majorTickValues,
            out IList<double> minorTickValues)
        {
            majorLabelValues = majorTickValues = minorTickValues = dateTimes().Select(ToDouble).ToList();
        }
    }
}