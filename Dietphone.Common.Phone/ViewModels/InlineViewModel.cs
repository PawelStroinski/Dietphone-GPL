using System;
using Dietphone.Tools;

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
        }

        public void Confirm()
        {
            OnConfirmed();
            OnHidden();
        }

        public void Cancel()
        {
            OnCancelled();
            OnHidden();
        }

        public void Delete()
        {
            OnNeedToDelete();
            OnHidden();
        }

        public abstract void Tombstone();

        protected abstract void Untombstone();

        private void OnHidden()
        {
            IsVisible = false;
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