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
        private const byte POINTS_FOR_SAME_CIRCUMSTANCE = 5;
        private readonly Factories factories;
        private readonly HourDifference hourDifference;
        private Finder finder;
        private Settings settings;
        private Insulin searchedInsulin, insulin;
        private Meal searchedMeal, meal;
        private MealItem searchedItem, item;
        private Sugar sugarBefore;
        private List<Sugar> sugarsAfter;
        private int percentOfEnergyDiff;

        public PatternsImpl(Factories factories, HourDifference hourDifference)
        {
            this.factories = factories;
            this.hourDifference = hourDifference;
        }

        public IList<Pattern> GetPatternsFor(Insulin insulin)
        {
            finder = factories.Finder;
            settings = factories.Settings;
            searchedInsulin = insulin;
            searchedMeal = finder.FindMealByInsulin(searchedInsulin);
            var patterns = new List<Pattern>();
            foreach (var meal in factories.Meals.Where(m => m != searchedMeal))
                foreach (var item in meal.Items)
                    foreach (var searchedItem in searchedMeal.Items)
                        if (item.ProductId == searchedItem.ProductId)
                        {
                            this.searchedItem = searchedItem;
                            this.item = item;
                            this.meal = meal;
                            if (ConsiderPattern())
                                patterns.Add(BuildPattern());
                        }
            return patterns;
        }

        private bool ConsiderPattern()
        {
            var itemPercentOfEnergy = item.PercentOfEnergyInMeal(meal);
            var searchedItemPercentOfEnergy = searchedItem.PercentOfEnergyInMeal(searchedMeal);
            percentOfEnergyDiff = Math.Abs(itemPercentOfEnergy - searchedItemPercentOfEnergy);
            if (percentOfEnergyDiff > MAX_PERCENT_OF_ENERGY_DIFF)
                return false;
            insulin = finder.FindInsulinByMeal(meal);
            if (insulin == null)
                return false;
            sugarBefore = finder.FindSugarBeforeInsulin(insulin);
            if (sugarBefore == null)
                return false;
            sugarsAfter = finder.FindSugarsAfterInsulin(insulin, settings.SugarsAfterInsulinHours);
            if (!sugarsAfter.Any())
                return false;
            return true;
        }

        private Pattern BuildPattern()
        {
            var pattern = new Pattern
            {
                Match = item,
                From = meal,
                Insulin = insulin,
                Before = sugarBefore,
                After = sugarsAfter
            };
            pattern.RightnessPoints += PercentOfEnergysRightnessPoints();
            pattern.RightnessPoints += RecentMealsRightnessPoints(searchedMeal.DateTime, meal.DateTime);
            pattern.RightnessPoints += SimillarHoursRightnessPoints(searchedMeal.DateTime, meal.DateTime);
            pattern.RightnessPoints += SameCircumstancesRightnessPoints(searchedInsulin, insulin);
            return pattern;
        }

        private byte PercentOfEnergysRightnessPoints()
        {
            return (byte)(MAX_PERCENT_OF_ENERGY_DIFF - percentOfEnergyDiff);
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

        private byte SimillarHoursRightnessPoints(DateTime left, DateTime right)
        {
            var difference = hourDifference.GetDifference(left.TimeOfDay, right.TimeOfDay);
            var rightnessPoints = 12 - difference;
            return (byte)rightnessPoints;
        }

        private byte SameCircumstancesRightnessPoints(Insulin left, Insulin right)
        {
            var leftCircumstances = left.ReadCircumstances();
            var rightCircumstances = right.ReadCircumstances();
            var sameCircumstances = leftCircumstances.Intersect(rightCircumstances).Count();
            return (byte)(sameCircumstances * POINTS_FOR_SAME_CIRCUMSTANCE);
        }
    }
}
