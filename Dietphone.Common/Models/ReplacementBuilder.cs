using System.Collections.Generic;
using System.Linq;
using System;

namespace Dietphone.Models
{
    public interface ReplacementBuilder
    {
        Replacement GetReplacementFor(Meal meal, IList<Pattern> usingPatterns);
    }

    public class ReplacementBuilderImpl : ReplacementBuilder
    {
        private const byte MAX_NOT_REPLACED_PERCENT_OF_ENERGY = 5;
        private readonly IEnumerable<IAction> actions;
        private Meal meal;
        private IList<Pattern> patterns;
        private Replacement replacement;

        public ReplacementBuilderImpl(params IAction[] actions)
        {
            this.actions = actions;
        }

        public Replacement GetReplacementFor(Meal meal, IList<Pattern> usingPatterns)
        {
            this.meal = meal;
            patterns = usingPatterns;
            replacement = new Replacement();
            CheckPatterns();
            BuildItems();
            DoActions();
            return replacement;
        }

        private void CheckPatterns()
        {
            foreach (var pattern in patterns)
                if (pattern.For == null)
                    throw new ArgumentException("Pattern.For cannot be null.");
        }

        private void BuildItems()
        {
            replacement.Items = new List<ReplacementItem>();
            foreach (var patternsFor in patterns.GroupBy(p => p.For))
            {
                var top = patternsFor.OrderByDescending(p => p.RightnessPoints).First();
                var replacementItem = new ReplacementItem { Pattern = top };
                replacement.Items.Add(replacementItem);
            }
        }

        private void DoActions()
        {
            foreach (var action in actions)
                action.Do(this);
        }

        public interface IAction
        {
            void Do(ReplacementBuilderImpl replacementBuilder);
        }

        public class IsComplete : IAction
        {
            public void Do(ReplacementBuilderImpl replacementBuilder)
            {
                short energySum = replacementBuilder.meal.Energy;
                float replacementEnergySum = replacementBuilder.replacement.Items
                    .Sum(replacement => replacement.Pattern.Match.Energy * replacement.Pattern.Factor);
                float notReplacedEnergy = Math.Abs(energySum - replacementEnergySum);
                double notReplacedEnergyPercent = notReplacedEnergy / energySum * 100;
                replacementBuilder.replacement.IsComplete
                    = notReplacedEnergyPercent <= MAX_NOT_REPLACED_PERCENT_OF_ENERGY;
            }
        }

        public class InsulinTotal : IAction
        {
            public void Do(ReplacementBuilderImpl replacementBuilder)
            {
                var insulinTotal = new Insulin();
                var patterns = replacementBuilder.replacement.Items.Select(item => item.Pattern);
                foreach (var pattern in patterns)
                {
                    var meal = pattern.From;
                    var totalCuInMeal = meal.Items.Sum(item => item.Cu);
                    var percentOfCu = totalCuInMeal == 0 ? 0 : pattern.Match.Cu / totalCuInMeal;
                    insulinTotal.NormalBolus
                        += (float)Math.Round(percentOfCu * pattern.Insulin.NormalBolus * pattern.Factor, 1);
                    var totalFpuInMeal = meal.Items.Sum(item => item.Fpu);
                    var percentOfFpu = totalFpuInMeal == 0 ? 0 : pattern.Match.Fpu / totalFpuInMeal;
                    insulinTotal.SquareWaveBolus
                        += (float)Math.Round(percentOfFpu * pattern.Insulin.SquareWaveBolus * pattern.Factor, 1);
                    insulinTotal.SquareWaveBolusHours
                        += (float)Math.Round(percentOfFpu * pattern.Insulin.SquareWaveBolusHours * pattern.Factor, 1);
                }
                replacementBuilder.replacement.InsulinTotal = insulinTotal;
            }
        }
    }
}
