using Dietphone.Models;
using Dietphone.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dietphone.ViewModels
{
    public class InsulinEditingViewModel : EditingViewModelBase<Insulin>
    {
        public InsulinViewModel Insulin { get; private set; }
        public SugarViewModel CurrentSugar { get; private set; }
        //private IList<InsulinCircumstanceViewModel> circumstances;
        //private readonly object circumstancesLock = new object();

        public InsulinEditingViewModel(Factories factories)
            : base(factories)
        {
        }

        //public IList<InsulinCircumstanceViewModel> Circumstances
        //{
        //    get
        //    {
        //        lock (circumstancesLock)
        //        {
        //            if (circumstances == null)
        //            {
        //                var model = factories.InsulinCircumstances;
        //                circumstances = model.Select(circumstance => new InsulinCircumstanceViewModel(
        //                    circumstance, factories)).ToList();
        //            }
        //            return circumstances;
        //        }
        //    }
        //}

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
                modelCopy.InitializeCircumstances(modelSource.ReadCircumstances().ToList());
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
