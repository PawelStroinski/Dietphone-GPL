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

namespace Dietphone.Common.Phone.Tests
{
    public class InsulinEditingViewModelTests
    {
        private Factories factories;
        private Navigator navigator;
        private StateProvider stateProvider;
        private InsulinEditingViewModel sut;
        private Insulin insulin;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            navigator = Substitute.For<Navigator>();
            stateProvider = Substitute.For<StateProvider>();
            sut = new InsulinEditingViewModel(factories);
            sut.Navigator = navigator;
            sut.StateProvider = stateProvider;
            insulin = new Fixture().Create<Insulin>();
            insulin.InitializeCircumstances(new List<Guid>());
            factories.InsulinCircumstances.Returns(new Fixture().CreateMany<InsulinCircumstance>().ToList());
            factories.CreateSugar().Returns(new Sugar());
        }

        private void InitializeViewModel()
        {
            factories.CreateInsulin().Returns(insulin);
            sut.Load();
        }

        private void ChooseCircumstance()
        {
            var circumstances = sut.Subject.Circumstances.ToList();
            circumstances.Add(sut.Circumstances.Except(circumstances).First());
            sut.Subject.Circumstances = circumstances;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FindAndCopyModelAndMakeViewModel(bool editingExisting)
        {
            if (editingExisting)
            {
                navigator.GetInsulinIdToEdit().Returns(insulin.Id);
                factories.Finder.FindInsulinById(insulin.Id).Returns(insulin);
            }
            else
                factories.CreateInsulin().Returns(insulin);
            sut.Load();
            Assert.AreEqual(insulin.Id, sut.Subject.Id);
        }

        [Test]
        public void MakeViewModelFindsSugar()
        {
            var sugar = new Sugar { BloodSugar = 150 };
            factories.Finder.FindSugarBeforeInsulin(insulin).Returns(sugar);
            InitializeViewModel();
            Assert.AreEqual(sugar.BloodSugar.ToString(), sut.CurrentSugar.BloodSugar);
        }

        [Test]
        public void MakeViewModelCreatesSugarIfCantFind()
        {
            var sugar = new Sugar { BloodSugar = 150 };
            factories.CreateSugar().Returns(sugar);
            InitializeViewModel();
            Assert.AreEqual(sugar.BloodSugar.ToString(), sut.CurrentSugar.BloodSugar);
        }

        [Test]
        public void CurrentSugarCanBeWrittenAndRead()
        {
            factories.Settings.Returns(new Settings());
            InitializeViewModel();
            sut.CurrentSugar.BloodSugar = "110";
            Assert.AreEqual("110", sut.CurrentSugar.BloodSugar);
        }

        [Test]
        public void CircumstancesCanBeRead()
        {
            InitializeViewModel();
            var expected = factories.InsulinCircumstances;
            var actual = sut.Circumstances;
            Assert.AreEqual(expected.Count, actual.Count);
        }

        [Test]
        public void CircumstancesAreBuffered()
        {
            InitializeViewModel();
            var expected = factories.InsulinCircumstances.First();
            var actual = sut.Circumstances.First(circumstance => circumstance.Id == expected.Id);
            Assert.AreEqual(expected.Name, actual.Name);
            expected.Name = "foo";
            Assert.AreNotEqual(expected.Name, actual.Name);
        }

        [Test]
        public void AddCircumstance()
        {
            var newCircumstance = new InsulinCircumstance();
            factories.CreateInsulinCircumstance()
                .Returns(newCircumstance)
                .AndDoes(delegate { factories.InsulinCircumstances.Add(newCircumstance); });
            InitializeViewModel();
            var factoriesCountBefore = factories.InsulinCircumstances.Count;
            var sutCountBefore = sut.Circumstances.Count;
            var insulinCountBefore = sut.Subject.Circumstances.Count;
            sut.AddCircumstance("new");
            Assert.AreEqual(factoriesCountBefore, factories.InsulinCircumstances.Count);
            Assert.AreEqual(sutCountBefore + 1, sut.Circumstances.Count);
            Assert.AreEqual(insulinCountBefore, sut.Subject.Circumstances.Count);
            Assert.AreEqual("new", sut.Circumstances.Last().Name);
        }

        [Test]
        public void CanEditCircumstance()
        {
            InitializeViewModel();
            Assert.IsFalse(sut.CanEditCircumstance());
            ChooseCircumstance();
            Assert.IsTrue(sut.CanEditCircumstance());
        }

        [Test]
        public void CanDeleteCircumstance()
        {
            InitializeViewModel();
            Assert.AreEqual(InsulinEditingViewModel.CanDeleteCircumstanceResult.NoCircumstanceChoosen,
                sut.CanDeleteCircumstance());
            ChooseCircumstance();
            Assert.AreEqual(InsulinEditingViewModel.CanDeleteCircumstanceResult.Yes,
                sut.CanDeleteCircumstance());
        }

        [Test]
        public void CanDeleteCircumstanceWhenOnlyOne()
        {
            factories.InsulinCircumstances.Returns(new Fixture().CreateMany<InsulinCircumstance>(1).ToList());
            InitializeViewModel();
            ChooseCircumstance();
            Assert.AreEqual(InsulinEditingViewModel.CanDeleteCircumstanceResult.NoThereIsOnlyOneCircumstance,
                sut.CanDeleteCircumstance());
        }

        [Test]
        public void DeleteCircumstance()
        {
            InitializeViewModel();
            ChooseCircumstance();
            ChooseCircumstance();
            var expected = sut.Subject.Circumstances.Skip(1).ToList();
            sut.DeleteCircumstance();
            var actual = sut.Subject.Circumstances;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SummaryForSelectedCircumstances()
        {
            InitializeViewModel();
            ChooseCircumstance();
            ChooseCircumstance();
            var circumstances = sut.Subject.Circumstances;
            var expected = circumstances.First().Name + ", " + circumstances.Last().Name;
            Assert.AreEqual(expected, sut.SummaryForSelectedCircumstances());
        }

        [Test]
        public void NameOfFirstChoosenCircumstanceGetter()
        {
            InitializeViewModel();
            Assert.AreEqual(string.Empty, sut.NameOfFirstChoosenCircumstance);
            ChooseCircumstance();
            ChooseCircumstance();
            var expected = sut.Subject.Circumstances.First().Name;
            var actual = sut.NameOfFirstChoosenCircumstance;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NameOfFirstChoosenCircumstanceSetter()
        {
            InitializeViewModel();
            Assert.Throws<InvalidOperationException>(() =>
            {
                sut.NameOfFirstChoosenCircumstance = "newname1";
            });
            ChooseCircumstance();
            ChooseCircumstance();
            sut.NameOfFirstChoosenCircumstance = "newname";
            var actual = sut.Subject.Circumstances.First().Name;
            Assert.AreEqual("newname", actual);
        }

        [Test]
        public void InvalidateCircumstancesInvalidatesWithoutChangingAnything()
        {
            factories.CreateInsulinCircumstance().Returns(new InsulinCircumstance());
            InitializeViewModel();
            ChooseCircumstance();
            ChooseCircumstance();
            sut.AddCircumstance(string.Empty);
            sut.DeleteCircumstance();
            sut.NameOfFirstChoosenCircumstance = "foo";
            var previousAll = sut.Circumstances;
            var previousAllIds = sut.Circumstances.Select(circumstance => circumstance.Id).ToList();
            var previousChoosen = sut.Subject.Circumstances;
            var propertyChanged = false;
            sut.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == "Circumstances")
                    propertyChanged = true;
            };
            sut.InvalidateCircumstances();
            Assert.AreNotSame(previousAll, sut.Circumstances);
            Assert.AreEqual(previousAllIds, sut.Circumstances.Select(circumstance => circumstance.Id));
            Assert.AreNotSame(previousChoosen, sut.Subject.Circumstances);
            Assert.IsTrue(propertyChanged);
            Assert.AreEqual(new InsulinCircumstanceViewModel[] { sut.Circumstances.First() }, sut.Subject.Circumstances);
            Assert.AreEqual("foo", sut.NameOfFirstChoosenCircumstance);
            Assert.AreNotEqual("foo", sut.Subject.Circumstances.First().Model.Name, "Should be buffered");
        }
    }
}
