﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Globalization;
using Ploeh.AutoFixture;

namespace Dietphone.Models.Tests
{
    public class SugarEstimatorTests : ModelBasedTests
    {
    }

    public class SugarCollectorTests : ModelBasedTests
    {
        private readonly object dateLock = new object();

        [TestCase(new string[] { "12:20 | 12:00 100 12:40 110" }, "12:00", "12:00 | 12:00 100 12:40 110")]
        [TestCase(new string[] { "12:20 | 12:00 100 12:40 110" }, "12:00", "12:00 | 12:00 100", "12:00 | 12:40 110")]
        [TestCase(new string[] { "12:20 | 12:00 100 12:40 110" }, "12:00", "14:00 | 14:00 100", "14:00 | 14:40 110")]
        [TestCase(new string[] { "12:00 | 12:00 100", "14:30 | 14:30 110" }, "12:00", "12:00 | 12:00 100",
            "12:00 | 14:30 110")]
        [TestCase(new string[] { "10:05 | 10:00 100 10:10 120", "12:44 | 12:59 115 12:30 110" }, "10:00",
            "14:00 | 16:59 115 16:30 110", "12:00 | 12:00 100", "12:30 | 12:40 120")]
        public void ReturnsSugarsGrouppedByHours(string[] expectedKeyHourAndSugars, string meal,
            params string[] replacementMealsAndSugars)
        {
            var replacementItems = GetReplacementItems(replacementMealsAndSugars);
            var sut = new SugarCollector();
            var actual = sut.CollectByHour(AddMeal(meal), replacementItems);
            AssertExpectedSugarsEqualActual(expectedKeyHourAndSugars, actual);
        }

        [Test]
        public void ReturnsCopiesAndNotOriginalSugarsWhichRemainUnchanged()
        {
            var replacementItems = GetReplacementItems("12:00 | 12:00 100 12:40 110");
            var sugar = replacementItems[0].Pattern.After.First();
            var date = sugar.DateTime;
            var sut = new SugarCollector();
            var actual = sut.CollectByHour(AddMeal("12:00"), replacementItems);
            Assert.AreNotSame(sugar, actual.Values.First().First().Collected);
            Assert.AreEqual(date, sugar.DateTime);
        }

        [Test]
        public void ReturnsReplacementItems()
        {
            var replacementItems = GetReplacementItems("12:00 | 12:00 100 12:10 120", "12:00 | 12:20 120 12:30 110");
            var sut = new SugarCollector();
            var actual = sut.CollectByHour(AddMeal("12:00"), replacementItems).Values.First();
            Assert.AreEqual(100, actual.First().Collected.BloodSugar);
            Assert.AreEqual(110, actual.Last().Collected.BloodSugar);
            Assert.AreSame(replacementItems[0], actual.First().Source);
            Assert.AreSame(replacementItems[1], actual.Last().Source);
        }

        [Test]
        public void HandlesSugarsOnDifferentDateThanMeal()
        {
            var replacementItems = GetReplacementItems("23:00 | 02:00 100");
            var sugar = replacementItems[0].Pattern.After.Single();
            sugar.DateTime = sugar.DateTime.AddDays(1);
            var sut = new SugarCollector();
            var meal = AddMeal("22:15");
            var actual = sut.CollectByHour(meal, replacementItems).Values.First();
            Assert.AreEqual(meal.DateTime.AddHours(3), actual.Single().Collected.DateTime);
        }

        private List<ReplacementItem> GetReplacementItems(params string[] mealAndSugars)
        {
            var replacementItems = new List<ReplacementItem>();
            lock (dateLock)
            {
                var date = basedate;
                basedate = basedate.AddDays(1);
                foreach (var item in mealAndSugars)
                {
                    var splet = item.Split('|').Select(s => s.Trim()).ToList();
                    var pattern = new Pattern();
                    pattern.From = AddMeal(splet[0]);
                    pattern.After = AddSugars(splet[1]);
                    replacementItems.Add(new ReplacementItem { Pattern = pattern });
                }
                basedate = date;
            }
            return replacementItems;
        }

        private void AssertExpectedSugarsEqualActual(string[] expectedKeyHourAndSugars,
            Dictionary<TimeSpan, List<CollectedSugar>> actual)
        {
            Assert.AreEqual(expectedKeyHourAndSugars.Count(), actual.Count);
            foreach (var item in expectedKeyHourAndSugars)
            {
                var splet = item.Split('|').Select(s => s.Trim()).ToList();
                var actualSugars = actual[TimeSpan.Parse(splet[0])].Select(sugar => sugar.Collected);
                var expectedSugars = AddSugars(splet[1]);
                Assert.IsTrue(expectedSugars.SequenceEqual(actualSugars));
            }
        }
    }

    public class SugarRelatorTests
    {
        [TestCase(100, "120", "100 120 * 1")]
        [TestCase(100, "120", "100 140 * 0.5")]
        [TestCase(150, "125.16", "140 128 * 2.07")]
        [TestCase(90, "120 110", "80 140 * 0.5", "100 110 * 2")]
        [TestCase(5.1f, "5.25", "5.0 5.5 * 0.3")]
        public void ReturnsSugarChangeMultipliedByFactorAndAddedToCurrentSugar(float currentBefore,
            string expectedAfters, params string[] beforeAfterAndFactor)
        {
            var culture = new CultureInfo("en");
            var fixture = new Fixture();
            var collectedSugars = beforeAfterAndFactor
                .Select(toSplit => new
                {
                    Splet = toSplit.Split(new char[] { ' ', '*' }, StringSplitOptions.RemoveEmptyEntries)
                })
                .Select(splet => new CollectedSugar
                {
                    Collected = new Sugar
                    {
                        BloodSugar = float.Parse(splet.Splet[1], culture),
                        DateTime = fixture.Create<DateTime>()
                    },
                    Source = new ReplacementItem
                    {
                        Pattern = new Pattern
                        {
                            Before = new Sugar { BloodSugar = float.Parse(splet.Splet[0], culture) },
                            Factor = float.Parse(splet.Splet[2], culture)
                        }
                    }
                }).ToList();
            var sut = new SugarRelator();
            sut.Relate(currentBefore: new Sugar { BloodSugar = currentBefore }, collectedSugars: collectedSugars);
            var expected = expectedAfters.Split(' ').Select(substring => float.Parse(substring, culture)).ToList();
            var actual = collectedSugars.Select(collected => collected.Related.BloodSugar).ToList();
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(collectedSugars.Select(collected => collected.Collected.DateTime),
                collectedSugars.Select(collected => collected.Related.DateTime));
        }
    }
}