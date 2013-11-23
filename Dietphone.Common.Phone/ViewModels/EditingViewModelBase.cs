using System;
using Dietphone.Models;
using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public abstract class EditingViewModelBase<TModel, TViewModel> : PivotTombstoningViewModel
        where TModel : EntityWithId
        where TViewModel : ViewModelBase
    {
        public Navigator Navigator { get; set; }
        public event EventHandler<CannotSaveEventArgs> CannotSave;
        public event EventHandler IsDirtyChanged;
        protected TModel modelCopy;
        protected TModel modelSource;
        protected readonly Factories factories;
        protected readonly Finder finder;
        private TViewModel subject;
        private bool isDirty;
        private const string IS_DIRTY = "IS_DIRTY";

        public EditingViewModelBase(Factories factories)
        {
            this.factories = factories;
            finder = factories.Finder;
        }

        public virtual TViewModel Subject
        {
            get
            {
                return subject;
            }
            protected set
            {
                this.subject = value;
            }
        }

        public bool IsDirty
        {
            get
            {
                return isDirty;
            }
            set
            {
                if (isDirty != value)
                {
                    isDirty = value;
                    OnIsDirtyChanged();
                }
            }
        }

        public void Load()
        {
            FindAndCopyModel();
            if (modelCopy == null)
            {
                Navigator.GoBack();
            }
            else
            {
                UntombstoneModel();
                OnModelReady();
                MakeViewModel();
                UntombstoneOtherThings();
                UntombstoneCommonUi();
                OnCommonUiReady();
            }
        }

        public override void Tombstone()
        {
            TombstoneModel();
            TombstoneOtherThings();
            TombstoneCommonUi();
        }

        public override void Untombstone()
        {
            throw new NotSupportedException("Use Load() instead");
        }

        public bool CanSave()
        {
            var validation = Validate();
            if (!string.IsNullOrEmpty(validation))
            {
                var args = new CannotSaveEventArgs();
                args.Reason = validation;
                OnCannotSave(args);
                return args.Ignore;
            }
            return true;
        }

        public void CancelAndReturn()
        {
            Navigator.GoBack();
        }

        protected abstract void FindAndCopyModel();

        protected abstract void MakeViewModel();

        protected abstract string Validate();

        protected virtual void TombstoneModel()
        {
            var key = typeof(TModel).ToString();
            var state = StateProvider.State;
            state[key] = modelCopy.Serialize(string.Empty);
        }

        protected virtual void UntombstoneModel()
        {
            var key = typeof(TModel).ToString();
            var state = StateProvider.State;
            if (state.ContainsKey(key))
            {
                var stateValue = (string)state[key];
                var untombstoned = stateValue.Deserialize<TModel>(string.Empty);
                if (untombstoned.Id == modelCopy.Id)
                {
                    modelCopy.CopyFrom(untombstoned);
                }
            }
        }

        protected virtual void TombstoneOtherThings()
        {
        }

        protected virtual void UntombstoneOtherThings()
        {
        }

        private void TombstoneCommonUi()
        {
            var state = StateProvider.State;
            state[IS_DIRTY] = IsDirty;
            TombstonePivot();
        }

        private void UntombstoneCommonUi()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(IS_DIRTY))
            {
                IsDirty = (bool)state[IS_DIRTY];
                UntombstonePivot();
            }
        }

        protected void OnCannotSave(CannotSaveEventArgs e)
        {
            if (CannotSave != null)
            {
                CannotSave(this, e);
            }
        }

        protected void OnIsDirtyChanged()
        {
            if (IsDirtyChanged != null)
            {
                IsDirtyChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnModelReady()
        {
        }

        protected virtual void OnCommonUiReady()
        {
        }
    }

    public class CannotSaveEventArgs : EventArgs
    {
        public string Reason { get; set; }
        public bool Ignore { get; set; }
    }
}
