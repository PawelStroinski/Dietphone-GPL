using Dietphone.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Dietphone.Tools;
using System;
using System.Globalization;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Dietphone.ViewModels
{
    public class InsulinEditingViewModel : EditingViewModelBase<Insulin>
    {
        public InsulinViewModel Insulin { get; private set; }

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
