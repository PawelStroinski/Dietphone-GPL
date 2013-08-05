using NUnit.Framework;
using Ploeh.SemanticComparison.Fluent;

namespace Dietphone.Models.Tests
{
    public class MealTests : ModelBasedTests
    {
        [TestCase("1 100g 2 200g", "1 100g 2 200g")]
        [TestCase("1 100g 1 100g", "1 200g")]
        [TestCase("1 100g 2 100g 2 200g 1 100g 1 10g", "1 210g 2 300g")]
        [TestCase("1 100g 1 100ml", "1 100g 1 100ml", Description = "Doesn't sum up items with different units")]
        public void NormalizedItemsReturnSameProductsSummedUpAsOneItem(string before, string after)
        {
            var mealBefore = AddMeal("12:00 " + before);
            var mealAfter = AddMeal("12:00 " + after);
            var expected = mealAfter.Items;
            var actual = mealBefore.NormalizedItems();
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
                expected[i].AsSource().OfLikeness<MealItem>().ShouldEqual(actual[i]);
        }

        [Test]
        public void NormalizedItemsReturnsSameItemInstancesWhenNotSums()
        {
            var mealToNormalize = AddMeal("12:00 1 100g 2 100g");
            var normalized = mealToNormalize.NormalizedItems();
            Assert.AreSame(mealToNormalize.Items[0], normalized[0]);
        }

        [Test]
        public void NormalizedItemsReturnsNewItemInstancesWhenSums()
        {
            var mealToNormalize = AddMeal("12:00 1 100g 1 100g");
            var normalized = mealToNormalize.NormalizedItems();
            Assert.AreNotSame(mealToNormalize.Items[0], normalized[0]);
        }
    }
}
