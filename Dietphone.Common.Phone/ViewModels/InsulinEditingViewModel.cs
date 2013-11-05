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
        public bool InsulinHeaderCalculatedVisible { get; private set; }
        public string InsulinHeaderCalculatedText { get; private set; }
        private List<InsulinCircumstanceViewModel> addedCircumstances = new List<InsulinCircumstanceViewModel>();
        private List<InsulinCircumstanceViewModel> deletedCircumstances = new List<InsulinCircumstanceViewModel>();
        private bool isBusy;
        private readonly ReplacementBuilderAndSugarEstimatorFacade facade;

        public InsulinEditingViewModel(Factories factories, ReplacementBuilderAndSugarEstimatorFacade facade)
            : base(factories)
        {
            this.facade = facade;
            this.InsulinHeaderCalculatedText = string.Empty;
        }

        public string NameOfFirstChoosenCircumstance
        {
            get
            {
                return Subject.Circumstances.Any() ? Subject.Circumstances.First().Name : string.Empty;
            }
            set
            {
                if (!Subject.Circumstances.Any())
                    throw new InvalidOperationException("No insulin circumstance choosen.");
                Subject.Circumstances.First().Name = value;
            }
        }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
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

        public CanDeleteCircumstanceResult CanDeleteCircumstance()
        {
            if (Circumstances.Count < 2)
                return CanDeleteCircumstanceResult.NoThereIsOnlyOneCircumstance;
            if (!Subject.Circumstances.Any())
                return CanDeleteCircumstanceResult.NoCircumstanceChoosen;
            return CanDeleteCircumstanceResult.Yes;
        }

        public void DeleteCircumstance()
        {
            var toDelete = Subject.Circumstances.First();
            var choosenViewModels = Subject.Circumstances.ToList();
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

        public void InvalidateCircumstances()
        {
            var newCircumstances = new ObservableCollection<InsulinCircumstanceViewModel>();
            foreach (var circumstance in Circumstances)
            {
                var newCircumstance = new InsulinCircumstanceViewModel(circumstance.Model, factories);
                newCircumstance.MakeBuffer();
                newCircumstance.CopyFrom(circumstance);
                newCircumstances.Add(newCircumstance);
            }
            Circumstances = newCircumstances;
            Subject.InvalidateCircumstances(Circumstances);
            OnPropertyChanged("Circumstances");
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
            MakeSugarViewModel();
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

        private void MakeSugarViewModel()
        {
            var sugar = finder.FindSugarBeforeInsulin(modelSource);
            if (sugar == null)
                sugar = factories.CreateSugar();
            CurrentSugar = new SugarViewModel(sugar, factories);
        }

        public enum CanDeleteCircumstanceResult { Yes, NoCircumstanceChoosen, NoThereIsOnlyOneCircumstance };
    }
}
