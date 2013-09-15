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
        private const string INSULIN = "INSULIN";

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

        protected override void TombstoneModel()
        {
            var state = StateProvider.State;
            state[INSULIN] = modelCopy.Serialize(string.Empty);
        }

        protected override void UntombstoneModel()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(INSULIN))
            {
                var stateValue = (string)state[INSULIN];
                var untombstoned = stateValue.Deserialize<Insulin>(string.Empty);
                if (untombstoned.Id == modelCopy.Id)
                {
                    modelCopy.CopyFrom(untombstoned);
                }
            }
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
