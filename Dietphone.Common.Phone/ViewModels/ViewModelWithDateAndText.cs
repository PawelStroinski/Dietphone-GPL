using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dietphone.ViewModels
{
    public abstract class ViewModelWithDateAndText : ViewModelWithDate
    {
        public abstract string Text { get; }
    }
}
