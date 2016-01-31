using System;
using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public abstract class JournalItemViewModel : ViewModelWithDate
    {
        public abstract Guid Id { get; }
        public abstract string Text { get; }
        public abstract string Text2 { get; }
        public abstract bool IsInsulin { get; }
        public abstract bool IsSugar { get; }
        public abstract bool IsMeal { get; }
        public abstract bool IsNotMeal { get; }

        public bool HasText2
        {
            get
            {
                return !string.IsNullOrEmpty(Text2);
            }
        }

        public virtual bool FilterIn(string filter)
        {
            return Text.ContainsIgnoringCase(filter) || Text2.ContainsIgnoringCase(filter);
        }

        public virtual void Choose(Navigator navigator)
        {
        }
    }
}
