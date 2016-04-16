using System;
using System.Collections.Generic;
using System.Linq;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using Dietphone.Views;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Dietphone.Smartphone.Tests
{
    public class MealEditingViewModelTests : TestBase
    {
        private MealEditingViewModel sut;
        private Meal meal;
        private Factories factories;
        private StateProvider stateProvider;
        private TrialViewModel trial;
        private MealEditingViewModel.BackNavigation backNavigation;
        private MessageDialog messageDialog;
        private const string NOT_IS_LOCKED_DATE_TIME = "NOT_IS_LOCKED_DATE_TIME";

        [SetUp]
        public void TestInitialize()
        {
            meal = new Meal { Id = Guid.NewGuid() };
            meal.InitializeItems(new List<MealItem>());
            factories = Substitute.For<Factories>();
            factories.Finder.FindMealById(meal.Id).Returns(meal);
            factories.MealNames.Returns(new List<MealName>());
            factories.MealNames.Add(new MealName { Id = Guid.NewGuid() });
            factories.DefaultEntities.MealName.Returns(new MealName { Id = Guid.NewGuid() });
            trial = Substitute.For<TrialViewModel>();
            backNavigation = new MealEditingViewModel.BackNavigation();
            messageDialog = Substitute.For<MessageDialog>();
            sut = new MealEditingViewModel(factories, new BackgroundWorkerSyncFactory(), trial, backNavigation,
                new MealItemEditingViewModel(), messageDialog);
            sut.Navigator = Substitute.For<Navigator>();
            stateProvider = Substitute.For<StateProvider>();
            sut.StateProvider = stateProvider;
            sut.Init(new MealEditingViewModel.Navigation { MealIdToEdit = meal.Id });
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
            factories.Settings.Returns(new Settings());
            factories.Products.Returns(new Fixture().CreateMany<Product>().ToList());
            sut.Load();
            backNavigation.AddCopyOfThisItem = new MealItem { ProductId = factories.Products.First().Id };
            backNavigation.AddCopyOfThisItem.SetOwner(factories);
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
            backNavigation.AddCopyOfThisItem = new MealItem { ProductId = factories.Products.First().Id };
            backNavigation.AddCopyOfThisItem.SetOwner(factories);
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
            sut.OpenScoresSettings.Call();
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

        [Test]
        public void AddItemCallsTrialRunAndThenNavigatorGoToMainToAddMealItem()
        {
            sut.Navigator.When(navigator => navigator.GoToMainToAddMealItem()).Do(_ => trial.Received().Run());
            sut.AddItem.Call();
            sut.Navigator.Received().GoToMainToAddMealItem();
        }

        [Test]
        public void Messages()
        {
            Assert.AreEqual(Translations.AreYouSureYouWantToSaveThisMeal, sut.Messages.CannotSaveCaption);
        }

        [TestCase("foo")]
        [TestCase(null)]
        public void AddName(string name)
        {
            sut.Load();
            factories.CreateMealName().Returns(new MealName());
            messageDialog.Input(Translations.Name, Translations.AddName).Returns(name);
            var beforeAddingEditingNameCalled = false;
            var afterAddedEditedNameCalled = false;
            sut.BeforeAddingEditingName += delegate { beforeAddingEditingNameCalled = true; };
            sut.AfterAddedEditedName += delegate { afterAddedEditedNameCalled = true; };
            sut.AddName.Call();
            factories.Received(name == null ? 0 : 1).CreateMealName();
            Assert.IsTrue(beforeAddingEditingNameCalled);
            Assert.AreEqual(name != null, afterAddedEditedNameCalled);
        }

        [TestCase("foo", false)]
        [TestCase("foo", true)]
        [TestCase(null, true)]
        public void EditName(string newName, bool selectedNonDefault)
        {
            sut.Load();
            if (selectedNonDefault)
                sut.Subject.Name = sut.Names.Last();
            var beforeAddingEditingNameCalled = false;
            var afterAddedEditedNameCalled = false;
            var inputCalled = false;
            sut.BeforeAddingEditingName += delegate { beforeAddingEditingNameCalled = true; };
            sut.AfterAddedEditedName += delegate { afterAddedEditedNameCalled = true; };
            messageDialog.Input(Translations.Name, Translations.EditName, value: sut.NameOfName).Returns(_ =>
            {
                inputCalled = true;
                return newName;
            });
            sut.EditName.Call();
            messageDialog.Received(selectedNonDefault ? 0 : 1).Show(sut.NameOfName, Translations.CannotEditThisName);
            Assert.AreEqual(selectedNonDefault, beforeAddingEditingNameCalled);
            Assert.AreEqual(selectedNonDefault, inputCalled);
            Assert.AreEqual(newName != null && selectedNonDefault, afterAddedEditedNameCalled);
            if (newName != null && selectedNonDefault)
                Assert.AreEqual(newName, sut.NameOfName);
            else
                Assert.AreNotEqual(newName, sut.NameOfName);
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void DeleteName(bool selectedNonDefault, bool confirmSetup)
        {
            sut.Load();
            if (selectedNonDefault)
                sut.Subject.Name = sut.Names.Last();
            var nameDeleteCalled = false;
            var expected = sut.Names.Count - 1;
            sut.NameDelete += (_, action) =>
            {
                var actualBefore = sut.Names.Count;
                Assert.AreEqual(expected + 1, actualBefore);
                action();
                var actualAfter = sut.Names.Count;
                Assert.AreEqual(expected, actualAfter);
                nameDeleteCalled = true;
            };
            var confirmCalled = false;
            messageDialog.Confirm(string.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisName,
                sut.NameOfName), Translations.DeleteName).Returns(_ =>
                {
                    confirmCalled = true;
                    return confirmSetup;
                });
            sut.DeleteName.Call();
            messageDialog.Received(selectedNonDefault ? 0 : 1)
                .Show(sut.NameOfName, Translations.CannotDeleteThisName);
            Assert.AreEqual(selectedNonDefault, confirmCalled);
            Assert.AreEqual(confirmCalled && confirmSetup, nameDeleteCalled);
        }

        [Test]
        public void DeleteNameWhenNameDeleteEventNotHandled()
        {
            sut.Load();
            sut.Subject.Name = sut.Names.Last();
            var expected = sut.Names.Count - 1;
            messageDialog.Confirm(null, null).ReturnsForAnyArgs(true);
            sut.DeleteName.Call();
            var actual = sut.Names.Count;
            Assert.AreEqual(expected, actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DeleteAndSaveAndReturn(bool confirm)
        {
            factories.Meals.Returns(new List<Meal> { meal });
            sut.Load();
            messageDialog.Confirm(string.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisMeal,
                sut.IdentifiableName), Translations.DeleteMeal).Returns(confirm);
            sut.DeleteAndSaveAndReturn();
            Assert.AreEqual(confirm ? 0 : 1, factories.Meals.Count);
        }
    }
}
