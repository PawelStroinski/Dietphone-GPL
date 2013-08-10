using System;
using System.Collections.Generic;
using System.Linq;

namespace Dietphone.Models
{
    public interface PatternBuilder
    {
        IList<Pattern> GetPatternsFor(Insulin insulin, Meal meal, IList<MealItem> normalizedItems);
    }

    public class PatternBuilderImpl : PatternBuilder
    {
        private const byte MAX_PERCENT_OF_ENERGY_DIFF = 10;
        private const byte POINTS_FOR_SAME_CIRCUMSTANCE = 5;
        private readonly Factories factories;
        private readonly IEnumerable<IVisitor> visitors;
        private Finder finder;
        private Settings settings;
        private Insulin searchedInsulin, insulin;
        private Meal searchedMeal, meal;
        private IList<MealItem> searchedItems;
        private MealItem searchedItem, item;
        private Sugar sugarBefore;
        private List<Sugar> sugarsAfter;
        private int percentOfEnergyDiff;
        private Pattern pattern;

        public PatternBuilderImpl(Factories factories, params IVisitor[] visitors)
        {
            this.factories = factories;
            this.visitors = visitors;
        }

        public IList<Pattern> GetPatternsFor(Insulin insulin, Meal meal, IList<MealItem> normalizedItems)
        {
            var patterns = new List<Pattern>();
            finder = factories.Finder;
            settings = factories.Settings;
            searchedInsulin = insulin;
            searchedMeal = meal;
            searchedItems = normalizedItems;
            foreach (var testMeal in factories.Meals.Where(m => m != searchedMeal))
            {
                var mealHasMatch = testMeal.Items.Any(item =>
                    searchedItems.Any(searchedItem =>
                        item.ProductId == searchedItem.ProductId && item.Unit == searchedItem.Unit));
                if (mealHasMatch)
                    foreach (var item in testMeal.NormalizedItems())
                        foreach (var searchedItem in searchedItems)
                            if (item.ProductId == searchedItem.ProductId && item.Unit == searchedItem.Unit)
                            {
                                this.searchedItem = searchedItem;
                                this.item = item;
                                this.meal = testMeal;
                                if (ConsiderPattern())
                                    patterns.Add(BuildPattern());
                            }
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
            pattern = new Pattern
            {
                Match = item,
                From = meal,
                Insulin = insulin,
                Before = sugarBefore,
                After = sugarsAfter,
                For = searchedItem,
                Factor = item.Value == 0 ? 0 : searchedItem.Value / item.Value
            };
            AcceptVisitors();
            return pattern;
        }

        private void AcceptVisitors()
        {
            foreach (var visitor in visitors)
                visitor.Visit(this);
        }

        public interface IVisitor
        {
            void Visit(PatternBuilderImpl patternBuilder);
        }

        public abstract class VisitorPoints : IVisitor
        {
            public void Visit(PatternBuilderImpl patternBuilder)
            {
                patternBuilder.pattern.RightnessPoints += Points(patternBuilder);
            }

            protected abstract byte Points(PatternBuilderImpl patternBuilder);
        }

        public class PointsForPercentOfEnergy : VisitorPoints
        {
            protected override byte Points(PatternBuilderImpl patternBuilder)
            {
                return (byte)(MAX_PERCENT_OF_ENERGY_DIFF - patternBuilder.percentOfEnergyDiff);
            }
        }

        public class PointsForRecentMeal : VisitorPoints
        {
            protected override byte Points(PatternBuilderImpl patternBuilder)
            {
                return Points(patternBuilder.searchedMeal.DateTime, patternBuilder.meal.DateTime);
            }

            private byte Points(DateTime left, DateTime right)
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

        public class PointsForSimillarHour : VisitorPoints
        {
            private readonly HourDifference hourDifference;

            public PointsForSimillarHour(HourDifference hourDifference)
            {
                this.hourDifference = hourDifference;
            }

            protected override byte Points(PatternBuilderImpl patternBuilder)
            {
                return Points(patternBuilder.searchedMeal.DateTime, patternBuilder.meal.DateTime);
            }

            private byte Points(DateTime left, DateTime right)
            {
                var difference = hourDifference.GetDifference(left.TimeOfDay, right.TimeOfDay);
                var rightnessPoints = 12 - difference;
                return (byte)rightnessPoints;
            }
        }

        public class PointsForSameCircumstances : VisitorPoints
        {
            protected override byte Points(PatternBuilderImpl patternBuilder)
            {
                return Points(patternBuilder.searchedInsulin, patternBuilder.insulin);
            }

            private byte Points(Insulin left, Insulin right)
            {
                var leftCircumstances = left.ReadCircumstances();
                var rightCircumstances = right.ReadCircumstances();
                var sameCircumstances = leftCircumstances.Intersect(rightCircumstances).Count();
                return (byte)(sameCircumstances * POINTS_FOR_SAME_CIRCUMSTANCE);
            }
        }

        public class PointsForSimillarSugarBefore : VisitorPoints
        {
            private const byte MAX_POINTS_FOR_SIMILLAR_SUGAR_BEFORE = POINTS_FOR_SAME_CIRCUMSTANCE;

            protected override byte Points(PatternBuilderImpl patternBuilder)
            {
                var searchedSugarBefore = patternBuilder.finder.FindSugarBeforeInsulin(patternBuilder.searchedInsulin);
                if (searchedSugarBefore == null)
                    return 0;
                var left = searchedSugarBefore.BloodSugarInMgdL;
                var right = patternBuilder.sugarBefore.BloodSugarInMgdL;
                return Points(left, right);
            }

            private byte Points(float left, float right)
            {
                var diff = Math.Abs(left - right);
                var roundedDiffDividedByTen = (int)Math.Round(diff / 10, MidpointRounding.AwayFromZero);
                var rightnessPoints = MAX_POINTS_FOR_SIMILLAR_SUGAR_BEFORE - roundedDiffDividedByTen;
                rightnessPoints = Math.Max(0, rightnessPoints);
                return (byte)rightnessPoints;
            }
        }

        public class PointsForFactorCloserToOne : VisitorPoints
        {
            private const byte MAX_POINTS_FOR_FACTOR_CLOSER_TO_ONE = POINTS_FOR_SAME_CIRCUMSTANCE;

            protected override byte Points(PatternBuilderImpl patternBuilder)
            {
                return Points(patternBuilder.pattern.Factor);
            }

            private byte Points(float factor)
            {
                if (factor > 1)
                    factor = 1 / factor;
                var rightnessPoints = (float)MAX_POINTS_FOR_FACTOR_CLOSER_TO_ONE * factor;
                rightnessPoints = (float)Math.Round(rightnessPoints);
                return (byte)rightnessPoints;
            }
        }
    }
}
