using System;
using System.Linq;
using Dietphone.Models;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Dietphone.Common.Phone.Tests
{
    public class NamesAndMealsLoaderTests
    {
        [Test]
        public void MakesDatesAndSortsMeals()
        {
            var factories = Substitute.For<Factories>();
            var fixture = new Fixture();
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            factories.MealNames.Returns(fixture.CreateMany<MealName>().ToList());
            factories.Meals.Returns(fixture.CreateMany<Meal>(100).ToList());
            factories.Meals[0].DateTime = yesterday;
            factories.Meals[1].DateTime = today;
            for (int i = 2; i < 100; i++)
                factories.Meals[i].DateTime = today.AddDays(-i);
            var viewModel = new MealListingViewModel(factories, new BackgroundWorkerSyncFactory());
            var sut = new SutAccessor(viewModel);
            sut.LoadSynchronously();
            Assert.AreEqual(today, viewModel.Dates[0].Date);
            Assert.AreEqual(yesterday, viewModel.Dates[1].Date);
            Assert.AreEqual(today, viewModel.Meals[0].Date.Date);
            Assert.AreEqual(yesterday, viewModel.Meals[1].Date.Date);
            Assert.IsFalse(viewModel.Dates[viewModel.Dates.Count - 2].IsGroupOfOlder);
            Assert.IsTrue(viewModel.Dates[viewModel.Dates.Count - 1].IsGroupOfOlder);
        }

        class SutAccessor : MealListingViewModel.NamesAndMealsLoader
        {
            public SutAccessor(MealListingViewModel viewModel)
                : base(viewModel)
            {
            }

            public void LoadSynchronously()
            {
                base.DoWork();
                base.WorkCompleted();
            }
        }
    }
}
