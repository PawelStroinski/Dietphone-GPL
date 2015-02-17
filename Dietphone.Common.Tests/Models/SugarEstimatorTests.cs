using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Globalization;
using Ploeh.AutoFixture;
using NSubstitute;

namespace Dietphone.Models.Tests
{
    public class SugarEstimatorTests : ModelBasedTests
    {
        [Test]
        public void GetEstimatedSugarsAfter()
        {
            var meal = AddMeal("12:00 1 50g 2 200g");
            var currentBefore = AddSugars("12:05 100").First();
            var replacementItems = new List<ReplacementItem>()
            {
                new ReplacementItem(new Pattern
                {
                    From = AddMeal("15:25 1 155g"),
                    Before = AddSugars("15:00 105").First(),
                    After = AddSugars("16:00 105 17:00 115 18:00 145"),
                    For = meal.Items[0],
                    Factor = 2
                }),
                new ReplacementItem(new Pattern
                {
                    From = AddMeal("08:45 2 50g"),
                    Before = AddSugars("08:40 140").First(),
                    After = AddSugars("09:00 150 10:15 140"),
                    For = meal.Items[1],
                    Factor = 3
                }),
            };
            foreach (var replacementItem in replacementItems)
                replacementItem.Pattern.Match = replacementItem.Pattern.From.Items.First();
            factories.Settings.CuSugarsHoursToExcludingPlusOneSmoothing = 1;
            factories.Settings.FpuSugarsHoursFromExcludingMinusOneSmoothing = 1;
            var sut = new SugarEstimatorImpl(factories);
            var actual = sut.GetEstimatedSugarsAfter(meal, currentBefore, replacementItems);
            var expected = AddSugars("12:25 122.8 13:32 103.8 14:35 180");
            Assert.AreEqual(expected, actual);
        }
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

    public class SugarWeighterTests : ModelBasedTests
    {
        [TestCase("both", "12:00 1 50g 7 50g", 5, 10, "15:00 1 75g 7 60g", 7.5, 14)]
        [TestCase("noCu", "12:00 7 50g", 0, 3.5, "15:00 7 100g", 0, 7)]
        [TestCase("noFpu", "12:00 8 50g", 2.5, 0, "15:00 8 100g", 5, 0)]
        [TestCase("noCuInMeal", "12:00 7 50g", 0, 3.5, "15:00 1 75g 7 60g", 7.5, 14)]
        [TestCase("noFpuInMeal", "12:00 8 50g", 2.5, 0, "15:00 1 75g 7 60g", 7.5, 14)]
        [TestCase("noCuInReplacement", "12:00 1 50g 7 50g", 5, 10, "15:00 7 100g", 0, 7)]
        [TestCase("noFpuInReplacement", "12:00 1 50g 7 50g", 5, 10, "15:00 8 100g", 5, 0)]
        public void ReturnsSumOfWeightsReturnedByCuFpuSugarWeighterWeightedByCuToFpuRatioInMealAndReplacementMeal(
            string caseName, string meal, double mealCu, double mealFpu,
            string replacement, double replacementCu, double replacementFpu)
        {
            var collectedSugars = new List<CollectedSugar>();
            var expectedWeights = new List<float>();
            AddProduct(7, energy: 0, carbs: 0, protein: 40, fat: 60);
            AddProduct(8, energy: 0, carbs: 50, protein: 0, fat: 0);
            var currentMeal = AddMeal(meal);
            var replacementMeal = AddMeal(replacement);
            Assert.AreEqual(currentMeal.Cu, mealCu);
            Assert.AreEqual(currentMeal.Fpu, mealFpu);
            Assert.AreEqual(replacementMeal.Cu, replacementCu);
            Assert.AreEqual(replacementMeal.Fpu, replacementFpu);
            var cuFpuSugarWeighter = Substitute.For<CuFpuSugarWeighter>();
            for (int i = 1; i <= 5; i++)
            {
                var collectedSugar = new CollectedSugar { Source = new ReplacementItem { Pattern = new Pattern() } };
                collectedSugar.Source.Pattern.From = replacementMeal;
                collectedSugars.Add(collectedSugar);
                cuFpuSugarWeighter.WeigthCu(currentMeal, collectedSugar).Returns(i * 5000);
                cuFpuSugarWeighter.WeigthFpu(currentMeal, collectedSugar).Returns(i * 10000);
                var expectedWeight = 0.0;
                if (caseName == "both") expectedWeight = i * 5000 * (5.0 / 10) * (7.5 / 14)
                                                       + i * 10000 * (10.0 / 5) * (14 / 7.5);
                else if (caseName == "noCu") expectedWeight = i * 10000;
                else if (caseName == "noFpu") expectedWeight = i * 5000;
                else if (caseName == "noCuInMeal") expectedWeight = i * 10000 * 1 * (14 / 7.5);
                else if (caseName == "noFpuInMeal") expectedWeight = i * 5000 * 1 * (7.5 / 14);
                else if (caseName == "noCuInReplacement") expectedWeight = i * 10000 * (10.0 / 5) * 1;
                else if (caseName == "noFpuInReplacement") expectedWeight = i * 5000 * (5.0 / 10) * 1;
                else throw new InvalidOperationException("Unknown caseName");
                expectedWeights.Add((float)expectedWeight);
            }
            var sut = new SugarWeighter(cuFpuSugarWeighter);
            sut.Weight(currentMeal, collectedSugars);
            var actualWeights = collectedSugars.Select(collected => collected.Weight).ToList();
            Assert.AreEqual(expectedWeights, actualWeights);
        }
    }

    public class CuFpuSugarWeighterTests : ModelBasedTests
    {
        private Meal currentMeal;
        private Meal replacementMeal;
        private float expected;
        private CollectedSugar collectedSugar;

        private void Initialize(
            string meal, string replacementMealWithFirstItemAsReplacementItem, string sugarTime, string expectedWeigth)
        {
            currentMeal = AddMeal("12:00 " + meal);
            replacementMeal = AddMeal("18:00 " + replacementMealWithFirstItemAsReplacementItem);
            var sugar = AddSugars(sugarTime + " 100")[0];
            expected = expectedWeigth.Split('*').Select(substring => float.Parse(substring, new CultureInfo("en")))
                .Aggregate((left, right) => left * right);
            collectedSugar = new CollectedSugar { Source = new ReplacementItem { Pattern = new Pattern() } };
            collectedSugar.Collected = sugar;
            collectedSugar.Related = sugar;
            var pattern = collectedSugar.Source.Pattern;
            pattern.Match = replacementMeal.Items[0];
            pattern.From = replacementMeal;
            pattern.For = currentMeal.Items.First(item => item.Product == pattern.Match.Product);
        }

        [TestCase("1 100g", "1 100g", "12:00", "100*100*1")]
        [TestCase("1 100g", "1 100g", "11:30", "100*100*1")]
        [TestCase("1 100g", "1 100g", "12:30", "100*100*1")]
        [TestCase("1 150g 2 50g", "1 100g", "14:00", "75*100*1")]
        [TestCase("1 150g 2 50g", "2 50g 1 150g", "14:30", "25*25*0.5")]
        [TestCase("1 150g 3 100g", "3 50g 1 100g", "12:30", "25*20*1")]
        [TestCase("3 100g", "3 100g", "14:54", "100*100*0.1")]
        [TestCase("3 100g", "3 100g", "15:00", "100*100*0")]
        [TestCase("3 100g", "3 100g", "15:30", "100*100*0")]
        public void ReturnsProductOfMealItemPercentOfCuInMealAndReplacementMealItemPercentOfCuInReplacementMealAndTime(
            string meal, string replacementMealWithFirstItemAsReplacementItem, string sugarTime, string expectedWeigth)
        {
            var settings = new Settings { CuSugarsHoursToExcludingPlusOneSmoothing = 2 };
            var half = factories.Finder.FindProductById(((byte)3).ToGuid());
            half.CarbsTotalPer100g = 50;
            Assume.That(factories.Products.First().CuPer100g / 2 == half.CuPer100g);
            Initialize(meal, replacementMealWithFirstItemAsReplacementItem, sugarTime, expectedWeigth);
            var sut = new CuFpuSugarWeighterImpl(settings);
            var actual = sut.WeigthCu(currentMeal, collectedSugar);
            Assert.AreEqual(expected, actual);
        }

        [TestCase("1 100g", "1 100g", "12:00", "100*100*0")]
        [TestCase("1 100g", "1 100g", "11:30", "100*100*0")]
        [TestCase("1 100g", "1 100g", "12:30", "100*100*0")]
        [TestCase("1 100g", "1 100g", "12:36", "100*100*0.1")]
        [TestCase("2 100g", "2 100g", "13:24", "100*100*0.9")]
        [TestCase("3 100g", "3 100g", "13:30", "100*100*1")]
        [TestCase("1 150g 2 50g", "1 100g", "14:00", "75*100*1")]
        [TestCase("1 150g 2 50g", "2 50g 1 150g", "13:00", "25*25*0.5")]
        [TestCase("1 150g 3 100g", "3 50g 1 100g", "16:00", "25*20*1")]
        public void ReturnsProductOfMealItemPercentOfFpuInMealAndReplacementMealItemPercentOfFpuInReplacementMealAndTime(
            string meal, string replacementMealWithFirstItemAsReplacementItem, string sugarTime, string expectedWeigth)
        {
            var settings = new Settings { FpuSugarsHoursFromExcludingMinusOneSmoothing = 1.5f };
            var half = factories.Finder.FindProductById(((byte)3).ToGuid());
            half.ProteinPer100g = 73;
            half.FatPer100g = 40;
            Assume.That(factories.Products.First().FpuPer100g / 2 == half.FpuPer100g);
            Initialize(meal, replacementMealWithFirstItemAsReplacementItem, sugarTime, expectedWeigth);
            var sut = new CuFpuSugarWeighterImpl(settings);
            var actual = sut.WeigthFpu(currentMeal, collectedSugar);
            Assert.AreEqual(expected, actual);
        }
    }

    public class SugarAggregatorTests : ModelBasedTests
    {
        [Test]
        public void ReturnsWeightedAverage()
        {
            var collectedByHour = new Dictionary<TimeSpan, List<CollectedSugar>>();
            var dateTime = basedate.AddHours(12).AddMinutes(30);
            collectedByHour[new TimeSpan(20, 10, 00)] = new List<CollectedSugar>
            { 
                new CollectedSugar { Related = new Sugar { BloodSugar = 125, DateTime = dateTime }, Weight = 10000 }
            };
            collectedByHour[new TimeSpan(20, 15, 00)] = new List<CollectedSugar>
            { 
                new CollectedSugar { Related = new Sugar { BloodSugar = 129, DateTime = dateTime }, Weight = 5000 }
            };
            collectedByHour[new TimeSpan(22, 15, 00)] = new List<CollectedSugar>
            { 
                new CollectedSugar { Related = new Sugar { BloodSugar = 90, DateTime = dateTime }, Weight = 7100 },
                new CollectedSugar { Related = new Sugar { BloodSugar = 140, DateTime = dateTime }, Weight = 2900 },
                new CollectedSugar { Related = new Sugar { BloodSugar = 145, DateTime = dateTime }, Weight = 7000 }
            };
            var sut = new SugarAggregator();
            var actual = sut.Aggregate(collectedByHour);
            var actualKeys = actual.Keys.ToList();
            var expectedKeys = new List<Sugar>();
            expectedKeys.Add(new Sugar { BloodSugar = 125, DateTime = basedate + new TimeSpan(20, 10, 00) });
            expectedKeys.Add(new Sugar { BloodSugar = 129, DateTime = basedate + new TimeSpan(20, 15, 00) });
            expectedKeys.Add(new Sugar { BloodSugar = 121.2f, DateTime = basedate + new TimeSpan(22, 15, 00) });
            Assert.AreEqual(expectedKeys, actualKeys);
            foreach (var actualKey in actualKeys)
                Assert.AreSame(collectedByHour[actualKey.DateTime.TimeOfDay], actual[actualKey]);
        }

        [Test]
        public void SkipsWhenWeightCannotBeSummedToNonZero()
        {
            var collectedByHour = new Dictionary<TimeSpan, List<CollectedSugar>>();
            collectedByHour[new TimeSpan(21, 05, 00)] = new List<CollectedSugar>
            { 
                new CollectedSugar { Related = new Sugar { BloodSugar = 100, DateTime = basedate }, Weight = 0 },
                new CollectedSugar { Related = new Sugar { BloodSugar = 120, DateTime = basedate }, Weight = 0 } 
            };
            collectedByHour[new TimeSpan(21, 10, 00)] = new List<CollectedSugar>
            { 
                new CollectedSugar { Related = new Sugar { BloodSugar = 125, DateTime = basedate }, Weight = 10000 }
            };
            collectedByHour[new TimeSpan(21, 25, 00)] = new List<CollectedSugar>
            { 
                new CollectedSugar { Related = new Sugar { BloodSugar = 100, DateTime = basedate }, Weight = -20 },
                new CollectedSugar { Related = new Sugar { BloodSugar = 120, DateTime = basedate }, Weight = 20 } 
            };
            collectedByHour[new TimeSpan(21, 35, 00)] = new List<CollectedSugar>();
            var sut = new SugarAggregator();
            var actual = sut.Aggregate(collectedByHour);
            Assert.AreEqual(new TimeSpan(21, 10, 00), actual.Single().Key.DateTime.TimeOfDay);
        }

        [Test]
        public void SkipsWhenSumOfWeightIsVerySmall()
        {
            // TODO
        }
    }
}
