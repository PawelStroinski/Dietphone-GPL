using System;
using System.ComponentModel;
using Dietphone.Models;

namespace Dietphone.ViewModels
{
    public abstract class EditingViewModelWithDate<TModel, TViewModel> : EditingViewModelBase<TModel, TViewModel>
        where TModel : EntityWithId
        where TViewModel : ViewModelWithDate
    {
        protected bool isLockedDateTime;
        protected bool updatingLockedDateTime;
        private const byte LOCKED_DATE_TIME_RECENT_MINUTES = 3;
        private const string NOT_IS_LOCKED_DATE_TIME = "NOT_IS_LOCKED_DATE_TIME";

        public EditingViewModelWithDate(Factories factories)
            : base(factories)
        {
        }

        public override TViewModel Subject
        {
            protected set
            {
                base.Subject = value;
                Subject.PropertyChanged += Subject_PropertyChanged;
            }
        }

        // Note: changing NotIsLockedDateTime may change the Subject.DateTime
        // with help of UpdateLockedDateTime().
        public bool NotIsLockedDateTime
        {
            get
            {
                return !isLockedDateTime;
            }
            set
            {
                if (!isLockedDateTime != value)
                {
                    isLockedDateTime = !value;
                    UpdateLockedDateTime();
                    OnPropertyChanged("NotIsLockedDateTime");
                }
            }
        }

        protected void LockRecentDateTime()
        {
            var difference = (DateTime.Now - Subject.DateTime).Duration();
            isLockedDateTime = difference <= TimeSpan.FromMinutes(LOCKED_DATE_TIME_RECENT_MINUTES);
        }

        protected void UpdateLockedDateTime()
        {
            if (isLockedDateTime)
            {
                updatingLockedDateTime = true;
                Subject.DateTime = DateTime.Now;
                updatingLockedDateTime = false;
            }
        }

        protected override void TombstoneOthers()
        {
            var state = StateProvider.State;
            state[NOT_IS_LOCKED_DATE_TIME] = NotIsLockedDateTime;
        }

        protected override void UntombstoneOthers()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(NOT_IS_LOCKED_DATE_TIME))
            {
                NotIsLockedDateTime = (bool)state[NOT_IS_LOCKED_DATE_TIME];
            }
        }

        private void Subject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsDirty = true;
            if (e.PropertyName == "DateTime" && !updatingLockedDateTime)
            {
                NotIsLockedDateTime = true;
            }
        }
    }
}
