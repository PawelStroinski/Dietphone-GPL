using System;
using System.Collections.Generic;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System.Linq;

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
        }

        private void InitializeViewModel()
        {
            factories.CreateInsulin().Returns(insulin);
            sut.Load();
        }

        private void ChooseCircumstance()
        {
            var circumstances = sut.Subject.Circumstances;
            circumstances.Add(sut.Circumstances.First());
            sut.Subject.Circumstances = circumstances;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FindAndCopyModel_And_MakeViewModel(bool editingExisting)
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
        public void AddAndSetCircumstance()
        {
            var newCircumstance = new InsulinCircumstance();
            factories.CreateInsulinCircumstance()
                .Returns(newCircumstance)
                .AndDoes(delegate { factories.InsulinCircumstances.Add(newCircumstance); });
            InitializeViewModel();
            var factoriesCountBefore = factories.InsulinCircumstances.Count;
            var sutCountBefore = sut.Circumstances.Count;
            var insulinCountBefore = sut.Subject.Circumstances.Count;
            sut.AddAndSetCircumstance("new");
            Assert.AreEqual(factoriesCountBefore, factories.InsulinCircumstances.Count);
            Assert.AreEqual(sutCountBefore + 1, sut.Circumstances.Count);
            Assert.AreEqual(insulinCountBefore + 1, sut.Subject.Circumstances.Count);
            Assert.AreEqual("new", sut.Circumstances.Last().Name);
            Assert.AreEqual("new", sut.Subject.Circumstances.Last().Name);
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
            Assert.IsFalse(sut.CanDeleteCircumstance());
            ChooseCircumstance();
            Assert.IsTrue(sut.CanDeleteCircumstance());
        }

        [Test]
        public void DeleteCircumstance()
        {
            InitializeViewModel();
            Assert.IsFalse(sut.CanDeleteCircumstance());
            ChooseCircumstance();
            ChooseCircumstance();
            var expected = sut.Subject.Circumstances.Skip(1).ToList();
            sut.DeleteCircumstance();
            var actual = sut.Subject.Circumstances;
            Assert.AreEqual(expected, actual);
        }
    }
}
