using System;
using System.Collections.Generic;
using System.Linq;

namespace Dietphone.Models
{
    public interface Finder
    {
        Meal FindMealById(Guid mealId);
        MealName FindMealNameById(Guid mealNameId);
        Product FindProductById(Guid productId);
        Category FindCategoryById(Guid categoryId);
        InsulinCircumstance FindInsulinCircumstanceById(Guid insulinCircumstanceId);
        List<Product> FindProductsByCategory(Guid categoryId);
        List<Product> FindProductsAddedByUser();
        Category FindCategoryFirstAlphabetically();
        Meal FindMealByInsulin(Insulin insulin);
        Insulin FindInsulinByMeal(Meal meal);
        Sugar FindSugarBeforeInsulin(Insulin insulin);
    }

    public sealed class FinderImpl : Finder
    {
        private readonly Factories factories;

        public FinderImpl(Factories factories)
        {
            this.factories = factories;
        }

        public Meal FindMealById(Guid mealId)
        {
            var meals = factories.Meals;
            return meals.FindById(mealId);
        }

        public MealName FindMealNameById(Guid mealNameId)
        {
            var mealNames = factories.MealNames;
            return mealNames.FindById(mealNameId);
        }

        public Product FindProductById(Guid productId)
        {
            var products = factories.Products;
            return products.FindById(productId);
        }

        public Category FindCategoryById(Guid categoryId)
        {
            var categories = factories.Categories;
            return categories.FindById(categoryId);
        }

        public InsulinCircumstance FindInsulinCircumstanceById(Guid insulinCircumstanceId)
        {
            var insulinCircumstances = factories.InsulinCircumstances;
            return insulinCircumstances.FindById(insulinCircumstanceId);
        }

        public List<Product> FindProductsByCategory(Guid categoryId)
        {
            var result = from product in factories.Products
                         where product.CategoryId == categoryId
                         select product;
            return result.ToList();
        }

        public List<Product> FindProductsAddedByUser()
        {
            var result = from product in factories.Products
                         where product.AddedByUser
                         select product;
            return result.ToList();
        }

        public Category FindCategoryFirstAlphabetically()
        {
            var categories = factories.Categories;
            var sortedCategories = categories.OrderBy(category => category.Name);
            return sortedCategories.FirstOrDefault();
        }

        public Meal FindMealByInsulin(Insulin insulin)
        {
            var meals = factories.Meals;
            var earliest = insulin.DateTime.AddHours(-1);
            var latest = insulin.DateTime.AddHours(1);
            var candidates = meals.Where(m => m.DateTime >= earliest && m.DateTime <= latest).ToList();
            if (candidates.Count > 1)
                return candidates.OrderBy(m => Math.Abs((m.DateTime - insulin.DateTime).Ticks)).FirstOrDefault();
            else
                return candidates.FirstOrDefault();
        }

        public Insulin FindInsulinByMeal(Meal meal)
        {
            var insulins = factories.Insulins;
            var earliest = meal.DateTime.AddHours(-1);
            var latest = meal.DateTime.AddHours(1);
            var candidates = insulins.Where(i => i.DateTime >= earliest && i.DateTime <= latest).ToList();
            if (candidates.Count > 1)
                return candidates.OrderBy(i => Math.Abs((i.DateTime - meal.DateTime).Ticks)).FirstOrDefault();
            else
                return candidates.FirstOrDefault();
        }

        public Sugar FindSugarBeforeInsulin(Insulin insulin)
        {
            throw new NotImplementedException();
        }
    }

    public static class FinderExtensions
    {
        public static T FindById<T>(this IEnumerable<T> items, Guid itemId) where T : HasId
        {
            var result = from item in items
                         where item.Id == itemId
                         select item;
            return result.FirstOrDefault();
        }
    }

    public interface HasId
    {
        Guid Id { get; }
    }
}