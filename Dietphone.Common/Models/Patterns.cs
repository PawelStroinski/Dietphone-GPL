using System;
using System.Collections.Generic;
using System.Linq;

namespace Dietphone.Models
{
    public interface Patterns
    {
        IList<Pattern> GetPatternsFor(Insulin insulin);
    }

    public class PatternsImpl : Patterns
    {
        private const byte MAX_PERCENT_OF_ENERGY_DIFF = 10;
        private readonly Factories factories;

        public PatternsImpl(Factories factories)
        {
            this.factories = factories;
        }

        public IList<Pattern> GetPatternsFor(Insulin insulin)
        {
            var patterns = new List<Pattern>();
            var finder = factories.Finder;
            var settings = factories.Settings;
            var searchedMeal = finder.FindMealByInsulin(insulin);
            foreach (var meal in factories.Meals.Where(m => m != searchedMeal))
                foreach (var item in meal.Items)
                    foreach (var searchedItem in searchedMeal.Items)
                        if (item.ProductId == searchedItem.ProductId)
                        {
                            var itemPercentOfEnergy = item.PercentOfEnergyInMeal(meal);
                            var searchedItemPercentOfEnergy = searchedItem.PercentOfEnergyInMeal(searchedMeal);
                            var percentOfEnergyDiff = Math.Abs(itemPercentOfEnergy - searchedItemPercentOfEnergy);
                            if (percentOfEnergyDiff > MAX_PERCENT_OF_ENERGY_DIFF)
                                continue;
                            var foundInsulin = finder.FindInsulinByMeal(meal);
                            if (foundInsulin == null)
                                continue;
                            var before = finder.FindSugarBeforeInsulin(foundInsulin);
                            if (before == null)
                                continue;
                            var after = finder.FindSugarsAfterInsulin(foundInsulin, settings.SugarsAfterInsulinHours);
                            if (!after.Any())
                                continue;
                            var pattern = new Pattern
                            {
                                Match = item,
                                From = meal,
                                Insulin = foundInsulin,
                                Before = before,
                                After = after
                            };
                            pattern.RightnessPoints += (byte)(MAX_PERCENT_OF_ENERGY_DIFF - percentOfEnergyDiff);
                            pattern.RightnessPoints += RecentMealsRightnessPoints(searchedMeal.DateTime, meal.DateTime);
                            patterns.Add(pattern);
                        }
            return patterns;
        }

        private byte RecentMealsRightnessPoints(DateTime left, DateTime right)
        {
            byte rightnessPoints = 0;
            var diff = left > right ? left - right : right - left;
            var daysDiffs = new int[] { 360, 180, 90, 60, 30, 15, 7, 2 };
            foreach (var daysDiff in daysDiffs)
                if (diff <= new TimeSpan(daysDiff, 0, 0, 0))
                    rightnessPoints++;
            return rightnessPoints;
        }
    }
}
