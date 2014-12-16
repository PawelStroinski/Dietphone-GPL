using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using System.Collections.Generic;

namespace Dietphone.Common.Phone.Tests
{
    public class MealViewModelTests
    {
        private Factories factories;
        private Meal meal;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            meal = new Meal();
        }

        [Test]
        public void TextAndText2()
        {
            var sut = new MealViewModel(meal, factories);
            Assert.IsEmpty(sut.Text);
            Assert.IsEmpty(sut.Text2);
        }

        [Test]
        public void ChangingTheItemsInvalidatesTheScores()
        {
            var sut = new MealViewModel(meal, factories);
            meal.SetOwner(factories);
            meal.InitializeItems(new List<MealItem>());
            factories.CreateMealItem().Returns(new MealItem());
            sut.Scores.ChangesProperty(string.Empty, () => sut.AddItem());
            var item = sut.Items[0];
            sut.Scores.ChangesProperty(string.Empty, () => item.Value = "100");
        }
    }
}
