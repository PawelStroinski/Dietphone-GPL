using System;
using System.Collections.Generic;
using System.Linq;

namespace Dietphone.Models
{
    public interface ReplacementBuilderAndSugarEstimatorFacade
    {
        ReplacementAndEstimatedSugars GetReplacementAndEstimatedSugars(Meal meal, Insulin insulin,
            Sugar currentBefore);
    }

    public class ReplacementBuilderAndSugarEstimatorFacadeImpl : ReplacementBuilderAndSugarEstimatorFacade
    {
        private readonly PatternBuilder patternBuilder;
        private readonly ReplacementBuilder replacementBuilder;
        private readonly SugarEstimator sugarEstimator;

        public ReplacementBuilderAndSugarEstimatorFacadeImpl(PatternBuilder patternBuilder,
            ReplacementBuilder replacementBuilder, SugarEstimator sugarEstimator)
        {
            this.patternBuilder = patternBuilder;
            this.replacementBuilder = replacementBuilder;
            this.sugarEstimator = sugarEstimator;
        }

        public ReplacementAndEstimatedSugars GetReplacementAndEstimatedSugars(Meal meal, Insulin insulin,
            Sugar currentBefore)
        {
            var patterns = patternBuilder.GetPatternsFor(insulin, meal);
            var replacement = replacementBuilder.GetReplacementFor(meal, patterns);
            var estimatedSugars = sugarEstimator.GetEstimatedSugarsAfter(meal, currentBefore,
                usingReplacementItems: replacement.Items);
            return new ReplacementAndEstimatedSugars
            {
                Replacement = replacement,
                EstimatedSugars = estimatedSugars
            };
        }
    }


    public class ReplacementAndEstimatedSugars
    {
        public Replacement Replacement { get; set; }
        public IList<Sugar> EstimatedSugars { get; set; }
    }
}
