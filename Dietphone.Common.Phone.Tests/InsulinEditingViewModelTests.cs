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
        private Sugar sugar;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            navigator = Substitute.For<Navigator>();
            stateProvider = Substitute.For<StateProvider>();
            facade = Substitute.For<ReplacementBuilderAndSugarEstimatorFacade>();
            sut = new InsulinEditingViewModel(factories, facade, new BackgroundWorkerSyncFactory());
            sut.Navigator = navigator;
            sut.StateProvider = stateProvider;
            insulin = new Fixture().Create<Insulin>();
            insulin.InitializeCircumstances(new List<Guid>());
            sugar = new Sugar();
            factories.InsulinCircumstances.Returns(new Fixture().CreateMany<InsulinCircumstance>().ToList());
            factories.CreateSugar().Returns(sugar);
            factories.Settings.Returns(new Settings { MaxBolus = 5 });
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
            Assert.AreEqual(150, sugar.BloodSugar);
        }

        [Test]
        public void MakeViewModelCopiesCreatedSugar()
        {
            var sugar = new Sugar { BloodSugar = 150 };
            factories.CreateSugar().Returns(sugar);
            InitializeViewModel();
            sut.CurrentSugar.BloodSugar = "155";
            Assert.AreEqual(150, sugar.BloodSugar);
        }

        [Test]
        public void MakeViewModelSetsOwnerOfCopiedSugar()
        {
            var sugar = new Sugar { BloodSugar = 150 };
            factories.CreateSugar().Returns(sugar);
            InitializeViewModel();
            Assert.AreEqual(150, sut.CurrentSugar.Sugar.BloodSugarInMgdL);
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
            sut.ChangesProperty("Circumstances", () =>
            {
                sut.InvalidateCircumstances();
                Assert.AreNotSame(previousAll, sut.Circumstances);
                Assert.AreEqual(previousAllIds, sut.Circumstances.Select(circumstance => circumstance.Id));
                Assert.AreNotSame(previousChoosen, sut.Subject.Circumstances);
            });
            Assert.AreEqual(new InsulinCircumstanceViewModel[] { sut.Circumstances.First() }, sut.Subject.Circumstances);
            Assert.AreEqual("foo", sut.NameOfFirstChoosenCircumstance);
            Assert.AreNotEqual("foo", sut.Subject.Circumstances.First().Model.Name, "Should be buffered");
        }

        [Test]
        public void ShouldFocusSugarWhenNewSugar()
        {
            InitializeViewModel();
            Assert.IsTrue(sut.ShouldFocusSugar());
        }

        [Test]
        public void ShouldNotFocusSugarWhenExistingSugar()
        {
            var sugar = new Sugar { BloodSugar = 150 };
            factories.Finder.FindSugarBeforeInsulin(insulin).Returns(sugar);
            InitializeViewModel();
            Assert.IsFalse(sut.ShouldFocusSugar());
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
                        Items = new List<ReplacementItem> { new ReplacementItem() },
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
            public void IsCalculatedIsFalseAndTextIsEmptyAfterOpen()
            {
                InitializeViewModel();
                Assert.IsFalse(sut.IsCalculated);
                Assert.IsEmpty(sut.IsCalculatedText);
            }

            [Test]
            public void WhenOpenedWithEnteredBolusNeverRecalculatesIt()
            {
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                facade.DidNotReceiveWithAnyArgs().GetReplacementAndEstimatedSugars(null, null, null);
            }

            [Test]
            public void WhenOpenedWithNoBolusDoesNotCalculateItImmediately()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                facade.DidNotReceiveWithAnyArgs().GetReplacementAndEstimatedSugars(null, null, null);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void WhenOpenedWithNoBolusCalculatesItAfterSugarOrCircumstancesChanged(bool onSugarChange)
            {
                if (!onSugarChange)
                    sugar.BloodSugar = 100;
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.Subject.ChangesProperty("NormalBolus", () =>
                {
                    sut.Subject.ChangesProperty("SquareWaveBolus", () =>
                    {
                        sut.Subject.ChangesProperty("SquareWaveBolusHours", () =>
                        {
                            if (onSugarChange)
                                sut.CurrentSugar.BloodSugar = "100";
                            else
                                ChooseCircumstance();
                        });
                    });
                });
                Assert.AreEqual(2.5f, sut.Subject.Insulin.NormalBolus);
                Assert.AreEqual(2f, sut.Subject.Insulin.SquareWaveBolus);
                Assert.AreEqual(3f, sut.Subject.Insulin.SquareWaveBolusHours);
            }

            [Test]
            public void SugarNeedsToBeEnteredForCalculation()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                ChooseCircumstance();
                facade.DidNotReceiveWithAnyArgs().GetReplacementAndEstimatedSugars(null, null, null);
            }

            [Test]
            public void CalculationUpdatesIsCalculated()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.ChangesProperty("IsCalculated", () =>
                {
                    sut.CurrentSugar.BloodSugar = "100";
                });
                Assert.IsTrue(sut.IsCalculated);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void CalculationUpdatesIsCalculatedText(bool isComplete)
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                replacementAndEstimatedSugars.Replacement.IsComplete = isComplete;
                InitializeViewModel();
                sut.ChangesProperty("IsCalculatedText", () =>
                {
                    sut.CurrentSugar.BloodSugar = "100";
                });
                Assert.AreEqual(isComplete
                    ? Translations.InsulinHeaderCalculated : Translations.InsulinHeaderIncomplete,
                    sut.IsCalculatedText);
            }

            [Test]
            public void SetsIsBusyForTheTimeOfCalculation()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.ChangesProperty("IsBusy", () =>
                {
                    sut.CurrentSugar.BloodSugar = "100";
                });
                Assert.IsFalse(sut.IsBusy);
            }

            [Test]
            public void WhenNoReplacementsFoundDoesNotShowCalculation()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.AreNotEqual(0, sut.Subject.Insulin.NormalBolus);
                sut.Subject.Insulin.NormalBolus = 0;
                sut.Subject.Insulin.SquareWaveBolus = 0;
                replacementAndEstimatedSugars.Replacement.Items.Clear();
                ChooseCircumstance();
                Assert.AreEqual(0, sut.Subject.Insulin.NormalBolus);
                Assert.AreEqual(0, sut.Subject.Insulin.SquareWaveBolus);
                Assert.IsFalse(sut.IsCalculated);
            }

            [Test]
            public void WhenNoReplacementsFoundHidesPreviousCalculation()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.AreNotEqual(0, sut.Subject.Insulin.NormalBolus);
                replacementAndEstimatedSugars.Replacement.Items.Clear();
                ChooseCircumstance();
                Assert.AreEqual(0, sut.Subject.Insulin.NormalBolus);
                Assert.AreEqual(0, sut.Subject.Insulin.SquareWaveBolus);
                Assert.AreEqual(0, sut.Subject.Insulin.SquareWaveBolusHours);
                Assert.IsFalse(sut.IsCalculated);
            }

            [Test]
            public void WhenNormalBolusIsEditedSetsIsCalculatedToFalse()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsTrue(sut.IsCalculated);
                sut.ChangesProperty("IsCalculated", () =>
                {
                    sut.Subject.NormalBolus = "1.5";
                });
                Assert.IsFalse(sut.IsCalculated);
            }

            [Test]
            public void WhenSquareWaveBolusIsEditedSetsIsCalculatedToFalse()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsTrue(sut.IsCalculated);
                sut.ChangesProperty("IsCalculated", () =>
                {
                    sut.Subject.SquareWaveBolus = "1.5";
                });
                Assert.IsFalse(sut.IsCalculated);
            }

            [Test]
            public void WhenSquareWaveBolusHoursIsEditedSetsIsCalculatedToFalse()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsTrue(sut.IsCalculated);
                sut.ChangesProperty("IsCalculated", () =>
                {
                    sut.Subject.SquareWaveBolusHours = "1.5";
                });
                Assert.IsFalse(sut.IsCalculated);
            }

            [Test]
            public void AfterNormalBolusIsEditedDoesNotCalculate()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.Subject.NormalBolus = "1.5";
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsFalse(sut.IsCalculated);
            }

            [Test]
            public void AfterSquareWaveBolusIsEditedDoesNotCalculate()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.Subject.SquareWaveBolus = "1.5";
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsFalse(sut.IsCalculated);
            }

            [Test]
            public void AfterSquareWaveBolusHoursIsEditedDoesNotCalculate()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.Subject.SquareWaveBolusHours = "1.5";
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsFalse(sut.IsCalculated);
            }

            [Test]
            public void CalculationDoesNotCountAsEditing()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.AreEqual(replacementAndEstimatedSugars.Replacement.InsulinTotal.NormalBolus,
                    sut.Subject.Insulin.NormalBolus);
                replacementAndEstimatedSugars.Replacement.InsulinTotal.NormalBolus--;
                ChooseCircumstance();
                Assert.AreEqual(replacementAndEstimatedSugars.Replacement.InsulinTotal.NormalBolus,
                    sut.Subject.Insulin.NormalBolus);
            }

            [Test]
            public void WhenEditedBolusIsEmptiedCalculatesAgain1()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.Subject.NormalBolus = "1.5";
                sut.Subject.SquareWaveBolus = "1.5";
                sut.Subject.SquareWaveBolusHours = "1.5";
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsFalse(sut.IsCalculated);
                sut.Subject.NormalBolus = string.Empty;
                Assert.IsFalse(sut.IsCalculated);
                sut.Subject.SquareWaveBolus = string.Empty;
                Assert.IsFalse(sut.IsCalculated);
                sut.Subject.SquareWaveBolusHours = string.Empty;
                Assert.IsTrue(sut.IsCalculated);
            }

            [Test]
            public void WhenEditedBolusIsEmptiedCalculatesAgain2()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.Subject.NormalBolus = "1.5";
                sut.Subject.SquareWaveBolus = "1.5";
                sut.Subject.SquareWaveBolusHours = "1.5";
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsFalse(sut.IsCalculated);
                sut.Subject.SquareWaveBolusHours = string.Empty;
                Assert.IsFalse(sut.IsCalculated);
                sut.Subject.SquareWaveBolus = string.Empty;
                Assert.IsFalse(sut.IsCalculated);
                sut.Subject.NormalBolus = string.Empty;
                Assert.IsTrue(sut.IsCalculated);
            }

            [Test]
            public void WhenEditedBolusIsEmptiedCalculatesAgain3()
            {
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.Subject.NormalBolus = "1.5";
                sut.Subject.SquareWaveBolus = "1.5";
                sut.Subject.SquareWaveBolusHours = "1.5";
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsFalse(sut.IsCalculated);
                sut.Subject.SquareWaveBolusHours = string.Empty;
                Assert.IsFalse(sut.IsCalculated);
                sut.Subject.NormalBolus = string.Empty;
                Assert.IsFalse(sut.IsCalculated);
                sut.Subject.SquareWaveBolus = string.Empty;
                Assert.IsTrue(sut.IsCalculated);
            }

            [Test]
            public void UsesRelatedMealIdWhenProvided()
            {
                var relatedMeal = new Meal { Id = Guid.NewGuid() };
                navigator.GetRelatedMealId().Returns(relatedMeal.Id);
                factories.Finder.FindMealById(relatedMeal.Id).Returns(relatedMeal);
                insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                var replacementAndEstimatedSugars = new ReplacementAndEstimatedSugars
                {
                    Replacement = new Replacement
                    {
                        InsulinTotal = new Insulin { NormalBolus = 1.5f },
                        Items = this.replacementAndEstimatedSugars.Replacement.Items
                    },
                    EstimatedSugars = this.replacementAndEstimatedSugars.EstimatedSugars
                };
                facade.GetReplacementAndEstimatedSugars(relatedMeal, Arg.Any<Insulin>(), Arg.Any<Sugar>())
                    .Returns(replacementAndEstimatedSugars);
                sut.CurrentSugar.BloodSugar = "100";
                Assert.AreEqual(1.5f, sut.Subject.Insulin.NormalBolus);
            }
        }
    }
}
