using Dietphone.Models;
using Dietphone.Tools;
using System;

namespace Dietphone.ViewModels
{
    public class InsulinEditingViewModel : EditingViewModelBase<Insulin>
    {
        public InsulinViewModel Insulin { get; private set; }
        public SugarViewModel CurrentSugar { get; private set; }

        public InsulinEditingViewModel(Factories factories)
            : base(factories)
        {
        }

        protected override void FindAndCopyModel()
        {
            var id = Navigator.GetInsulinIdToEdit();
            if (id == Guid.Empty)
                modelSource = factories.CreateInsulin();
            else
                modelSource = finder.FindInsulinById(id);
            if (modelSource != null)
            {
                modelCopy = modelSource.GetCopy();
                modelCopy.SetOwner(factories);
            }
        }

        protected override void MakeViewModel()
        {
            MakeInsulinViewModelInternal();
            CurrentSugar = new SugarViewModel(new Sugar(), factories);
        }

        protected override string Validate()
        {
            return string.Empty;
        }

        private void MakeInsulinViewModelInternal()
        {
            Insulin = new InsulinViewModel(modelCopy, factories);
            Insulin.PropertyChanged += delegate
            {
                IsDirty = true;
            };
        }
    }
}
