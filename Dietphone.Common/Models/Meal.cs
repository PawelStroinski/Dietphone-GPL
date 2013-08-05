﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Dietphone.Tools;
using System.Xml.Serialization;
using Dietphone.Views;

namespace Dietphone.Models
{
    public class Meal : EntityWithId
    {
        public DateTime DateTime { get; set; }
        public Guid NameId { get; set; }
        public string Note { get; set; }
        protected List<MealItem> items;

        [XmlIgnore]
        public ReadOnlyCollection<MealItem> Items
        {
            get
            {
                if (items == null)
                {
                    throw new InvalidOperationException("Call InitializeItems first.");
                }
                return items.AsReadOnly();
            }
        }

        public short Energy
        {
            get
            {
                var energySum = Items.Sum(item => item.Energy);
                return (short)energySum;
            }
        }

        public float Protein
        {
            get
            {
                var proteinSum = Items.Sum(item => item.Protein);
                return proteinSum;
            }
        }

        public float Fat
        {
            get
            {
                var fatSum = Items.Sum(item => item.Fat);
                return fatSum;
            }
        }

        public float DigestibleCarbs
        {
            get
            {
                var digestibleCarbsSum = Items.Sum(item => item.DigestibleCarbs);
                return digestibleCarbsSum;
            }
        }

        public float Cu
        {
            get
            {
                var digestibleCarbsSum = Items.Sum(item => item.DigestibleCarbs);
                var calculator = new Calculator()
                {
                    DigestibleCarbs = digestibleCarbsSum
                };
                return calculator.Cu;
            }
        }

        public float Fpu
        {
            get
            {
                var proteinSum = Items.Sum(item => item.Protein);
                var fatSum = Items.Sum(item => item.Fat);
                var calculator = new Calculator()
                {
                    Protein = proteinSum,
                    Fat = fatSum
                };
                return calculator.Fpu;
            }
        }

        public void InitializeItems(List<MealItem> newItems)
        {
            var alreadyInitialized = items != null;
            if (alreadyInitialized)
            {
                throw new InvalidOperationException("Items can only be initialized once.");
            }
            items = newItems;
        }

        public void CopyItemsFrom(Meal source)
        {
            InternalCopyItemsFrom(source);
            AssignOwner();
        }

        public MealItem AddItem()
        {
            var item = Owner.CreateMealItem();
            items.Add(item);
            return item;
        }

        public void DeleteItem(MealItem item)
        {
            items.Remove(item);
        }

        public string Validate()
        {
            return ValidateItems();
        }

        public ReadOnlyCollection<MealItem> NormalizedItems()
        {
            var groupedItems = items
                .GroupBy(g => new { g.ProductId, g.Unit })
                .ToList();
            if (groupedItems.Count == items.Count)
                return items.AsReadOnly();
            var normalizedItems = groupedItems
                .Select(m => new MealItem
                {
                    ProductId = m.First().ProductId,
                    Unit = m.First().Unit,
                    Value = m.Sum(M => M.Value)
                })
                .ToList();
            foreach (var item in normalizedItems)
                item.SetOwner(Owner);
            return normalizedItems.AsReadOnly();
        }

        protected void InternalCopyItemsFrom(Meal source)
        {
            var sourceItems = source.items;
            items = sourceItems.GetItemsCopy();
        }

        protected override void OnOwnerAssigned()
        {
            if (items != null)
            {
                AssignOwner();
            }
        }

        private string ValidateItems()
        {
            var validation = string.Empty;
            foreach (var item in Items)
            {
                var itemValidation = item.Validate();
                if (!string.IsNullOrEmpty(itemValidation))
                {
                    var itemNumber = Items.IndexOf(item) + 1;
                    var itemFormatted = string.Format(Translations.IngredientNoContainsFollowingErrors,
                        itemNumber, itemValidation);
                    validation += itemFormatted;
                }
            }
            return validation;
        }

        private void AssignOwner()
        {
            foreach (var item in items)
            {
                item.SetOwner(Owner);
            }
        }
    }
}
