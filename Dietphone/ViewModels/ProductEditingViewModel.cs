﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Dietphone.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public class ProductEditingViewModel : ViewModelBase
    {
        public ObservableCollection<CategoryViewModel> Categories { get; private set; }
        public ProductViewModel Product { get; private set; }
        public event EventHandler GotDirty;
        public event EventHandler<CannotSaveEventArgs> CannotSave;
        private Factories factories;
        private Finder finder;
        private Navigator navigator;
        private Product modelCopy;
        private Product modelSource;
        private List<CategoryViewModel> addedCategories = new List<CategoryViewModel>();
        private List<CategoryViewModel> deletedCategories = new List<CategoryViewModel>();

        public ProductEditingViewModel(Factories factories, Navigator navigator)
        {
            this.factories = factories;
            this.finder = factories.Finder;
            this.navigator = navigator;
            FindAndCopyModel();
            if (modelCopy == null)
            {
                navigator.GoBack();
            }
            else
            {
                CreateProductViewModel();
                LoadCategories();
            }
        }

        public string CategoryName
        {
            get
            {
                var category = Product.Category;
                return category.Name;
            }
            set
            {
                var category = Product.Category;
                category.Name = value;
            }
        }

        public List<string> AllServingSizeUnits
        {
            get
            {
                return UnitAbbreviations.GetAll();
            }
        }

        public void AddAndSetCategory(string name)
        {
            var tempModel = factories.CreateCategory();
            var models = factories.Categories;
            models.Remove(tempModel);
            var viewModel = new CategoryViewModel(tempModel);
            viewModel.Name = name;
            Categories.Add(viewModel);
            Product.Category = viewModel;
            addedCategories.Add(viewModel);
        }

        public bool CanDeleteCategory()
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

        public void DeleteCategory()
        {
            var toDelete = Product.Category;
            var newIndex = Categories.IndexOf(toDelete) + 1;
            if (newIndex > Categories.Count - 1)
            {
                newIndex -= 2;
            }
            Product.Category = Categories[newIndex];
            Categories.Remove(toDelete);
            deletedCategories.Add(toDelete);
        }

        public bool CanSave()
        {
            var validation = modelCopy.Validate();
            if (!string.IsNullOrEmpty(validation))
            {
                var args = new CannotSaveEventArgs();
                args.Reason = validation;
                OnCannotSave(args);
                return args.Ignore;
            }
            return true;
        }

        public void SaveAndReturn()
        {
            modelSource.CopyFrom(modelCopy);
            SaveCategories();
            navigator.GoBack();
        }

        public void CancelAndReturn()
        {
            navigator.GoBack();
        }

        public void DeleteAndReturn()
        {
            var models = factories.Products;
            models.Remove(modelSource);
            navigator.GoBack();
        }

        private void FindAndCopyModel()
        {
            var id = navigator.GetPassedProductId();
            modelSource = finder.FindProductById(id);
            if (modelSource != null)
            {
                modelCopy = modelSource.GetCopy();
                modelCopy.Owner = factories;
            }
        }

        private void CreateProductViewModel()
        {
            var maxCuAndFpu = new MaxCuAndFpuInCategories(finder, modelCopy);
            Product = new ProductViewModel(modelCopy, maxCuAndFpu);
            Product.PropertyChanged += delegate { OnGotDirty(); };
        }

        private void LoadCategories()
        {
            var loader = new ProductListingViewModel.CategoriesAndProductsLoader(factories);
            Categories = loader.GetCategoriesReloaded();
            Product.Categories = Categories;
            foreach (var category in Categories)
            {
                category.MakeBuffer();
            }
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

        protected void OnGotDirty()
        {
            if (GotDirty != null)
            {
                GotDirty(this, EventArgs.Empty);
            }
        }

        protected void OnCannotSave(CannotSaveEventArgs e)
        {
            if (CannotSave != null)
            {
                CannotSave(this, e);
            }
        }
    }

    public class CannotSaveEventArgs : EventArgs
    {
        public string Reason { get; set; }
        public bool Ignore { get; set; }
    }
}
