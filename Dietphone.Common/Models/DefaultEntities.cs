using System;
using Dietphone.Views;

namespace Dietphone.Models
{
    public interface DefaultEntities
    {
        MealName MealName { get; }
        Product Product { get; }
        InsulinCircumstance InsulinCircumstance { get; }
    }

    public sealed class DefaultEntitiesImpl : DefaultEntities
    {
        private MealName mealName;
        private Product product;
        private InsulinCircumstance insulinCircumstance;
        private readonly Factories owner;
        private readonly object mealNameLock = new object();
        private readonly object productLock = new object();
        private readonly object insulinCircumstanceLock = new object();

        public DefaultEntitiesImpl(Factories owner)
        {
            this.owner = owner;
        }

        public MealName MealName
        {
            get
            {
                lock (mealNameLock)
                {
                    if (mealName == null)
                    {
                        mealName = owner.CreateMealName();
                        var mealNames = owner.MealNames;
                        mealNames.Remove(mealName);
                        mealName.Id = Guid.Empty;
                        mealName.Name = Translations.NoName;
                    }
                    return mealName;
                }
            }
        }

        public Product Product
        {
            get
            {
                lock (productLock)
                {
                    if (product == null)
                    {
                        product = owner.CreateProduct();
                        var products = owner.Products;
                        products.Remove(product);
                        product.Id = Guid.Empty;
                        product.Name = Translations.NoProduct;
                    }
                    return product;
                }
            }
        }

        public InsulinCircumstance InsulinCircumstance
        {
            get
            {
                lock (insulinCircumstanceLock)
                {
                    if (insulinCircumstance == null)
                    {
                        insulinCircumstance = owner.CreateInsulinCircumstance();
                        var circumstances = owner.InsulinCircumstances;
                        circumstances.Remove(insulinCircumstance);
                        insulinCircumstance.Id = Guid.Empty;
                        insulinCircumstance.Name = Translations.UnknownInsulinCircumstance;
                    }
                    return insulinCircumstance;
                }
            }
        }
    }
}
