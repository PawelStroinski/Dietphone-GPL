using Dietphone.Models;
using Dietphone.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dietphone.ViewModels
{
    public class InsulinEditingViewModel : EditingViewModelWithDate<Insulin, InsulinViewModel>
    {
        public ObservableCollection<InsulinCircumstanceViewModel> Circumstances { get; private set; }
        public SugarViewModel CurrentSugar { get; private set; }
        private List<InsulinCircumstanceViewModel> addedCircumstances = new List<InsulinCircumstanceViewModel>();
        private List<InsulinCircumstanceViewModel> deletedCircumstances = new List<InsulinCircumstanceViewModel>();

        public InsulinEditingViewModel(Factories factories)
            : base(factories)
        {
        }

        public void AddCircumstance(string name)
        {
            var tempModel = factories.CreateInsulinCircumstance();
            var models = factories.InsulinCircumstances;
            models.Remove(tempModel);
            var viewModel = new InsulinCircumstanceViewModel(tempModel, factories);
            viewModel.Name = name;
            Circumstances.Add(viewModel);
            addedCircumstances.Add(viewModel);
        }

        public bool CanEditCircumstance()
        {
            return Subject.Circumstances.Any();
        }

        public bool CanDeleteCircumstance()
        {
            return Subject.Circumstances.Any();
        }

        public void DeleteCircumstance()
        {
            var toDelete = Subject.Circumstances.First();
            var choosenViewModels = Subject.Circumstances;
            choosenViewModels.Remove(toDelete);
            Subject.Circumstances = choosenViewModels;
            Circumstances.Remove(toDelete);
            deletedCircumstances.Add(toDelete);
        }

        public string SummaryForSelectedCircumstances()
        {
            return string.Join(", ",
                Subject.Circumstances.Select(circumstance => circumstance.Name));
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
                modelCopy.InitializeCircumstances(modelSource.ReadCircumstances().ToList());
            }
        }

        protected override void MakeViewModel()
        {
            LoadCircumstances();
            MakeInsulinViewModelInternal();
            CurrentSugar = new SugarViewModel(new Sugar(), factories);
        }

        protected override string Validate()
        {
            return string.Empty;
        }

        private void LoadCircumstances()
        {
            var loader = new InsulinListingViewModel.CircumstancesAndInsulinsLoader(factories, true);
            Circumstances = loader.Circumstances;
            foreach (var circumstance in Circumstances)
                circumstance.MakeBuffer();
        }

        private void MakeInsulinViewModelInternal()
        {
            Subject = new InsulinViewModel(modelCopy, factories, allCircumstances: Circumstances);
            Subject.PropertyChanged += delegate
            {
                IsDirty = true;
            };
        }
    }
}
