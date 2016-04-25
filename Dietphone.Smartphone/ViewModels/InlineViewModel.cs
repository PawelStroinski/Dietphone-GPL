using System;
using System.Windows.Input;
using Dietphone.Tools;
using MvvmCross.Core.ViewModels;

namespace Dietphone.ViewModels
{
    public abstract class InlineViewModel<T> : ViewModelBase
    {
        public bool CanDelete { get; set; }
        public StateProvider StateProvider { protected get; set; }
        public T Subject { get; private set; }
        public bool IsVisible { get; private set; }
        public event EventHandler NeedToShow;
        public event EventHandler Confirmed;
        public event EventHandler Cancelled;
        public event EventHandler NeedToDelete;

        public void Show(T subject)
        {
            Subject = subject;
            Untombstone();
            OnNeedToShow();
            IsVisible = true;
            OnPropertyChanged("IsVisible");
        }

        public ICommand Confirm
        {
            get
            {
                return new MvxCommand(() =>
                {
                    OnConfirmed();
                    OnHidden();
                });
            }
        }

        public ICommand Cancel
        {
            get
            {
                return new MvxCommand(() =>
                {
                    OnCancelled();
                    OnHidden();
                });
            }
        }

        public ICommand Delete
        {
            get
            {
                return new MvxCommand(() =>
                {
                    OnNeedToDelete();
                    OnHidden();
                });
            }
        }

        public abstract void Tombstone();

        protected abstract void Untombstone();

        private void OnHidden()
        {
            IsVisible = false;
            OnPropertyChanged("IsVisible");
            ClearTombstoning();
        }

        protected abstract void ClearTombstoning();

        protected void OnNeedToShow()
        {
            if (NeedToShow != null)
            {
                NeedToShow(this, EventArgs.Empty);
            }
        }

        protected void OnConfirmed()
        {
            if (Confirmed != null)
            {
                Confirmed(this, EventArgs.Empty);
            }
        }

        protected void OnCancelled()
        {
            if (Cancelled != null)
            {
                Cancelled(this, EventArgs.Empty);
            }
        }

        protected void OnNeedToDelete()
        {
            if (NeedToDelete != null)
            {
                NeedToDelete(this, EventArgs.Empty);
            }
        }
    }
}