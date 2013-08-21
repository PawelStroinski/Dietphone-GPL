using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Dietphone.Models.Tests
{
    public class SugarEstimatorTests : ModelBasedTests
    {
    }

    public class SugarCollectorTests : ModelBasedTests
    {
        private readonly object dateLock = new object();

        [TestCase(new string[] { "12:00 | 12:00 100 12:40 110" }, "12:00", "12:00 | 12:00 100 12:40 110")]
        [TestCase(new string[] { "12:00 | 12:00 100 12:40 110" }, "12:00", "12:00 | 12:00 100", "12:00 | 12:40 110")]
        [TestCase(new string[] { "12:00 | 12:00 100 12:40 110" }, "12:00", "14:00 | 14:00 100", "14:00 | 14:40 110")]
        [TestCase(new string[] { "12:00 | 12:00 100", "14:00 | 14:30 110" }, "12:00", "12:00 | 12:00 100",
            "12:00 | 14:30 110")]
        [TestCase(new string[] { "10:00 | 10:00 100 10:10 120", "12:00 | 12:59 115 12:30 110" }, "10:00",
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
            Assert.AreNotSame(sugar, actual.Values.First().First().Copy);
            Assert.AreEqual(date, sugar.DateTime);
        }

        [Test]
        public void ReturnsReplacementItems()
        {
            var replacementItems = GetReplacementItems("12:00 | 12:00 100 12:10 120", "12:00 | 12:20 120 12:30 110");
            var sut = new SugarCollector();
            var actual = sut.CollectByHour(AddMeal("12:00"), replacementItems).Values.First();
            Assert.AreEqual(100, actual.First().Copy.BloodSugar);
            Assert.AreEqual(110, actual.Last().Copy.BloodSugar);
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
            Assert.AreEqual(meal.DateTime.AddHours(3), actual.Single().Copy.DateTime);
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
                var actualSugars = actual[TimeSpan.Parse(splet[0])].Select(sugar => sugar.Copy);
                var expectedSugars = AddSugars(splet[1]);
                Assert.IsTrue(expectedSugars.SequenceEqual(actualSugars));
            }
        }
    }
}
