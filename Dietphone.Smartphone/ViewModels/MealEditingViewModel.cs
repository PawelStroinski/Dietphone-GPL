using Dietphone.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Dietphone.Tools;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using Dietphone.Views;

namespace Dietphone.ViewModels
{
    public class MealEditingViewModel : EditingViewModelWithDate<Meal, MealViewModel>
    {
        public bool NeedsScrollingItemsDown { get; set; }
        public ObservableCollection<MealNameViewModel> Names { get; private set; }
        public MealItemEditingViewModel ItemEditing { get { return itemEditing; } }
        public event EventHandler InvalidateItems;
        public event EventHandler BeforeAddingEditingName;
        public event EventHandler AfterAddedEditedName;
        public event EventHandler<Action> NameDelete;
        private Navigation navigation;
        private List<MealNameViewModel> addedNames = new List<MealNameViewModel>();
        private List<MealNameViewModel> deletedNames = new List<MealNameViewModel>();
        private MealNameViewModel defaultName;
        private MealItemViewModel editItem;
        private bool wentToSettings;
        private bool setIsDirtyWhenReady;
        private readonly BackgroundWorkerFactory workerFactory;
        private readonly TrialViewModel trial;
        private readonly BackNavigation backNavigation;
        private readonly MealItemEditingViewModel itemEditing;
        private const string MEAL = "MEAL";
        private const string NAMES = "NAMES";
        private const string ITEM_EDITING = "EDIT_ITEM";
        private const string EDIT_ITEM_INDEX = "EDIT_ITEM_INDEX";

        public MealEditingViewModel(Factories factories, BackgroundWorkerFactory workerFactory, TrialViewModel trial,
            BackNavigation backNavigation, MealItemEditingViewModel itemEditing, MessageDialog messageDialog)
            : base(factories, messageDialog)
        {
            this.workerFactory = workerFactory;
            this.trial = trial;
            this.backNavigation = backNavigation;
            this.itemEditing = itemEditing;
            OnItemEditingChanged();
        }

        public string NameOfName
        {
            get
            {
                var name = Subject.Name;
                return name.Name;
            }
            set
            {
                var name = Subject.Name;
                name.Name = value;
            }
        }

        public string IdentifiableName
        {
            get
            {
                return string.Format("{0}, {1}", NameOfName, Subject.DateAndTime);
            }
        }

        public void Init(Navigation navigation)
        {
            this.navigation = navigation;
        }

        public ICommand AddName
        {
            get
            {
                return new MvxCommand(() =>
                {
                    OnBeforeAddingEditingName(EventArgs.Empty);
                    var name = messageDialog.Input(Translations.Name, Translations.AddName);
                    if (!string.IsNullOrEmpty(name))
                    {
                        AddAndSetName(name);
                        OnAfterAddedEditedName(EventArgs.Empty);
                    }
                });
            }
        }

        public ICommand EditName
        {
            get
            {
                return new MvxCommand(() =>
                {
                    if (!CanEditName())
                    {
                        messageDialog.Show(NameOfName, Translations.CannotEditThisName);
                        return;
                    }
                    OnBeforeAddingEditingName(EventArgs.Empty);
                    var newName = messageDialog.Input(Translations.Name, Translations.EditName, value: NameOfName);
                    if (string.IsNullOrEmpty(newName))
                        return;
                    NameOfName = newName;
                    OnAfterAddedEditedName(EventArgs.Empty);
                });
            }
        }

        public ICommand DeleteName
        {
            get
            {
                return new MvxCommand(() =>
                {
                    if (!CanDeleteName())
                    {
                        messageDialog.Show(NameOfName, Translations.CannotDeleteThisName);
                        return;
                    }
                    if (messageDialog.Confirm(string.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisName,
                        NameOfName), Translations.DeleteName))
                    {
                        OnNameDelete(() => DeleteNameDo());
                    }
                });
            }
        }

        protected override void DoSaveAndReturn()
        {
            SaveWithUpdatedTime();
            Navigator.GoBack();
        }

        public void GoToInsulinEditing()
        {
            SaveWithUpdatedTime();
            IsDirty = false;
            var insulin = finder.FindInsulinByMeal(modelSource);
            if (insulin == null)
                Navigator.GoToNewInsulinRelatedToMeal(modelSource.Id);
            else
                Navigator.GoToInsulinEditingRelatedToMeal(insulin.Id, modelSource.Id);
        }

        public void DeleteAndSaveAndReturn()
        {
            if (messageDialog.Confirm(
                string.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisMeal,
                IdentifiableName),
                Translations.DeleteMeal))
            {
                DeleteAndSaveAndReturnDo();
            }
        }

        public ICommand AddItem
        {
            get
            {
                return new MvxCommand(() =>
                {
                    trial.Run();
                    Navigator.GoToMainToAddMealItem();
                });
            }
        }

        public void EditItem(MealItemViewModel itemViewModel)
        {
            if (itemViewModel != null)
            {
                editItem = itemViewModel;
                editItem.MakeBuffer();
                itemEditing.Show(editItem);
            }
        }

        public ICommand OpenScoresSettings
        {
            get
            {
                return new MvxCommand(() =>
                {
                    wentToSettings = true;
                    Navigator.GoToSettings();
                });
            }
        }

        public void ReturnedFromNavigation()
        {
            if (wentToSettings)
            {
                wentToSettings = false;
                OnPropertyChanged(string.Empty);
                Subject.Scores.Invalidate();
                OnInvalidateItems(EventArgs.Empty);
            }
            else
            {
                AddCopyOfItem();
            }
        }

        public void UiRendered()
        {
            UntombstoneItemEditing();
        }

        protected override void FindAndCopyModel()
        {
            var id = navigation.MealIdToEdit;
            modelSource = finder.FindMealById(id);
            if (modelSource != null)
            {
                modelCopy = modelSource.GetCopy();
                modelCopy.SetOwner(factories);
                modelCopy.CopyItemsFrom(modelSource);
            }
        }

        protected override void OnModelReady()
        {
            AddCopyOfItem();
        }

        protected override void OnCommonUiReady()
        {
            if (setIsDirtyWhenReady)
            {
                IsDirty = true;
                setIsDirtyWhenReady = false;
            }
        }

        protected override void MakeViewModel()
        {
            LoadNames();
            UntombstoneNames();
            MakeMealViewModelInternal();
            base.MakeViewModel();
        }

        protected override string Validate()
        {
            return modelCopy.Validate();
        }

        protected void OnItemEditingChanged()
        {
            itemEditing.Confirmed += delegate
            {
                editItem.FlushBuffer();
                editItem.Invalidate();
            };
            itemEditing.Cancelled += delegate
            {
                editItem.ClearBuffer();
                editItem.Invalidate();
            };
            itemEditing.NeedToDelete += delegate
            {
                Subject.DeleteItem(editItem);
            };
            itemEditing.CanDelete = true;
            itemEditing.StateProvider = StateProvider;
        }

        private void AddAndSetName(string name)
        {
            var tempModel = factories.CreateMealName();
            var models = factories.MealNames;
            models.Remove(tempModel);
            var viewModel = new MealNameViewModel(tempModel, factories);
            viewModel.Name = name;
            Names.Add(viewModel);
            Subject.Name = viewModel;
            addedNames.Add(viewModel);
        }

        private bool CanEditName()
        {
            return Subject.Name != defaultName;
        }

        private bool CanDeleteName()
        {
            return Subject.Name != defaultName;
        }

        private void DeleteNameDo()
        {
            var toDelete = Subject.Name;
            Subject.Name = Names.GetNextItemToSelectWhenDeleteSelected(toDelete);
            Names.Remove(toDelete);
            deletedNames.Add(toDelete);
        }

        private void DeleteAndSaveAndReturnDo()
        {
            var models = factories.Meals;
            models.Remove(modelSource);
            SaveNames();
            Navigator.GoBack();
        }

        private void LoadNames()
        {
            var loader = new JournalViewModel.JournalLoader(factories, sortCircumstances: false, sortNames: true,
                workerFactory: workerFactory);
            Names = loader.Names;
            foreach (var mealName in Names)
            {
                mealName.MakeBuffer();
            }
            defaultName = loader.DefaultName;
            Names.Insert(0, defaultName);
        }

        protected override void TombstoneModel()
        {
            var state = StateProvider.State;
            var dto = DTOFactory.MealToDTO(modelCopy);
            state[MEAL] = dto.Serialize(string.Empty);
        }

        protected override void UntombstoneModel()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(MEAL))
            {
                var dtoState = (string)state[MEAL];
                var dto = dtoState.Deserialize<MealDTO>(string.Empty);
                if (dto.Id == modelCopy.Id)
                    DTOReader.DTOToMeal(dto, modelCopy);
            }
        }

        protected override void TombstoneOtherThings()
        {
            base.TombstoneOtherThings();
            TombstoneNames();
            TombstoneItemEditing();
        }

        internal override Messages Messages
        {
            get
            {
                return new Messages
                {
                    CannotSaveCaption = Translations.AreYouSureYouWantToSaveThisMeal
                };
            }
        }

        private void TombstoneNames()
        {
            var names = new List<MealName>();
            foreach (var name in Names)
            {
                name.AddModelTo(names);
            }
            var state = StateProvider.State;
            state[NAMES] = names;
        }

        private void UntombstoneNames()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(NAMES))
            {
                var untombstoned = (List<MealName>)state[NAMES];
                addedNames.Clear();
                var notUntombstoned = from name in Names
                                      where untombstoned.FindById(name.Id) == null
                                      select name;
                deletedNames = notUntombstoned.ToList();
                foreach (var deletedName in deletedNames)
                {
                    Names.Remove(deletedName);
                }
                foreach (var model in untombstoned)
                {
                    var existingViewModel = Names.FindById(model.Id);
                    if (existingViewModel != null)
                    {
                        existingViewModel.CopyFromModel(model);
                    }
                    else
                    {
                        var addedViewModel = new MealNameViewModel(model, factories);
                        Names.Add(addedViewModel);
                        addedNames.Add(addedViewModel);
                    }
                }
            }
        }

        private void TombstoneItemEditing()
        {
            var state = StateProvider.State;
            state[ITEM_EDITING] = itemEditing.IsVisible;
            if (itemEditing.IsVisible)
            {
                var items = Subject.Items;
                var editItemIndex = items.IndexOf(editItem);
                state[EDIT_ITEM_INDEX] = editItemIndex;
                itemEditing.Tombstone();
            }
        }

        private void UntombstoneItemEditing()
        {
            var state = StateProvider.State;
            var itemEditing = false;
            if (state.ContainsKey(ITEM_EDITING))
            {
                itemEditing = (bool)state[ITEM_EDITING];
            }
            if (itemEditing)
            {
                var editItemIndex = (int)state[EDIT_ITEM_INDEX];
                var items = Subject.Items;
                if (editItemIndex > -1 && editItemIndex < items.Count)
                {
                    var item = items[editItemIndex];
                    EditItem(item);
                }
            }
        }

        private void MakeMealViewModelInternal()
        {
            Subject = new MealViewModel(modelCopy, factories)
            {
                Names = Names,
                DefaultName = defaultName
            };
        }

        private void SaveWithUpdatedTime()
        {
            UpdateLockedDateTime();
            modelSource.CopyFrom(modelCopy);
            modelSource.CopyItemsFrom(modelCopy);
            SaveNames();
        }

        private void SaveNames()
        {
            foreach (var viewModel in Names)
            {
                viewModel.FlushBuffer();
            }
            var models = factories.MealNames;
            foreach (var viewModel in addedNames)
            {
                models.Add(viewModel.Model);
            }
            foreach (var viewModel in deletedNames)
            {
                models.Remove(viewModel.Model);
            }
        }

        private void AddCopyOfItem()
        {
            var addCopyOfThisItem = backNavigation.AddCopyOfThisItem;
            if (addCopyOfThisItem != null)
            {
                if (Subject == null)
                {
                    var model = modelCopy.AddItem();
                    model.CopyFrom(addCopyOfThisItem);
                    setIsDirtyWhenReady = true;
                }
                else
                {
                    var viewModel = Subject.AddItem();
                    viewModel.CopyFromModel(addCopyOfThisItem);
                }
                var mruProducts = factories.MruProducts;
                mruProducts.AddProduct(addCopyOfThisItem.Product);
                backNavigation.AddCopyOfThisItem = null;
                NeedsScrollingItemsDown = true;
            }
        }

        protected void OnInvalidateItems(EventArgs e)
        {
            if (InvalidateItems != null)
            {
                InvalidateItems(this, e);
            }
        }

        protected void OnBeforeAddingEditingName(EventArgs e)
        {
            if (BeforeAddingEditingName != null)
            {
                BeforeAddingEditingName(this, e);
            }
        }

        protected void OnAfterAddedEditedName(EventArgs e)
        {
            if (AfterAddedEditedName != null)
            {
                AfterAddedEditedName(this, e);
            }
        }

        protected void OnNameDelete(Action action)
        {
            if (NameDelete == null)
                action();
            else
                NameDelete(this, action);
        }

        public class Navigation
        {
            public Guid MealIdToEdit { get; set; }
        }

        public class BackNavigation
        {
            public MealItem AddCopyOfThisItem { get; set; }
        }
    }
}
