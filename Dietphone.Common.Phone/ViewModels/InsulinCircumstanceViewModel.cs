using System;
using Dietphone.Models;
using System.Collections.Generic;
using Dietphone.Tools;
using System.Linq;

namespace Dietphone.ViewModels
{
    public class InsulinCircumstanceViewModel : ViewModelWithBufferAcc<InsulinCircumstance>, HasId
    {
        public InsulinCircumstanceViewModel(InsulinCircumstance model, Factories factories)
            : base(model, factories)
        {
        }

        public Guid Id
        {
            get
            {
                return Model.Id;
            }
        }

        public string Name
        {
            get
            {
                return BufferOrModel.Name;
            }
            set
            {
                BufferOrModel.Name = value;
                OnPropertyChanged("Name");
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
