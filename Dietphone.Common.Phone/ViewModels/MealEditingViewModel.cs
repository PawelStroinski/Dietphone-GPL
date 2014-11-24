using Dietphone.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Dietphone.Tools;
using System;
using System.Globalization;
using System.Linq;

namespace Dietphone.ViewModels
{
    public class MealEditingViewModel : EditingViewModelWithDate<Meal, MealViewModel>
    {
        public MealItem AddCopyOfThisItem { get; set; }
        public bool NeedsScrollingItemsDown { get; set; }
        public ObservableCollection<MealNameViewModel> Names { get; private set; }
        public event EventHandler InvalidateItems;
        private List<MealNameViewModel> addedNames = new List<MealNameViewModel>();
        private List<MealNameViewModel> deletedNames = new List<MealNameViewModel>();
        private MealNameViewModel defaultName;
        private MealItemEditingViewModel itemEditing;
        private MealItemViewModel editItem;
        private bool wentToSettings;
        private bool setIsDirtyWhenReady;
        private readonly BackgroundWorkerFactory workerFactory;
        private const string MEAL = "MEAL";
        private const string NAMES = "NAMES";
        private const string ITEM_EDITING = "EDIT_ITEM";
        private const string EDIT_ITEM_INDEX = "EDIT_ITEM_INDEX";

        public MealEditingViewModel(Factories factories, BackgroundWorkerFactory workerFactory)
            : base(factories)
        {
            this.workerFactory = workerFactory;
        }

        public MealItemEditingViewModel ItemEditing
        {
            private get
            {
                return itemEditing;
            }
            set
            {
                if (itemEditing != value)
                {
                    itemEditing = value;
                    OnItemEditingChanged();
                }
            }
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

        public void AddAndSetName(string name)
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

        public bool CanEditName()
        {
            return Subject.Name != defaultName;
        }

        public bool CanDeleteName()
        {
            return Subject.Name != defaultName;
        }

        public void DeleteName()
        {
            var toDelete = Subject.Name;
            Subject.Name = Names.GetNextItemToSelectWhenDeleteSelected(toDelete);
            Names.Remove(toDelete);
            deletedNames.Add(toDelete);
        }

        public void SaveWithUpdatedTimeAndReturn()
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
            var models = factories.Meals;
            models.Remove(modelSource);
            SaveNames();
            Navigator.GoBack();
        }

        public void AddItem()
        {
            Navigator.GoToMainToAddMealItem();
        }

        public void EditItem(MealItemViewModel itemViewModel)
        {
            if (itemViewModel != null)
            {
                editItem = itemViewModel;
                editItem.MakeBuffer();
                ItemEditing.Show(editItem);
            }
        }

        public void OpenScoresSettings()
        {
            wentToSettings = true;
            Navigator.GoToSettings();
        }

        public void ReturnedFromNavigation()
        {
            if (wentToSettings)
            {
                wentToSettings = false;
                OnPropertyChanged(string.Empty);
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
            var id = Navigator.GetMealIdToEdit();
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
            ItemEditing.Confirmed += delegate
            {
                editItem.FlushBuffer();
                editItem.Invalidate();
            };
            ItemEditing.Cancelled += delegate
            {
                editItem.ClearBuffer();
                editItem.Invalidate();
            };
            ItemEditing.NeedToDelete += delegate
            {
                Subject.DeleteItem(editItem);
            };
            ItemEditing.CanDelete = true;
            ItemEditing.StateProvider = StateProvider;
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
            var dto = new MealDTO();
            dto.CopyFrom(modelCopy);
            dto.DTOCopyItemsFrom(modelCopy);
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
                {
                    modelCopy.CopyFrom(dto);
                    modelCopy.CopyItemsFrom(dto);
                }
            }
        }

        protected override void TombstoneOtherThings()
        {
            base.TombstoneOtherThings();
            TombstoneNames();
            TombstoneItemEditing();
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
            state[ITEM_EDITING] = ItemEditing.IsVisible;
            if (ItemEditing.IsVisible)
            {
                var items = Subject.Items;
                var editItemIndex = items.IndexOf(editItem);
                state[EDIT_ITEM_INDEX] = editItemIndex;
                ItemEditing.Tombstone();
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
            if (AddCopyOfThisItem != null)
            {
                if (Subject == null)
                {
                    var model = modelCopy.AddItem();
                    model.CopyFrom(AddCopyOfThisItem);
                    setIsDirtyWhenReady = true;
                }
                else
                {
                    var viewModel = Subject.AddItem();
                    viewModel.CopyFromModel(AddCopyOfThisItem);
                }
                var mruProducts = factories.MruProducts;
                mruProducts.AddProduct(AddCopyOfThisItem.Product);
                AddCopyOfThisItem = null;
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
    }
}
