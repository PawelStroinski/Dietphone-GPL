using System;
using System.Collections.Generic;
using System.Linq;
using Dietphone.Models;
using NUnit.Framework;
using NSubstitute;

namespace Dietphone.Core.Tests.Models
{
    public class ReplacementBuilderAndSugarEstimatorFacadeTests
    {
        [Test]
        public void GetReplacementAndEstimatedSugars()
        {
            var patternBuilder = Substitute.For<PatternBuilder>();
            var replacementBuilder = Substitute.For<ReplacementBuilder>();
            var sugarEstimator = Substitute.For<SugarEstimator>();
            var sut = new ReplacementBuilderAndSugarEstimatorFacadeImpl(patternBuilder, replacementBuilder,
                sugarEstimator);
            var meal = new Meal();
            var insulin = new Insulin();
            var sugar = new Sugar();
            var patterns = new List<Pattern>();
            var replacement = new Replacement { Items = new List<ReplacementItem>() };
            var estimatedSugars = new List<Sugar>();
            patternBuilder.GetPatternsFor(insulin, meal, currentBefore: sugar).Returns(patterns);
            replacementBuilder.GetReplacementFor(meal, patterns).Returns(replacement);
            sugarEstimator.GetEstimatedSugarsAfter(meal, sugar, replacement.Items).Returns(estimatedSugars);
            var result = sut.GetReplacementAndEstimatedSugars(meal, insulin, sugar);
            Assert.AreSame(replacement, result.Replacement);
            Assert.AreSame(estimatedSugars, result.EstimatedSugars);
        }
    }
}
