using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;

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
    }
}
