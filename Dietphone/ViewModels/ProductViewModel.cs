﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Dietphone.Models;
using System.Collections.ObjectModel;
using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        public Product Product { get; private set; }
        public Collection<CategoryViewModel> Categories { private get; set; }
        private bool autoCalculatingEnergyPer100g;
        private bool autoCalculatingEnergyPerServing;
        private readonly MaxNutritivesInCategories maxNutritives;
        private static readonly Constrains max100g = new Constrains { Max = 100 };
        private static readonly Constrains big = new Constrains { Max = 10000 };
        private const byte RECT_WIDTH = 25;

        public ProductViewModel(Product product, MaxNutritivesInCategories maxNutritives)
        {
            Product = product;
            this.maxNutritives = maxNutritives;
            autoCalculatingEnergyPer100g = Product.EnergyPer100g == 0;
            autoCalculatingEnergyPerServing = Product.EnergyPerServing == 0;
        }

        public Guid Id
        {
            get
            {
                return Product.Id;
            }
        }

        public string Name
        {
            get
            {
                return Product.Name;
            }
            set
            {
                if (value != Product.Name)
                {
                    Product.Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public CategoryViewModel Category
        {
            get
            {
                if (Categories == null)
                {
                    throw new InvalidOperationException("Set Categories first.");
                }
                return GetCategory();
            }
            set
            {
                if (value != null)
                {
                    SetCategory(value);
                }
            }
        }

        public string ServingSizeValue
        {
            get
            {
                var result = Product.ServingSizeValue;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.ServingSizeValue;
                var newValue = oldValue.TryGetValueOf(value);
                Product.ServingSizeValue = big.Constraint(newValue);
                OnPropertyChanged("ServingSizeValue");
            }
        }

        public Unit ServingSizeUnit
        {
            get
            {
                return Product.ServingSizeUnit;
            }
            set
            {
                Product.ServingSizeUnit = value;
                OnPropertyChanged("ServingSizeUnit");
            }
        }

        public string ServingSizeDescription
        {
            get
            {
                return Product.ServingSizeDescription;
            }
            set
            {
                Product.ServingSizeDescription = value;
                OnPropertyChanged("ServingSizeDescription");
            }
        }

        public string EnergyPer100g
        {
            get
            {
                var result = Product.EnergyPer100g;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.EnergyPer100g;
                var newValue = oldValue.TryGetValueOf(value);
                Product.EnergyPer100g = big.Constraint(newValue);
                autoCalculatingEnergyPer100g = false;
                OnPropertyChanged("EnergyPer100g");
            }
        }

        public string EnergyPerServing
        {
            get
            {
                var result = Product.EnergyPerServing;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.EnergyPerServing;
                var newValue = oldValue.TryGetValueOf(value);
                Product.EnergyPerServing = big.Constraint(newValue);
                autoCalculatingEnergyPerServing = false;
                OnPropertyChanged("EnergyPerServing");
            }
        }

        public string ProteinPer100g
        {
            get
            {
                var result = Product.ProteinPer100g;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.ProteinPer100g;
                var newValue = oldValue.TryGetValueOf(value);
                Product.ProteinPer100g = max100g.Constraint(newValue);
                InvalidateMaxNutritivesAndUnits();
                AutoCalculateEnergyPer100g();
                OnPropertyChanged("ProteinPer100g");
            }
        }

        public string ProteinPerServing
        {
            get
            {
                var result = Product.ProteinPerServing;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.ProteinPerServing;
                var newValue = oldValue.TryGetValueOf(value);
                Product.ProteinPerServing = big.Constraint(newValue);
                AutoCalculateEnergyPerServing();
                OnPropertyChanged("ProteinPerServing");
            }
        }

        public string FatPer100g
        {
            get
            {
                var result = Product.FatPer100g;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.FatPer100g;
                var newValue = oldValue.TryGetValueOf(value);
                Product.FatPer100g = max100g.Constraint(newValue);
                InvalidateMaxNutritivesAndUnits();
                AutoCalculateEnergyPer100g();
                OnPropertyChanged("FatPer100g");
            }
        }

        public string FatPerServing
        {
            get
            {
                var result = Product.FatPerServing;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.FatPerServing;
                var newValue = oldValue.TryGetValueOf(value);
                Product.FatPerServing = big.Constraint(newValue);
                AutoCalculateEnergyPerServing();
                OnPropertyChanged("FatPerServing");
            }
        }

        public string CarbsTotalPer100g
        {
            get
            {
                var result = Product.CarbsTotalPer100g;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.CarbsTotalPer100g;
                var newValue = oldValue.TryGetValueOf(value);
                Product.CarbsTotalPer100g = max100g.Constraint(newValue);
                InvalidateMaxNutritivesAndUnits();
                AutoCalculateEnergyPer100g();
                OnPropertyChanged("CarbsTotalPer100g");
                OnPropertyChanged("DigestibleCarbsPer100g");
            }
        }

        public string CarbsTotalPerServing
        {
            get
            {
                var result = Product.CarbsTotalPerServing;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.CarbsTotalPerServing;
                var newValue = oldValue.TryGetValueOf(value);
                Product.CarbsTotalPerServing = big.Constraint(newValue);
                AutoCalculateEnergyPerServing();
                OnPropertyChanged("CarbsTotalPerServing");
            }
        }

        public string FiberPer100g
        {
            get
            {
                var result = Product.FiberPer100g;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.FiberPer100g;
                var newValue = oldValue.TryGetValueOf(value);
                Product.FiberPer100g = max100g.Constraint(newValue);
                InvalidateMaxNutritivesAndUnits();
                AutoCalculateEnergyPer100g();
                OnPropertyChanged("FiberPer100g");
                OnPropertyChanged("DigestibleCarbsPer100g");
            }
        }

        public string FiberPerServing
        {
            get
            {
                var result = Product.FiberPerServing;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Product.FiberPerServing;
                var newValue = oldValue.TryGetValueOf(value);
                Product.FiberPerServing = big.Constraint(newValue);
                AutoCalculateEnergyPerServing();
                OnPropertyChanged("FiberPerServing");
            }
        }

        public string DigestibleCarbsPer100g
        {
            get
            {
                var result = Product.DigestibleCarbsPer100g;
                return result.ToStringOrEmpty();
            }
        }

        public string CuPer100g
        {
            get
            {
                var result = Product.CuPer100g;
                return string.Format("{0} WW", result);
            }
        }

        public string FpuPer100g
        {
            get
            {
                var result = Product.FpuPer100g;
                return string.Format("{0} WBT", result);
            }
        }

        public byte WidthOfFilledCuRect
        {
            get
            {
                var nutritives = maxNutritives.Get(Product.CategoryId);
                return GetWidthOfFilledRect(Product.CuPer100g, nutritives.CuPer100g);
            }
        }

        public byte WidthOfEmptyCuRect
        {
            get
            {
                return (byte)(RECT_WIDTH - WidthOfFilledCuRect);
            }
        }

        public byte WidthOfFilledFpuRect
        {
            get
            {
                var nutritives = maxNutritives.Get(Product.CategoryId);
                return GetWidthOfFilledRect(Product.FpuPer100g, nutritives.FpuPer100g);
            }
        }

        public byte WidthOfEmptyFpuRect
        {
            get
            {
                return (byte)(RECT_WIDTH - WidthOfFilledFpuRect);
            }
        }

        public byte DoubledWidthOfFilledCuRect
        {
            get
            {
                return (byte)(WidthOfFilledCuRect * 2);
            }
        }

        public byte DoubledWidthOfEmptyCuRect
        {
            get
            {
                return (byte)(WidthOfEmptyCuRect * 2);
            }
        }

        public byte DoubledWidthOfFilledFpuRect
        {
            get
            {
                return (byte)(WidthOfFilledFpuRect * 2);
            }
        }

        public byte DoubledWidthOfEmptyFpuRect
        {
            get
            {
                return (byte)(WidthOfEmptyFpuRect * 2);
            }
        }

        private byte GetWidthOfFilledRect(float value, float maxValue)
        {
            if (maxValue == 0)
                return 0;
            var multiplier = value / maxValue;
            var width = multiplier * RECT_WIDTH;
            var roundedWidth = (byte)Math.Round(width);
            return roundedWidth;
        }

        private CategoryViewModel GetCategory()
        {
            var result = from viewModel in Categories
                         where viewModel.Id == Product.CategoryId
                         select viewModel;
            return result.FirstOrDefault();
        }

        private void SetCategory(CategoryViewModel value)
        {
            var oldCategory = Product.CategoryId;
            Product.CategoryId = value.Id;
            maxNutritives.ResetCategory(oldCategory);
            InvalidateMaxNutritivesAndUnits();
            OnPropertyChanged("Category");
        }

        private void InvalidateMaxNutritivesAndUnits()
        {
            maxNutritives.ResetCategory(Product.CategoryId);
            OnPropertyChanged("WidthOfFilledCuRect");
            OnPropertyChanged("WidthOfEmptyCuRect");
            OnPropertyChanged("WidthOfFilledFpuRect");
            OnPropertyChanged("WidthOfEmptyFpuRect");
            OnPropertyChanged("DoubledWidthOfFilledCuRect");
            OnPropertyChanged("DoubledWidthOfEmptyCuRect");
            OnPropertyChanged("DoubledWidthOfFilledFpuRect");
            OnPropertyChanged("DoubledWidthOfEmptyFpuRect");
            OnPropertyChanged("CuPer100g");
            OnPropertyChanged("FpuPer100g");
        }

        private void AutoCalculateEnergyPer100g()
        {
            if (autoCalculatingEnergyPer100g)
            {
                var result = Product.CalculatedEnergyPer100g;
                EnergyPer100g = result.ToString();
                autoCalculatingEnergyPer100g = true;
            }
        }

        private void AutoCalculateEnergyPerServing()
        {
            if (autoCalculatingEnergyPerServing)
            {
                var result = Product.CalculatedEnergyPerServing;
                EnergyPerServing = result.ToString();
                autoCalculatingEnergyPerServing = true;
            }
        }
    }

    public class MaxNutritivesInCategories
    {
        private Finder finder;
        private Product replacement;
        private Dictionary<Guid, Nutritives> nutritives = new Dictionary<Guid, Nutritives>();
        private Guid categoryId;
        private List<Product> productsInCategory;

        public MaxNutritivesInCategories(Finder finder)
        {
            this.finder = finder;
        }

        public MaxNutritivesInCategories(Finder finder, Product replacement)
        {
            this.finder = finder;
            this.replacement = replacement;
        }

        public void Reset()
        {
            nutritives.Clear();
        }

        public void ResetCategory(Guid categoryId)
        {
            if (nutritives.ContainsKey(categoryId))
            {
                nutritives.Remove(categoryId);
            }
        }

        public Nutritives Get(Guid categoryId)
        {
            Nutritives result;
            if (nutritives.TryGetValue(categoryId, out result))
            {
                return result;
            }
            else
            {
                this.categoryId = categoryId;
                result = CalculateCategory();
                nutritives.Add(categoryId, result);
                return result;
            }
        }

        private Nutritives CalculateCategory()
        {
            productsInCategory = finder.FindProductsByCategory(categoryId);
            ReplaceWithReplacement();
            var cus = from product in productsInCategory
                      select product.CuPer100g;
            var fpus = from product in productsInCategory
                       select product.FpuPer100g;
            return new Nutritives() { CuPer100g = cus.Max(), FpuPer100g = fpus.Max() };
        }

        private void ReplaceWithReplacement()
        {
            if (replacement != null)
            {
                DeleteReplaced();
                AddReplacement();
            }
        }

        private void DeleteReplaced()
        {
            var replaced = productsInCategory.FindById(replacement.Id);
            if (replaced != null)
            {
                productsInCategory.Remove(replaced);
            }
        }

        private void AddReplacement()
        {
            if (replacement.CategoryId == categoryId)
            {
                productsInCategory.Add(replacement);
            }
        }
    }

    public class Nutritives
    {
        public float CuPer100g { get; set; }
        public float FpuPer100g { get; set; }
    }
}
