using Dietphone.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Dietphone.Tools;
using System.Linq;
using System;
using Dietphone.Views;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;

namespace Dietphone.ViewModels
{
    public class ProductEditingViewModel : EditingViewModelBase<Product, ProductViewModel>
    {
        public ObservableCollection<CategoryViewModel> Categories { get; private set; }
        public event EventHandler BeforeAddingEditingCategory;
        public event EventHandler AfterAddedEditedCategory;
        public event EventHandler<Action> CategoryDelete;
        private Navigation navigation;
        private List<CategoryViewModel> addedCategories = new List<CategoryViewModel>();
        private List<CategoryViewModel> deletedCategories = new List<CategoryViewModel>();
        private readonly BackgroundWorkerFactory workerFactory;
        private readonly LearningCuAndFpu learningCuAndFpu;
        private const string PRODUCT = "PRODUCT";
        private const string CATEGORIES = "CATEGORIES";

        public ProductEditingViewModel(Factories factories, BackgroundWorkerFactory workerFactory,
            MessageDialog messageDialog, LearningCuAndFpu learningCuAndFpu)
            : base(factories, messageDialog)
        {
            this.workerFactory = workerFactory;
            this.learningCuAndFpu = learningCuAndFpu;
        }

        public bool ShouldFocusName => string.IsNullOrEmpty(Subject.Name);

        public string CategoryName
        {
            get
            {
                var category = Subject.Category;
                return category.Name;
            }
            set
            {
                var category = Subject.Category;
                category.Name = value;
            }
        }

        public List<string> AllServingSizeUnits
        {
            get
            {
                return UnitAbbreviations.GetAbbreviationsFiltered(unit => unit != Unit.ServingSize);
            }
        }

        public void Init(Navigation navigation)
        {
            this.navigation = navigation;
        }

        public ICommand AddCategory
        {
            get
            {
                return new MvxCommand(() =>
                {
                    OnBeforeAddingEditingCategory(EventArgs.Empty);
                    var name = messageDialog.Input(Translations.Name, Translations.AddCategory);
                    if (!string.IsNullOrEmpty(name))
                    {
                        AddAndSetCategory(name);
                        OnAfterAddedEditedCategory(EventArgs.Empty);
                    }
                });
            }
        }

        public ICommand EditCategory
        {
            get
            {
                return new MvxCommand(() =>
                {
                    OnBeforeAddingEditingCategory(EventArgs.Empty);
                    var newName = messageDialog.Input(Translations.Name, Translations.EditCategory, value: CategoryName);
                    if (!string.IsNullOrEmpty(newName))
                    {
                        CategoryName = newName;
                        OnAfterAddedEditedCategory(EventArgs.Empty);
                    }
                });
            }
        }

        public ICommand DeleteCategory
        {
            get
            {
                return new MvxCommand(() =>
                {
                    if (!CanDeleteCategory())
                    {
                        messageDialog.Show(Translations.ThisCategoryIncludesOtherProducts, Translations.CannotDelete);
                        return;
                    }
                    if (messageDialog.Confirm(string.Format(
                        Translations.AreYouSureYouWantToPermanentlyDeleteThisCategory, CategoryName),
                        Translations.DeleteCategory))
                    {
                        OnCategoryDelete(() => DeleteCategoryDo());
                    }
                });
            }
        }

        public ICommand LearnCu
        {
            get
            {
                return new MvxCommand(() =>
                {
                    learningCuAndFpu.LearnCu();
                });
            }
        }

        public ICommand LearnFpu
        {
            get
            {
                return new MvxCommand(() =>
                {
                    learningCuAndFpu.LearnFpu();
                });
            }
        }

        protected override void DoSaveAndReturn()
        {
            modelSource.CopyFrom(modelCopy);
            SaveCategories();
            Navigator.GoBack();
        }

        public void DeleteAndSaveAndReturn()
        {
            if (messageDialog.Confirm(
                string.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisProduct,
                Subject.Name),
                Translations.DeleteProduct))
            {
                DeleteAndSaveAndReturnDo();
            }
        }

        protected override void FindAndCopyModel()
        {
            var id = navigation.ProductIdToEdit;
            modelSource = finder.FindProductById(id);
            if (modelSource != null)
            {
                modelCopy = modelSource.GetCopy();
                modelCopy.SetOwner(factories);
            }
        }

        protected override void MakeViewModel()
        {
            LoadCategories();
            UntombstoneCategories();
            MakeProductViewModelInternal();
        }

        protected override string Validate()
        {
            return modelCopy.Validate();
        }

        private void LoadCategories()
        {
            var loader = new ProductListingViewModel.CategoriesAndProductsLoader(factories, workerFactory);
            Categories = loader.Categories;
            foreach (var category in Categories)
            {
                category.MakeBuffer();
            }
        }

        protected override void TombstoneModel()
        {
            var state = StateProvider.State;
            state[PRODUCT] = modelCopy.Serialize(string.Empty);
        }

        protected override void UntombstoneModel()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(PRODUCT))
            {
                var stateValue = (string)state[PRODUCT];
                var untombstoned = stateValue.Deserialize<Product>(string.Empty);
                if (untombstoned.Id == modelCopy.Id)
                {
                    modelCopy.CopyFrom(untombstoned);
                }
            }
        }

        protected override void TombstoneOtherThings()
        {
            TombstoneCategories();
        }

        internal override Messages Messages
        {
            get
            {
                return new Messages
                {
                    CannotSaveCaption = Translations.AreYouSureYouWantToSaveThisProduct
                };
            }
        }

        private void AddAndSetCategory(string name)
        {
            var tempModel = factories.CreateCategory();
            var models = factories.Categories;
            models.Remove(tempModel);
            var viewModel = new CategoryViewModel(tempModel, factories);
            viewModel.Name = name;
            Categories.Add(viewModel);
            Subject.Category = viewModel;
            addedCategories.Add(viewModel);
        }

        private bool CanDeleteCategory()
        {
            var categoryId = modelCopy.CategoryId;
            var productsInCategory = finder.FindProductsByCategory(categoryId);
            bool otherProductsInCategory;
            if (productsInCategory.Count == 0)
            {
                otherProductsInCategory = false;
            }
            else
                if (productsInCategory.Count == 1)
            {
                otherProductsInCategory = productsInCategory[0] != modelSource;
            }
            else
            {
                otherProductsInCategory = true;
            }
            return !otherProductsInCategory && Categories.Count > 1;
        }

        private void DeleteCategoryDo()
        {
            var toDelete = Subject.Category;
            Subject.Category = Categories.GetNextItemToSelectWhenDeleteSelected(toDelete);
            Categories.Remove(toDelete);
            deletedCategories.Add(toDelete);
        }

        private void DeleteAndSaveAndReturnDo()
        {
            var models = factories.Products;
            models.Remove(modelSource);
            SaveCategories();
            Navigator.GoBack();
        }

        private void TombstoneCategories()
        {
            var categories = new List<Category>();
            foreach (var category in Categories)
            {
                category.AddModelTo(categories);
            }
            var state = StateProvider.State;
            state[CATEGORIES] = categories;
        }

        private void UntombstoneCategories()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(CATEGORIES))
            {
                var untombstoned = (List<Category>)state[CATEGORIES];
                addedCategories.Clear();
                var notUntombstoned = from category in Categories
                                      where untombstoned.FindById(category.Id) == null
                                      select category;
                deletedCategories = notUntombstoned.ToList();
                foreach (var deletedCategory in deletedCategories)
                {
                    Categories.Remove(deletedCategory);
                }
                foreach (var model in untombstoned)
                {
                    var existingViewModel = Categories.FindById(model.Id);
                    if (existingViewModel != null)
                    {
                        existingViewModel.CopyFromModel(model);
                    }
                    else
                    {
                        var addedViewModel = new CategoryViewModel(model, factories);
                        Categories.Add(addedViewModel);
                        addedCategories.Add(addedViewModel);
                    }
                }
            }
        }

        private void MakeProductViewModelInternal()
        {
            var maxCuAndFpu = new MaxCuAndFpuInCategories(finder, modelCopy);
            Subject = new ProductViewModel(modelCopy)
            {
                Categories = Categories,
                MaxCuAndFpu = maxCuAndFpu
            };
            Subject.PropertyChanged += delegate
            {
                IsDirty = true;
            };
        }

        private void SaveCategories()
        {
            foreach (var category in Categories)
            {
                category.FlushBuffer();
            }
            var models = factories.Categories;
            foreach (var category in addedCategories)
            {
                models.Add(category.Model);
            }
            foreach (var category in deletedCategories)
            {
                models.Remove(category.Model);
            }
        }

        protected void OnBeforeAddingEditingCategory(EventArgs e)
        {
            if (BeforeAddingEditingCategory != null)
            {
                BeforeAddingEditingCategory(this, e);
            }
        }

        protected void OnAfterAddedEditedCategory(EventArgs e)
        {
            if (AfterAddedEditedCategory != null)
            {
                AfterAddedEditedCategory(this, e);
            }
        }

        protected void OnCategoryDelete(Action action)
        {
            if (CategoryDelete == null)
                action();
            else
                CategoryDelete(this, action);
        }

        public class Navigation
        {
            public Guid ProductIdToEdit { get; set; }
        }
    }
}
