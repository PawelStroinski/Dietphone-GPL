using System.Collections.Generic;
using System;
using Dietphone.Tools;
using System.Linq;

namespace Dietphone.Models
{
    public interface Factories
    {
        StorageCreator StorageCreator { set; }
        Finder Finder { get; }
        DefaultEntities DefaultEntities { get; }
        List<Meal> Meals { get; }
        List<MealName> MealNames { get; }
        List<Product> Products { get; }
        List<Category> Categories { get; }
        List<Sugar> Sugars { get; }
        List<Insulin> Insulins { get; }
        List<InsulinCircumstance> InsulinCircumstances { get; }
        Settings Settings { get; }
        MruProducts MruProducts { get; }

        Meal CreateMeal();
        MealName CreateMealName();
        MealItem CreateMealItem();
        Product CreateProduct();
        Category CreateCategory();
        Sugar CreateSugar();
        Insulin CreateInsulin();
        InsulinCircumstance CreateInsulinCircumstance();
        void Save();
    }

    public sealed class FactoriesImpl : Factories
    {
        public Finder Finder { get; private set; }
        public DefaultEntities DefaultEntities { get; private set; }
        private MruProducts mruProducts;
        private Factory<Meal> mealFactory;
        private Factory<MealName> mealNameFactory;
        private Factory<Product> productFactory;
        private Factory<Category> categoryFactory;
        private Factory<Sugar> sugarFactory;
        private Factory<Insulin> insulinFactory;
        private Factory<InsulinCircumstance> insulinCircumstanceFactory;
        private Factory<Settings> settingsFactory;
        private readonly FactoryCreator factoryCreator;
        private readonly object mruProductsLock = new object();
        private readonly object mealFactoryLock = new object();
        private readonly object mealNameFactoryLock = new object();
        private readonly object productFactoryLock = new object();
        private readonly object categoryFactoryLock = new object();
        private readonly object sugarFactoryLock = new object();
        private readonly object insulinFactoryLock = new object();
        private readonly object insulinCircumstanceFactoryLock = new object();
        private readonly object settingsFactoryLock = new object();

        public FactoriesImpl()
        {
            factoryCreator = new FactoryCreator(this);
            Finder = new FinderImpl(this);
            DefaultEntities = new DefaultEntitiesImpl(this);
        }

        public StorageCreator StorageCreator
        {
            set
            {
                factoryCreator.StorageCreator = value;
            }
        }

        public List<Meal> Meals
        {
            get
            {
                return MealFactory.Entities;
            }
        }

        public List<MealName> MealNames
        {
            get
            {
                return MealNameFactory.Entities;
            }
        }

        public List<Product> Products
        {
            get
            {
                return ProductFactory.Entities;
            }
        }

        public List<Category> Categories
        {
            get
            {
                return CategoryFactory.Entities;
            }
        }

        public List<Sugar> Sugars
        {
            get
            {
                return SugarFactory.Entities;
            }
        }

        public List<Insulin> Insulins
        {
            get
            {
                return InsulinFactory.Entities;
            }
        }

        public List<InsulinCircumstance> InsulinCircumstances
        {
            get
            {
                return InsulinCircumstanceFactory.Entities;
            }
        }

        public Settings Settings
        {
            get
            {
                var entities = SettingsFactory.Entities;
                return entities.First();
            }
        }

        public MruProducts MruProducts
        {
            get
            {
                lock (mruProductsLock)
                {
                    if (mruProducts == null)
                    {
                        mruProducts = new MruProducts(Settings.MruProductIds, this);
                    }
                    return mruProducts;
                }
            }
        }

        public Meal CreateMeal() // TODO: Those Create* methods (class-specific lines) should be in individual classes
        {
            var meal = MealFactory.CreateEntity();
            meal.Id = Guid.NewGuid();
            meal.DateTime = DateTime.UtcNow;
            var items = new List<MealItem>();
            meal.InitializeItems(items);
            meal.SetNullStringPropertiesToEmpty();
            return meal;
        }

        public MealName CreateMealName()
        {
            var mealName = MealNameFactory.CreateEntity();
            mealName.Id = Guid.NewGuid();
            mealName.SetNullStringPropertiesToEmpty();
            return mealName;
        }

        public MealItem CreateMealItem()
        {
            var mealItem = new MealItem();
            mealItem.SetOwner(this);
            return mealItem;
        }

        public Product CreateProduct()
        {
            var product = ProductFactory.CreateEntity();
            product.Id = Guid.NewGuid();
            var defaultCategory = Finder.FindCategoryFirstAlphabetically();
            product.CategoryId = defaultCategory.Id;
            product.SetNullStringPropertiesToEmpty();
            return product;
        }

        public Category CreateCategory()
        {
            var category = CategoryFactory.CreateEntity();
            category.Id = Guid.NewGuid();
            category.SetNullStringPropertiesToEmpty();
            return category;
        }

        public Sugar CreateSugar()
        {
            var sugar = SugarFactory.CreateEntity();
            sugar.Id = Guid.NewGuid();
            sugar.DateTime = DateTime.UtcNow;
            return sugar;
        }

        public Insulin CreateInsulin()
        {
            var insulin = InsulinFactory.CreateEntity();
            insulin.Id = Guid.NewGuid();
            insulin.DateTime = DateTime.UtcNow;
            var circumstances = new List<Guid>();
            insulin.InitializeCircumstances(circumstances);
            insulin.SetNullStringPropertiesToEmpty();
            return insulin;
        }

        public InsulinCircumstance CreateInsulinCircumstance()
        {
            var circumstance = InsulinCircumstanceFactory.CreateEntity();
            circumstance.Id = Guid.NewGuid();
            circumstance.SetNullStringPropertiesToEmpty();
            return circumstance;
        }

        public void Save()
        {
            MealFactory.Save();
            MealNameFactory.Save();
            ProductFactory.Save();
            CategoryFactory.Save();
            SugarFactory.Save();
            InsulinFactory.Save();
            InsulinCircumstanceFactory.Save();
            SettingsFactory.Save();
        }

        private Factory<Meal> MealFactory
        {
            get
            {
                lock (mealFactoryLock)
                {
                    if (mealFactory == null)
                    {
                        mealFactory = factoryCreator.CreateFactory<Meal>();
                    }
                    return mealFactory;
                }
            }
        }

        private Factory<MealName> MealNameFactory
        {
            get
            {
                lock (mealNameFactoryLock)
                {
                    if (mealNameFactory == null)
                    {
                        mealNameFactory = factoryCreator.CreateFactory<MealName>();
                    }
                    return mealNameFactory;
                }
            }
        }

        private Factory<Product> ProductFactory
        {
            get
            {
                lock (productFactoryLock)
                {
                    if (productFactory == null)
                    {
                        productFactory = factoryCreator.CreateFactory<Product>();
                    }
                    return productFactory;
                }
            }
        }

        private Factory<Category> CategoryFactory
        {
            get
            {
                lock (categoryFactoryLock)
                {
                    if (categoryFactory == null)
                    {
                        categoryFactory = factoryCreator.CreateFactory<Category>();
                    }
                    return categoryFactory;
                }
            }
        }

        private Factory<Sugar> SugarFactory
        {
            get
            {
                lock (sugarFactoryLock)
                {
                    if (sugarFactory == null)
                    {
                        sugarFactory = factoryCreator.CreateFactory<Sugar>();
                    }
                    return sugarFactory;
                }
            }
        }

        private Factory<Insulin> InsulinFactory
        {
            get
            {
                lock (insulinFactoryLock)
                {
                    if (insulinFactory == null)
                    {
                        insulinFactory = factoryCreator.CreateFactory<Insulin>();
                    }
                    return insulinFactory;
                }
            }
        }

        private Factory<InsulinCircumstance> InsulinCircumstanceFactory
        {
            get
            {
                lock (insulinCircumstanceFactoryLock)
                {
                    if (insulinCircumstanceFactory == null)
                    {
                        insulinCircumstanceFactory = factoryCreator.CreateFactory<InsulinCircumstance>();
                    }
                    return insulinCircumstanceFactory;
                }
            }
        }

        private Factory<Settings> SettingsFactory
        {
            get
            {
                lock (settingsFactoryLock)
                {
                    if (settingsFactory == null)
                    {
                        settingsFactory = factoryCreator.CreateFactory<Settings>();
                    }
                    return settingsFactory;
                }
            }
        }
    }
}