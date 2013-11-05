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

namespace Dietphone.Common.Phone.Tests
{
    public class InsulinEditingViewModelTests
    {
        private Factories factories;
        private Navigator navigator;
        private StateProvider stateProvider;
        private InsulinEditingViewModel sut;
        private Insulin insulin;
        private ReplacementBuilderAndSugarEstimatorFacade facade;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            navigator = Substitute.For<Navigator>();
            stateProvider = Substitute.For<StateProvider>();
            facade = Substitute.For<ReplacementBuilderAndSugarEstimatorFacade>();
            sut = new InsulinEditingViewModel(factories, facade);
            sut.Navigator = navigator;
            sut.StateProvider = stateProvider;
            insulin = new Fixture().Create<Insulin>();
            insulin.InitializeCircumstances(new List<Guid>());
            factories.InsulinCircumstances.Returns(new Fixture().CreateMany<InsulinCircumstance>().ToList());
            factories.CreateSugar().Returns(new Sugar());
            factories.Settings.Returns(new Settings());
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
        public void MakeViewModelCopiesFoundSugar()
        {
            var sugar = new Sugar { BloodSugar = 150 };
            factories.Finder.FindSugarBeforeInsulin(insulin).Returns(sugar);
            InitializeViewModel();
            sut.CurrentSugar.BloodSugar = "155";
            Assert.AreEqual(150, sut.CurrentSugar.BloodSugar);
        }

        [Test]
        public void MakeViewModelCopiesCreatedSugar()
        {
            var sugar = new Sugar { BloodSugar = 150 };
            factories.CreateSugar().Returns(sugar);
            InitializeViewModel();
            sut.CurrentSugar.BloodSugar = "155";
            Assert.AreEqual(150, sut.CurrentSugar.BloodSugar);
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

        public class ReplacementAndEstimatedSugarsTests : InsulinEditingViewModelTests
        {
            private ReplacementAndEstimatedSugars replacementAndEstimatedSugars;

            [SetUp]
            public new void TestInitialize()
            {
                var meal = new Meal();
                replacementAndEstimatedSugars = new ReplacementAndEstimatedSugars
                {
                    Replacement = new Replacement
                    {
                        InsulinTotal
                            = new Insulin { NormalBolus = 2.5f, SquareWaveBolus = 2, SquareWaveBolusHours = 3 }
                    },
                    EstimatedSugars
                        = new List<Sugar> { new Sugar { BloodSugar = 100, DateTime = DateTime.Now.AddHours(1) },
                                            new Sugar { BloodSugar = 110, DateTime = DateTime.Now.AddHours(2) } }
                };
                factories.Finder.FindMealByInsulin(insulin).Returns(meal);
                facade.GetReplacementAndEstimatedSugars(meal,
                    Arg.Is<Insulin>(temp => temp.Id == insulin.Id),
                    Arg.Is<Sugar>(temp => temp.BloodSugar == 100f))
                    .Returns(replacementAndEstimatedSugars);
            }

            [Test]
            public void IsBusy()
            {
                Assert.IsFalse(sut.IsBusy);
                sut.ChangesProperty("IsBusy", () => { sut.IsBusy = true; });
                Assert.IsTrue(sut.IsBusy);
            }

            [Test]
            public void InsulinHeaderCalculatedVisibleIsFalseAndTextIsEmptyAfterOpen()
            {
                InitializeViewModel();
                Assert.IsFalse(sut.InsulinHeaderCalculatedVisible);
                Assert.IsEmpty(sut.InsulinHeaderCalculatedText);
            }

            [Test]
            public void WhenOpenedWithEnteredBolusNeverRecalculatesIt()
            {
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Thread.Sleep(10);
                facade.DidNotReceiveWithAnyArgs().GetReplacementAndEstimatedSugars(null, null, null);
            }

            [Test]
            public void WhenOpenedWithNoBolusDoesNotCalculateItImmediately()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                Thread.Sleep(10);
                facade.DidNotReceiveWithAnyArgs().GetReplacementAndEstimatedSugars(null, null, null);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void WhenOpenedWithNoBolusCalculatesItAfterSugarOrCircumstanceChange(bool sugarChange)
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                if (sugarChange)
                    sut.CurrentSugar.BloodSugar = "100";
                else
                    sut.Subject.Circumstances = sut.Subject.Circumstances.ToList();
                Thread.Sleep(10);
                Assert.AreEqual(2.5f, sut.Subject.Insulin.NormalBolus);
                Assert.AreEqual(2f, sut.Subject.Insulin.SquareWaveBolus);
                Assert.AreEqual(3f, sut.Subject.Insulin.SquareWaveBolusHours);
            }

            [Test]
            public void CalculationUpdatesInsulinHeaderCalculatedVisible()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.ChangesProperty("InsulinHeaderCalculatedVisible", () =>
                {
                    sut.CurrentSugar.BloodSugar = "100";
                    Thread.Sleep(10);
                });
                Assert.IsTrue(sut.InsulinHeaderCalculatedVisible);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void CalculationUpdatesInsulinHeaderCalculatedText(bool isComplete)
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                replacementAndEstimatedSugars.Replacement.IsComplete = isComplete;
                InitializeViewModel();
                sut.ChangesProperty("InsulinHeaderCalculatedText", () =>
                {
                    sut.CurrentSugar.BloodSugar = "100";
                    Thread.Sleep(10);
                });
                Assert.AreEqual(isComplete
                    ? Translations.InsulinHeaderCalculated : Translations.InsulinHeaderIncomplete,
                    sut.InsulinHeaderCalculatedText);
            }
        }
    }
}
