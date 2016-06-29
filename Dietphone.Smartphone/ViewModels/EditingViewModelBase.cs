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
        public event EventHandler IsDirtyChanged;
        protected TModel modelCopy;
        protected TModel modelSource;
        protected readonly Factories factories;
        protected readonly Finder finder;
        protected readonly MessageDialog messageDialog;
        private TViewModel subject;
        private bool isDirty;
        private const string IS_DIRTY = "IS_DIRTY";

        public EditingViewModelBase(Factories factories, MessageDialog messageDialog)
        {
            this.factories = factories;
            finder = factories.Finder;
            this.messageDialog = messageDialog;
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

        public void SaveAndReturn()
        {
            if (CanSave())
                DoSaveAndReturn();
        }

        public virtual void CancelAndReturn()
        {
            Navigator.GoBack();
        }

        private bool CanSave()
        {
            var validation = Validate();
            return string.IsNullOrEmpty(validation)
                || messageDialog.Confirm(validation, Messages.CannotSaveCaption);
        }

        protected abstract void DoSaveAndReturn();

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

        internal abstract Messages Messages { get; }
    }

    public class Messages
    {
        public string CannotSaveCaption { get; set; }
    }
}
