using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public abstract class ViewModelWithDateAndText : ViewModelWithDate
    {
        public abstract string Text { get; }

        public bool FilterIn(string filter)
        {
            return Text.ContainsIgnoringCase(filter);
        }
    }
}
