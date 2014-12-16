using System;
using System.Collections.Generic;
using System.Linq;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Dietphone.Common.Phone.Tests
{
    public class MealEditingViewModelTests
    {
        private MealEditingViewModel sut;
        private Meal meal;
        private Factories factories;
        private StateProvider stateProvider;
        private const string NOT_IS_LOCKED_DATE_TIME = "NOT_IS_LOCKED_DATE_TIME";

        [SetUp]
        public void TestInitialize()
        {
            meal = new Meal { Id = Guid.NewGuid() };
            meal.InitializeItems(new List<MealItem>());
            factories = Substitute.For<Factories>();
            factories.Finder.FindMealById(Guid.Empty).Returns(meal);
            factories.MealNames.Returns(new List<MealName>());
            sut = new MealEditingViewModel(factories, new BackgroundWorkerSyncFactory());
            sut.Navigator = Substitute.For<Navigator>();
            stateProvider = Substitute.For<StateProvider>();
            sut.StateProvider = stateProvider;
        }

        [Test]
        public void IsLockedDateTime_DefaultsToTrueWhenDateTimeIsIn3Minutes()
        {
            meal.DateTime = DateTime.Now.AddMinutes(-2.9);
            sut.Load();
            Assert.IsTrue(!sut.NotIsLockedDateTime);
        }

        [Test]
        public void IsLockedDateTime_DefaultsToFalseWhenDateTimeIsNotIn3Minutes()
        {
            meal.DateTime = DateTime.Now.AddMinutes(-3.1);
            sut.Load();
            Assert.IsFalse(!sut.NotIsLockedDateTime);
        }

        [Test]
        public void IsLockedDateTime_SetToTrueSetsDateTimeToCurrent()
        {
            sut.Load();
            sut.NotIsLockedDateTime = !true;
            Assert.IsTrue(DateTime.Now - sut.Subject.DateTime <= TimeSpan.FromSeconds(1));
        }

        [Test]
        public void DateTime_WhenChanged_SetsIsLockedDateTimeToFalse()
        {
            meal.DateTime = DateTime.Now;
            sut.Load();
            Assert.IsTrue(!sut.NotIsLockedDateTime);
            sut.Subject.DateTime = DateTime.Now.AddSeconds(1);
            Assert.IsFalse(!sut.NotIsLockedDateTime);
        }

        [Test]
        public void TombstoneOthers_Tombstones_NotIsLockedDateTime()
        {
            sut.ItemEditing = new MealItemEditingViewModel();
            sut.Load();
            sut.Tombstone();
            Assert.AreEqual(sut.NotIsLockedDateTime, stateProvider.State[NOT_IS_LOCKED_DATE_TIME]);
        }

        [Test]
        public void UntombstoneOthers_Untombstones_NotIsLockedDateTime()
        {
            var expected = !sut.NotIsLockedDateTime;
            stateProvider.State[NOT_IS_LOCKED_DATE_TIME].Returns(expected);
            stateProvider.State.ContainsKey(NOT_IS_LOCKED_DATE_TIME).Returns(true);
            sut.Load();
            Assert.AreEqual(expected, sut.NotIsLockedDateTime);
        }

        [Test]
        public void DateFormat()
        {
            Assert.IsTrue(sut.DateFormat.Contains("yy"));
        }

        [Test]
        public void GoToInsulinEditingSetsIsDirtyToFalse()
        {
            sut.Load();
            sut.IsDirty = true;
            sut.GoToInsulinEditing();
            Assert.IsFalse(sut.IsDirty);
        }

        [Test]
        public void GoToInsulinEditingWhenInsulinExists()
        {
            sut.Load();
            sut.IsDirty = true;
            var insulin = new Insulin { Id = Guid.NewGuid() };
            factories.Finder.FindInsulinByMeal(meal).Returns(insulin);
            sut.GoToInsulinEditing();
            sut.Navigator.Received().GoToInsulinEditingRelatedToMeal(insulin.Id, meal.Id);
        }

        [Test]
        public void GoToInsulinEditingWhenInsulinIsNew()
        {
            sut.Load();
            sut.IsDirty = true;
            sut.GoToInsulinEditing();
            sut.Navigator.Received().GoToNewInsulinRelatedToMeal(meal.Id);
        }

        [Test]
        public void AddsProductToMruWhenAddsCopyOfItem()
        {
            factories.CreateMealItem().Returns(new MealItem());
            factories.Finder.Returns(new FinderImpl(factories));
            factories.MruProducts.Returns(new MruProductsImpl(new List<Guid>(), factories));
            factories.Products.Returns(new Fixture().CreateMany<Product>().ToList());
            sut.Load();
            sut.AddCopyOfThisItem = new MealItem { ProductId = factories.Products.First().Id };
            sut.AddCopyOfThisItem.SetOwner(factories);
            sut.ReturnedFromNavigation();
            Assert.AreEqual(factories.Products.Take(1), factories.MruProducts.Products);
        }

        [Test]
        public void InvalidatesProductNameAfterAllChangesAreDoneWhenAddsCopyOfItem()
        {
            var mealItem = new MealItem();
            mealItem.SetOwner(factories);
            factories.CreateMealItem().Returns(mealItem);
            factories.Finder.Returns(new FinderImpl(factories));
            factories.Products.Returns(new Fixture().CreateMany<Product>().ToList());
            sut.Load();
            sut.AddCopyOfThisItem = new MealItem { ProductId = factories.Products.First().Id };
            sut.AddCopyOfThisItem.SetOwner(factories);
            var productName = string.Empty;
            sut.Subject.Items.CollectionChanged += delegate
            {
                var item = sut.Subject.Items.First();
                item.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == "ProductName")
                        productName = item.ProductName;
                };
            };
            sut.ReturnedFromNavigation();
            Assert.AreEqual(factories.Products.First().Name, productName);
        }

        [Test]
        public void ReturnedFromNavigationInvalidatesScoresWhenWentToSettings()
        {
            sut.Load();
            sut.OpenScoresSettings();
            sut.Subject.Scores.ChangesProperty(string.Empty, () => sut.ReturnedFromNavigation());
        }

        [Test]
        public void ChangingTheItemsSetsIsDirty()
        {
            sut.Load();
            factories.CreateMealItem().Returns(new MealItem());
            sut.Subject.AddItem();
            Assert.IsTrue(sut.IsDirty);
            sut.IsDirty = false;
            var item = sut.Subject.Items[0];
            item.Value = "100";
            Assert.IsTrue(sut.IsDirty);
        }
    }
}
