using System;
using System.Collections.Generic;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using Dietphone.Views;
using System.Collections.ObjectModel;
using Moq;

namespace Dietphone.Smartphone.Tests
{
    public class JournalViewModelTests
    {
        private Factories factories;
        private JournalViewModel sut;
        private SugarEditingViewModel sugarEditing;
        private StateProvider stateProvider;
        private Navigator navigator;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            factories.InsulinCircumstances.Returns(new List<InsulinCircumstance>());
            factories.MealNames.Returns(new List<MealName>());
            factories.Insulins.Returns(new List<Insulin>());
            factories.Sugars.Returns(new List<Sugar>());
            factories.Meals.Returns(new List<Meal>());
            factories.Settings.Returns(new Settings());
            sugarEditing = Substitute.For<SugarEditingViewModel>();
            sut = new JournalViewModel(factories, new BackgroundWorkerSyncFactory(), sugarEditing);
            stateProvider = Substitute.For<StateProvider>();
            stateProvider.State.Returns(new Dictionary<string, object>());
            sut.StateProvider = stateProvider;
            navigator = Substitute.For<Navigator>();
            sut.Navigator = navigator;
        }

        [Test]
        public void LoadLoadsWhenNotLoadedAlready()
        {
            var loaded = false;
            sut.Loaded += delegate { loaded = true; };
            sut.ChangesProperty("IsBusy", () =>
            {
                sut.Load();
            });
            Assert.IsTrue(loaded);
        }

        [Test]
        public void LoadDoesntLoadWhenLoadedAlready()
        {
            sut.Load();
            sut.NotChangesProperty("IsBusy", () =>
            {
                sut.Load();
            });
        }

        [Test]
        public void RefreshDoesntLoadWhenNotLoadedAlready()
        {
            sut.NotChangesProperty("IsBusy", () =>
            {
                sut.Refresh();
            });
        }

        [Test]
        public void RefreshLoadsWhenNotLoadedAlready()
        {
            sut.Load();
            var refreshed = false;
            sut.Refreshed += delegate { refreshed = true; };
            sut.ChangesProperty("IsBusy", () =>
            {
                sut.Refresh();
            });
            Assert.IsTrue(refreshed);
        }

        [Test]
        public void FindItem()
        {
            var sugar = new Sugar { Id = Guid.NewGuid() };
            var insulin = new Insulin { Id = Guid.NewGuid() };
            var meal = new Meal { Id = Guid.NewGuid() };
            factories.Sugars.Add(sugar);
            factories.Insulins.Add(insulin);
            factories.Meals.Add(meal);
            sut.Load();
            Assert.IsNull(sut.FindItem(Guid.Empty));
            Assert.IsInstanceOf<SugarViewModel>(sut.FindItem(sugar.Id));
            Assert.IsInstanceOf<InsulinViewModel>(sut.FindItem(insulin.Id));
            Assert.IsInstanceOf<MealViewModel>(sut.FindItem(meal.Id));
        }

        [Test]
        public void FindDate()
        {
            var sugar = new Sugar { DateTime = DateTime.Now };
            factories.Sugars.Add(sugar);
            sut.Load();
            Assert.AreEqual(sugar.DateTime.Date, sut.FindDate(sugar.DateTime.Date).Date);
        }

        [Test]
        public void ChooseWhenInsulin()
        {
            var navigator = new Mock<Navigator>(); // Why this test isn't stable with NSubstitute instead of Moq?
            sut.Navigator = navigator.Object;
            var insulin = new Insulin { Id = Guid.NewGuid() };
            var viewModel = new InsulinViewModel(insulin, factories, new List<InsulinCircumstanceViewModel>());
            sut.Choose(viewModel);
            navigator.Verify(Navigator => Navigator.GoToInsulinEditing(insulin.Id));
        }

        [Test]
        public void ChooseWhenSugar()
        {
            var sugar = new Sugar { BloodSugar = 100 };
            var viewModel = new SugarViewModel(sugar, factories);
            sut.Choose(viewModel);
            sugarEditing.Received().Show(Arg.Is<SugarViewModel>(vm => "100" == vm.BloodSugar));
        }

        [Test]
        public void ChooseWhenMeal()
        {
            var navigator = new Mock<Navigator>();
            sut.Navigator = navigator.Object;
            var meal = new MealViewModel(new Meal { Id = Guid.NewGuid() }, factories);
            sut.Choose(meal);
            navigator.Verify(Navigator => Navigator.GoToMealEditing(meal.Id));
        }

        [Test]
        public void ChooseCreatesACopyOfSugar()
        {
            var sugar = new Sugar { BloodSugar = 100 };
            var viewModel = new SugarViewModel(sugar, factories);
            sut.Choose(viewModel);
            sugarEditing.Subject.BloodSugar = "110";
            Assert.AreEqual(100, sugar.BloodSugar);
        }

        [Test]
        public void ChooseAndSugarEditingConfirm()
        {
            var sugar = new Sugar { BloodSugar = 100 };
            factories.Sugars.Add(sugar);
            sut.Load();
            var items = sut.Items.ToList();
            var viewModel = new SugarViewModel(sugar, factories);
            sut.Choose(viewModel);
            sugarEditing.Subject.BloodSugar = "110";
            sugarEditing.Confirm.Call();
            Assert.AreEqual(110, sugar.BloodSugar);
            Assert.AreNotEqual(items, sut.Items, "Should refresh");
        }

        [Test]
        public void ChooseAndSugarEditingDelete()
        {
            var sugar = new Sugar();
            factories.Sugars.Add(sugar);
            sut.Load();
            var viewModel = new SugarViewModel(sugar, factories);
            sut.Choose(viewModel);
            Assert.IsTrue(sugarEditing.CanDelete);
            sugarEditing.Delete.Call();
            Assert.IsEmpty(factories.Sugars);
            Assert.IsEmpty(sut.Items);
        }

        [Test]
        public void ChooseAndSugarEditingCancel()
        {
            var sugar = new Sugar();
            factories.Sugars.Add(sugar);
            sut.Load();
            var viewModel = new SugarViewModel(sugar, factories);
            sut.Choose(viewModel);
            sugarEditing.Cancel.Call();
            Assert.IsNotEmpty(factories.Sugars);
            Assert.IsNotEmpty(sut.Items);
        }

        [Test]
        public void AddInsulin()
        {
            var command = new JournalViewModel.AddInsulinCommand();
            sut.Add(command);
            navigator.Received().GoToNewInsulin();
            factories.DidNotReceive().CreateSugar();
        }

        [Test]
        public void AddSugar()
        {
            var sugar = new Sugar { BloodSugar = 110 };
            factories.CreateSugar().Returns(sugar).AndDoes(_ => factories.Sugars.Add(sugar));
            sut.Load();
            var command = new JournalViewModel.AddSugarCommand();
            sut.Add(command);
            sugarEditing.Received().Show(Arg.Is<SugarViewModel>(vm => "110" == vm.BloodSugar));
            Assert.AreEqual(1, sut.Items.Count);
            navigator.DidNotReceive().GoToNewInsulin();
        }

        [Test]
        public void AddSugarAndSugarEditingCancel()
        {
            var sugar = new Sugar();
            factories.CreateSugar().Returns(sugar).AndDoes(_ => factories.Sugars.Add(sugar));
            sut.Load();
            var command = new JournalViewModel.AddSugarCommand();
            sut.Add(command);
            sugarEditing.Cancel.Call();
            Assert.IsEmpty(factories.Sugars);
            Assert.IsEmpty(sut.Items);
        }

        [Test]
        public void AddMeal()
        {
            var meal = new Meal { Id = Guid.NewGuid() };
            factories.CreateMeal().Returns(meal);
            var command = new JournalViewModel.AddMealCommand();
            sut.Add(command);
            navigator.Received().GoToMealEditing(meal.Id);
        }

        [Test]
        public void TombstoneAndUntombstoneWhenSugarEditing()
        {
            var sugar = new Sugar { DateTime = DateTime.Now };
            factories.Sugars.Add(sugar);
            var viewModel = new SugarViewModel(sugar, factories);
            sut.Load();
            sut.Choose(viewModel);
            viewModel.BloodSugar = "100";
            sut.Tombstone();
            sugarEditing.Cancel.Call();
            sut.Untombstone();
            Assert.IsTrue(sugarEditing.IsVisible);
            sugarEditing.Confirm.Call();
            Assert.AreEqual(100, sugar.BloodSugar);
        }

        [Test]
        public void TombstoneAndUntombstoneWhenNotSugarEditing()
        {
            sut.Tombstone();
            sut.Untombstone();
            Assert.IsFalse(sugarEditing.IsVisible);
        }

        [Test]
        public void OnSearchChanged()
        {
            var sut = new SutAccessor();
            var updating = false;
            var update = true;
            var updated = false;
            sut.DescriptorsUpdating += delegate { updating = true; };
            sut.DescriptorsUpdated += delegate { updated = true; };
            sut.UpdateFilterDescriptors += delegate
            {
                Assert.IsTrue(updating);
                Assert.IsFalse(updated);
                update = true;
            };
            sut.OnSearchChanged();
            Assert.IsTrue(update);
            Assert.IsTrue(updated);
        }

        [Test]
        public void Grouping()
        {
            var sut = new GroupingJournalViewModel(factories, new BackgroundWorkerSyncFactory(), sugarEditing);
            Assert.IsNull(sut.Grouping.Groups);
        }

        class SutAccessor : JournalViewModel
        {
            public SutAccessor()
                : base(Substitute.For<Factories>(), Substitute.For<BackgroundWorkerFactory>(),
                    Substitute.For<SugarEditingViewModel>())
            {
            }

            public new void OnSearchChanged()
            {
                base.OnSearchChanged();
            }
        }
    }
}
