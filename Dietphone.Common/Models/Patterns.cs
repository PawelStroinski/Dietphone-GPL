﻿using System;
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
            var searchedMeal = factories.Finder.FindMealByInsulin(insulin);
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
                            var pattern = new Pattern { Match = item, From = meal };
                            pattern.RightnessPoints += (byte)(MAX_PERCENT_OF_ENERGY_DIFF - percentOfEnergyDiff);
                            patterns.Add(pattern);
                        }
            return patterns;
        }
    }
}