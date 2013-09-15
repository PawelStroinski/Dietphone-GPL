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
            throw new NotImplementedException();
        }

        protected override void MakeViewModel()
        {
            throw new NotImplementedException();
        }

        protected override string Validate()
        {
            throw new NotImplementedException();
        }

        protected override void TombstoneModel()
        {
            throw new NotImplementedException();
        }

        protected override void UntombstoneModel()
        {
            throw new NotImplementedException();
        }
    }
}
