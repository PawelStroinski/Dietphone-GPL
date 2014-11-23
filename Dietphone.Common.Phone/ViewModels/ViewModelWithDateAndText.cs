using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public abstract class ViewModelWithDateAndText : ViewModelWithDate
    {
        public abstract string Text { get; }
        public abstract string Text2 { get; }

        public virtual bool FilterIn(string filter)
        {
            return Text.ContainsIgnoringCase(filter) || Text2.ContainsIgnoringCase(filter);
        }
    }
}
