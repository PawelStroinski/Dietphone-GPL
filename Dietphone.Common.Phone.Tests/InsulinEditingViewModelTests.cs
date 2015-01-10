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
        private Settings settings;
        private Meal meal;

        [SetUp]
        public void TestInitialize()
        {
            var fixture = new Fixture();
            factories = Substitute.For<Factories>();
            navigator = Substitute.For<Navigator>();
            stateProvider = Substitute.For<StateProvider>();
            facade = Substitute.For<ReplacementBuilderAndSugarEstimatorFacade>();
            CreateSut();
            insulin = fixture.Create<Insulin>();
            insulin.InitializeCircumstances(new List<Guid>());
            insulin.SetOwner(factories);
            sugar = new Sugar();
            sugar.SetOwner(factories);
            factories.InsulinCircumstances.Returns(fixture.CreateMany<InsulinCircumstance>().ToList());
            factories.CreateSugar().Returns(sugar);
            settings = new Settings { MaxBolus = 5 };
            factories.Settings.Returns(settings);
            meal = fixture.Create<Meal>();
            factories.Finder.FindMealByInsulin(insulin).Returns(meal);
            factories.Finder.FindInsulinById(insulin.Id).Returns(insulin);
            var replacementAndEstimatedSugars = new ReplacementAndEstimatedSugars();
            replacementAndEstimatedSugars.EstimatedSugars = new List<Sugar>();
            replacementAndEstimatedSugars.Replacement
                = new Replacement { InsulinTotal = new Insulin(), Items = new List<ReplacementItem>() };
            facade.GetReplacementAndEstimatedSugars(Arg.Any<Meal>(), Arg.Any<Insulin>(), Arg.Any<Sugar>())
                    .Returns(replacementAndEstimatedSugars);
            factories.MealNames.Returns(new List<MealName>());
        }

        private void CreateSut()
        {
            sut = new InsulinEditingViewModel(factories, facade, new BackgroundWorkerSyncFactory());
            sut.Navigator = navigator;
            sut.StateProvider = stateProvider;
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

        public class GeneralTests : InsulinEditingViewModelTests
        {
            [TestCase(true)]
            [TestCase(false)]
            public void FindAndCopyModelAndMakeViewModel(bool editingExisting)
            {
                if (editingExisting)
                    navigator.GetInsulinIdToEdit().Returns(insulin.Id);
                else
                    factories.CreateInsulin().Returns(insulin);
                sut.Load();
                Assert.AreEqual(insulin.Id, sut.Subject.Id);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void FindAndCopyModelCopiesDateTimeFromMealWhenNewInsulin(bool editingExisting)
            {
                if (editingExisting)
                    navigator.GetInsulinIdToEdit().Returns(insulin.Id);
                else
                    factories.CreateInsulin().Returns(insulin);
                sut.Load();
                if (editingExisting)
                {
                    Assert.AreNotEqual(meal.DateTime, insulin.DateTime);
                    Assert.AreNotEqual(meal.DateTime, sugar.DateTime);
                }
                else
                {
                    Assert.AreEqual(meal.DateTime, insulin.DateTime);
                    Assert.AreEqual(meal.DateTime, sugar.DateTime);
                    Assert.AreEqual(meal.DateTime.ToLocalTime(), sut.Subject.DateTime);
                }
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
                sugar.BloodSugar = 150;
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
                sugar.BloodSugar = 150;
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "155";
                Assert.AreEqual(150, sugar.BloodSugar);
            }

            [Test]
            public void MakeViewModelSetsOwnerOfCopiedSugar()
            {
                sugar.BloodSugar = 150;
                InitializeViewModel();
                Assert.AreEqual(150, sut.CurrentSugar.Sugar.BloodSugarInMgdL);
            }

            [Test]
            public void WhenMakeViewModelCreatesSugarItSetsItsDateToInsulinsDate()
            {
                InitializeViewModel();
                Assert.AreEqual(insulin.DateTime, sugar.DateTime);
            }

            [Test]
            public void WhenMakeViewModelFindsSugarItDoesntChangeItsDate()
            {
                factories.Finder.FindSugarBeforeInsulin(insulin).Returns(sugar);
                InitializeViewModel();
                Assert.AreNotEqual(insulin.DateTime, sugar.DateTime);
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
            public void SaveWithUpdatedTimeAndReturn()
            {
                meal.DateTime = DateTime.Now.AddSeconds(-10);
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "140";
                ChooseCircumstance();
                sut.Subject.NormalBolus = "2.1";
                sut.Subject.SquareWaveBolus = "2.2";
                sut.Subject.SquareWaveBolusHours = "2.3";
                sut.Subject.Note = "note";
                sut.SaveWithUpdatedTimeAndReturn();
                Assert.AreEqual(140, sugar.BloodSugar);
                Assert.AreEqual(1, insulin.Circumstances.Count());
                Assert.AreEqual(sut.Circumstances.First().Id, insulin.ReadCircumstances().First());
                Assert.AreEqual(2.1, insulin.NormalBolus, 0.01);
                Assert.AreEqual(2.2, insulin.SquareWaveBolus, 0.01);
                Assert.AreEqual(2.3, insulin.SquareWaveBolusHours, 0.01);
                Assert.AreEqual("note", insulin.Note);
                Assert.AreEqual(DateTime.UtcNow.Ticks, insulin.DateTime.Ticks, TimeSpan.TicksPerSecond * 5);
                Assert.AreEqual(DateTime.UtcNow.Ticks, sugar.DateTime.Ticks, TimeSpan.TicksPerSecond * 5);
                navigator.Received().GoBack();
            }

            [Test]
            public void SaveWithUpdatedTimeAndReturnGoesForwardToMainPageInsteadOfGoingBackWhenRelatedMealIdGiven()
            {
                navigator.GetRelatedMealId().Returns(Guid.NewGuid());
                InitializeViewModel();
                sut.SaveWithUpdatedTimeAndReturn();
                navigator.Received().GoToMain();
                navigator.DidNotReceive().GoBack();
            }

            [Test]
            public void SaveWithUpdatedTimeAndReturnSavesCircumstances()
            {
                InitializeViewModel();
                var deletedCircumstanceId = sut.Circumstances.First().Id;
                var renamedCircumstanceId = sut.Circumstances.Skip(1).First().Id;
                var newCircumstance = new InsulinCircumstance();
                factories.CreateInsulinCircumstance().Returns(newCircumstance);
                ChooseCircumstance();
                sut.DeleteCircumstance();
                ChooseCircumstance();
                sut.NameOfFirstChoosenCircumstance = "newname";
                sut.AddCircumstance("foo");
                sut.SaveWithUpdatedTimeAndReturn();
                Assert.IsFalse(factories.InsulinCircumstances
                    .Any(circumstance => circumstance.Id == deletedCircumstanceId));
                Assert.AreEqual("newname", factories.InsulinCircumstances
                    .First(circumstance => circumstance.Id == renamedCircumstanceId).Name);
                Assert.IsTrue(factories.InsulinCircumstances.Contains(newCircumstance));
            }

            [Test]
            public void TombstoneAndUntombstone()
            {
                stateProvider.State.Returns(new Dictionary<string, object>());
                InitializeViewModel();
                sut.NotIsLockedDateTime = false;
                sut.CurrentSugar.BloodSugar = "120";
                sut.Tombstone();
                CreateSut();
                sut.dateTimeNow = () => DateTime.Now.AddHours(-1.5);
                InitializeViewModel();
                Assert.IsFalse(sut.NotIsLockedDateTime);
                Assert.AreEqual("120", sut.CurrentSugar.BloodSugar);
            }

            [Test]
            public void TombstoneAndUntombstoneDoesntCreateNewInsulin()
            {
                stateProvider.State.Returns(new Dictionary<string, object>());
                InitializeViewModel();
                sut.Subject.NormalBolus = "1.1";
                sut.Tombstone();
                CreateSut();
                factories.ClearReceivedCalls();
                InitializeViewModel();
                factories.DidNotReceive().CreateInsulin();
                sut.SaveWithUpdatedTimeAndReturn();
                Assert.AreEqual(1.1f, insulin.NormalBolus);
            }

            [Test]
            public void TombstoneAndUntombstoneCircumstances()
            {
                stateProvider.State.Returns(new Dictionary<string, object>());
                InitializeViewModel();
                var deletedCircumstanceId = sut.Circumstances.First().Id;
                var renamedCircumstanceId = sut.Circumstances.Skip(1).First().Id;
                var newCircumstance = new InsulinCircumstance();
                factories.CreateInsulinCircumstance().Returns(newCircumstance);
                ChooseCircumstance();
                sut.DeleteCircumstance();
                ChooseCircumstance();
                sut.NameOfFirstChoosenCircumstance = "newname";
                sut.AddCircumstance("foo");
                sut.Subject.NormalBolus = "1";
                sut.Tombstone();
                CreateSut();
                InitializeViewModel();
                sut.SaveWithUpdatedTimeAndReturn();
                Assert.IsFalse(factories.InsulinCircumstances
                    .Any(circumstance => circumstance.Id == deletedCircumstanceId));
                Assert.AreEqual("newname", factories.InsulinCircumstances
                    .First(circumstance => circumstance.Id == renamedCircumstanceId).Name);
                Assert.IsTrue(factories.InsulinCircumstances.Contains(newCircumstance));
                Assert.AreEqual(1, sut.Subject.Circumstances.Count());
                Assert.AreEqual("newname", sut.NameOfFirstChoosenCircumstance);
                Assert.AreEqual("1", sut.Subject.NormalBolus);
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

            [Test]
            public void ChoosingCircumstanceSignalsIsDirty()
            {
                InitializeViewModel();
                Assert.IsFalse(sut.IsDirty);
                ChooseCircumstance();
                Assert.IsTrue(sut.IsDirty);
            }

            [Test]
            public void ChangingBolusSignalsIsDirty()
            {
                InitializeViewModel();
                Assert.IsFalse(sut.IsDirty);
                sut.Subject.NormalBolus = "1";
                Assert.IsTrue(sut.IsDirty);
            }

            [Test]
            public void ChangingCurrentSugarSignalsIsDirty()
            {
                InitializeViewModel();
                Assert.IsFalse(sut.IsDirty);
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsTrue(sut.IsDirty);
            }

            [Test]
            public void DeleteAndSaveAndReturn()
            {
                var insulins = new List<Insulin> { insulin };
                factories.Insulins.Returns(insulins);
                factories.Sugars.Returns(new List<Sugar>());
                InitializeViewModel();
                ChooseCircumstance();
                var circumtanceId = sut.Subject.Circumstances.Single().Id;
                sut.NameOfFirstChoosenCircumstance = "foo";
                sut.CurrentSugar.BloodSugar = "100";
                sut.Subject.NormalBolus = "1";
                sut.DeleteAndSaveAndReturn();
                Assert.IsEmpty(insulins);
                Assert.AreEqual("foo", factories.InsulinCircumstances.FindById(circumtanceId).Name);
                Assert.AreNotEqual(100, sugar.BloodSugar);
                Assert.AreNotEqual(1, insulin.NormalBolus);
                navigator.Received().GoBack();
            }

            [Test]
            public void DeleteAndSaveAndReturnDeletesTheNewlyCreatedSugar()
            {
                factories.Sugars.Returns(new List<Sugar> { sugar });
                factories.Insulins.Returns(new List<Insulin>());
                InitializeViewModel();
                sut.DeleteAndSaveAndReturn();
                Assert.IsEmpty(factories.Sugars);
            }

            [Test]
            public void DeleteAndSaveAndReturnDoesntDeleteTheFoundSugar()
            {
                var sugar = new Sugar();
                factories.Finder.FindSugarBeforeInsulin(insulin).Returns(sugar);
                factories.Sugars.Returns(new List<Sugar> { sugar });
                factories.Insulins.Returns(new List<Insulin>());
                InitializeViewModel();
                sut.DeleteAndSaveAndReturn();
                Assert.IsNotEmpty(factories.Sugars);
            }

            [Test]
            public void CancelAndReturnCallsTheBaseCancelAndReturn()
            {
                factories.Sugars.Returns(new List<Sugar>());
                InitializeViewModel();
                sut.CancelAndReturn();
                navigator.Received().GoBack();
            }

            [Test]
            public void CancelAndReturnDeletesTheNewlyCreatedSugar()
            {
                factories.Sugars.Returns(new List<Sugar> { sugar });
                InitializeViewModel();
                sut.CancelAndReturn();
                Assert.IsEmpty(factories.Sugars);
            }

            [Test]
            public void CancelAndReturnDoesntDeleteTheFoundSugar()
            {
                var sugar = new Sugar();
                factories.Finder.FindSugarBeforeInsulin(insulin).Returns(sugar);
                factories.Sugars.Returns(new List<Sugar> { sugar });
                InitializeViewModel();
                sut.CancelAndReturn();
                Assert.IsNotEmpty(factories.Sugars);
            }

            [TestCase(true, false)]
            [TestCase(false, true)]
            [TestCase(false, false)]
            public void GoToMealEditing(bool newMeal, bool relatedMealProvided)
            {
                InitializeViewModel();
                sut.IsDirty = true;
                sut.CurrentSugar.BloodSugar = "140";
                if (newMeal || relatedMealProvided)
                    factories.Finder.FindMealByInsulin(insulin).Returns((Meal)null);
                if (newMeal)
                    factories.CreateMeal().Returns(meal);
                if (relatedMealProvided)
                {
                    navigator.GetRelatedMealId().Returns(meal.Id);
                    factories.Finder.FindMealById(meal.Id).Returns(meal);
                }
                sut.GoToMealEditing();
                Assert.AreEqual(140, sugar.BloodSugar);
                Assert.IsFalse(sut.IsDirty);
                sut.Navigator.Received().GoToMealEditing(meal.Id);
            }

            [Test]
            public void MealScores()
            {
                InitializeViewModel();
                Assert.IsInstanceOf<ScoreSelector>(sut.MealScores);
                Assert.IsNotInstanceOf<EmptyScoreSelector>(sut.MealScores);
                Assert.IsTrue(sut.MealScoresVisible);
            }

            [Test]
            public void MealScoresWhenNoMeal()
            {
                factories.Finder.FindMealByInsulin(insulin).Returns((Meal)null);
                InitializeViewModel();
                Assert.IsInstanceOf<EmptyScoreSelector>(sut.MealScores);
                Assert.IsFalse(sut.MealScoresVisible);
            }

            [Test]
            public void OpenScoresSettings()
            {
                sut.OpenScoresSettings();
                navigator.Received().GoToSettings();
            }

            [TestCase(true)]
            [TestCase(false)]
            public void ReturnedFromNavigationInvalidatesScoresIfWentToSettings(bool wentToSettings)
            {
                InitializeViewModel();
                if (wentToSettings)
                {
                    sut.OpenScoresSettings();
                    sut.MealScores.ChangesProperty(string.Empty, () => sut.ReturnedFromNavigation());
                }
                sut.MealScores.NotChangesProperty(string.Empty, () => sut.ReturnedFromNavigation());
            }

            [TestCase(true)]
            [TestCase(false)]
            public void NoMealPresent(bool expectedNoMealPresent)
            {
                if (expectedNoMealPresent)
                    factories.Finder.FindMealByInsulin(insulin).Returns((Meal)null);
                sut.ChangesProperty("NoMealPresent", () => InitializeViewModel());
                Assert.AreEqual(expectedNoMealPresent, sut.NoMealPresent);
            }

            [Test]
            public void NoSugarEntered()
            {
                sut.ChangesProperty("NoSugarEntered", () => InitializeViewModel());
                Assert.IsTrue(sut.NoSugarEntered);
                sut.ChangesProperty("NoSugarEntered", () => sut.CurrentSugar.BloodSugar = "100");
                Assert.IsFalse(sut.NoSugarEntered);
            }
        }

        public class ReplacementAndEstimatedSugarsTests : InsulinEditingViewModelTests
        {
            private ReplacementAndEstimatedSugars replacementAndEstimatedSugars;

            [SetUp]
            public new void TestInitialize()
            {
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
                facade.GetReplacementAndEstimatedSugars(meal,
                    Arg.Is<Insulin>(temp => temp.Id == insulin.Id),
                    Arg.Is<Sugar>(temp => temp.BloodSugar == 100f))
                    .Returns(replacementAndEstimatedSugars);
            }

            private void CheckTheCalculationAndTheSugarChartAreThere()
            {
                Assert.IsTrue(sut.IsCalculated);
                Assert.IsNotEmpty(sut.Calculated.Text);
                Assert.IsNotEmpty(sut.SugarChart);
            }

            private void CheckPatternViewModel(Pattern expected, PatternViewModel actual,
                IList<PatternViewModel> alternatives)
            {
                Assert.AreSame(expected, actual.Pattern);
                Assert.AreSame(expected.Match, actual.Match.Model);
                Assert.AreSame(expected.From, actual.From.Meal);
                Assert.AreSame(expected.Insulin, actual.Insulin.Insulin);
                Assert.AreSame(expected.Before, actual.Before.Sugar);
                Assert.AreSame(expected.After.ElementAt(1), actual.After[1].Sugar);
                Assert.AreSame(expected.For, actual.For.Model);
                Assert.IsNotNull(actual.Match.Scores.First);
                Assert.IsNotNull(actual.From.Scores.First);
                Assert.IsNotNull(actual.From.Name);
                actual.From.Meal.NameId = Guid.Empty;
                Assert.IsNotNull(actual.From.Name);
                Assert.IsNotNull(actual.Insulin.Circumstances);
                Assert.IsNotEmpty(actual.Before.Text);
                Assert.IsNotEmpty(actual.After[1].Text);
                Assert.IsNotNull(actual.For.Scores.First);
                actual.Insulin.NormalBolus = "1";
                CheckPatternViewModelGoToMeal(expected, actual);
                CheckPatternViewModelGoToInsulin(expected, actual);
                CheckPatternViewModelShowAlternatives(actual, alternatives);
            }

            private void CheckPatternViewModelGoToMeal(Pattern expected, PatternViewModel actual)
            {
                navigator.ClearReceivedCalls();
                sut.Subject.NormalBolus = "2.1";
                actual.GoToMeal();
                Assert.AreEqual(2.1, insulin.NormalBolus, 0.01);
                navigator.Received().GoToMealEditing(expected.From.Id);
            }

            private void CheckPatternViewModelGoToInsulin(Pattern expected, PatternViewModel actual)
            {
                navigator.ClearReceivedCalls();
                sut.Subject.NormalBolus = "2.2";
                actual.GoToInsulin();
                Assert.AreEqual(2.2, insulin.NormalBolus, 0.01);
                navigator.Received().GoToInsulinEditing(expected.Insulin.Id);
            }

            private void CheckPatternViewModelShowAlternatives(PatternViewModel actual,
                IList<PatternViewModel> alternatives)
            {
                if (!actual.HasAlternatives)
                {
                    Assert.Throws<InvalidOperationException>(() => actual.ShowAlternatives());
                    return;
                }
                Assert.IsFalse(sut.CalculationDetailsAlternativesVisible);
                Assert.IsEmpty(sut.CalculationDetailsAlternatives);
                sut.ChangesProperty("CalculationDetailsAlternativesVisible", () =>
                {
                    sut.ChangesProperty("CalculationDetailsAlternatives", () =>
                    {
                        actual.ShowAlternatives();
                    });
                });
                Assert.IsTrue(sut.CalculationDetailsAlternativesVisible);
                Assert.AreSame(alternatives, sut.CalculationDetailsAlternatives);
                sut.ChangesProperty("CalculationDetailsAlternativesVisible", () =>
                {
                    sut.CloseCalculationDetailsAlternatives();
                });
                Assert.IsFalse(sut.CalculationDetailsAlternativesVisible);
            }

            [Test]
            public void IsBusy()
            {
                Assert.IsFalse(sut.IsBusy);
                sut.ChangesProperty("IsBusy", () => { sut.IsBusy = true; });
                Assert.IsTrue(sut.IsBusy);
            }

            [Test]
            public void IsCalculatedIsFalseAfterOpen()
            {
                InitializeViewModel();
                Assert.IsFalse(sut.IsCalculated);
                Assert.IsFalse(sut.IsCalculationIncomplete);
                Assert.IsFalse(sut.IsCalculationEmpty);
            }

            [Test]
            public void CalculatedHasEmptyValueAfterOpen()
            {
                InitializeViewModel();
                Assert.IsEmpty(sut.Calculated.Text);
            }

            [Test]
            public void SugarChartIsEmptyAfterOpen()
            {
                InitializeViewModel();
                Assert.IsEmpty(sut.SugarChart);
            }

            [TestCase(true, false)]
            [TestCase(false, false)]
            [TestCase(true, true)]
            [TestCase(false, true)]
            public void CalculatesAfterSugarOrCircumstancesChanged(bool onSugarChange, bool openedWithNoBolus)
            {
                if (!onSugarChange)
                    sugar.BloodSugar = 100;
                if (openedWithNoBolus)
                    insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                sut.ChangesProperty("Calculated", () =>
                {
                    if (onSugarChange)
                        sut.CurrentSugar.BloodSugar = "100";
                    else
                        ChooseCircumstance();
                });
                Assert.AreEqual(2.5f, sut.Calculated.Insulin.NormalBolus);
                Assert.AreEqual(2f, sut.Calculated.Insulin.SquareWaveBolus);
                Assert.AreEqual(3f, sut.Calculated.Insulin.SquareWaveBolusHours);
                Assert.AreNotEqual(sut.Calculated.Text, sut.Subject.Text);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void SugarNeedsToBeEnteredForCalculation(bool openedWithNoBolus)
            {
                if (openedWithNoBolus)
                    insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                ChooseCircumstance();
                facade.DidNotReceiveWithAnyArgs().GetReplacementAndEstimatedSugars(null, null, null);
            }

            [Test]
            public void CalculationUpdatesIsCalculated()
            {
                InitializeViewModel();
                sut.ChangesProperty("IsCalculated", () =>
                {
                    sut.CurrentSugar.BloodSugar = "100";
                });
                Assert.IsTrue(sut.IsCalculated);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void CalculationUpdatesIsCalculationIncomplete(bool isComplete)
            {
                replacementAndEstimatedSugars.Replacement.IsComplete = isComplete;
                InitializeViewModel();
                sut.ChangesProperty("IsCalculationIncomplete", () =>
                {
                    sut.CurrentSugar.BloodSugar = "100";
                });
                Assert.AreNotEqual(isComplete, sut.IsCalculationIncomplete);
            }

            [Test]
            public void SetsIsBusyForTheTimeOfCalculation()
            {
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
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsNotEmpty(sut.Calculated.Text);
                replacementAndEstimatedSugars.Replacement.Items.Clear();
                ChooseCircumstance();
                Assert.IsEmpty(sut.Calculated.Text);
                Assert.IsFalse(sut.IsCalculated);
            }

            [Test]
            public void WhenNoReplacementsFoundDoesNotShowSugarChart()
            {
                InitializeViewModel();
                replacementAndEstimatedSugars.Replacement.Items.Clear();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsEmpty(sut.SugarChart);
            }

            [Test]
            public void WhenNoReplacementsFoundHidesPreviousCalculation()
            {
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsNotEmpty(sut.Calculated.Text);
                Assert.IsTrue(sut.IsCalculationIncomplete);
                replacementAndEstimatedSugars.Replacement.Items.Clear();
                sut.ChangesProperty("IsCalculationIncomplete", () =>
                {
                    sut.ChangesProperty("IsCalculated", () =>
                    {
                        sut.ChangesProperty("Calculated", () =>
                        {
                            ChooseCircumstance();
                        });
                    });
                });
                Assert.IsEmpty(sut.Calculated.Text);
                Assert.IsFalse(sut.IsCalculated);
                Assert.IsFalse(sut.IsCalculationIncomplete);
            }

            [Test]
            public void WhenNoReplacementsFoundHidesPreviousSugarChart()
            {
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsNotEmpty(sut.SugarChart);
                replacementAndEstimatedSugars.Replacement.Items.Clear();
                sut.ChangesProperty("SugarChart", () =>
                {
                    ChooseCircumstance();
                });
                Assert.IsEmpty(sut.SugarChart);
            }

            [Test]
            public void WhenNoReplacementsFoundSetsIsCalculationEmptyToTrueAndViceVersa()
            {
                InitializeViewModel();
                replacementAndEstimatedSugars.Replacement.Items.Clear();
                sut.ChangesProperty("IsCalculationEmpty", () =>
                {
                    sut.CurrentSugar.BloodSugar = "100";
                });
                Assert.IsTrue(sut.IsCalculationEmpty);
                replacementAndEstimatedSugars.Replacement.Items.Add(new ReplacementItem());
                sut.ChangesProperty("IsCalculationEmpty", () =>
                {
                    ChooseCircumstance();
                });
                Assert.IsFalse(sut.IsCalculationEmpty);
            }

            [Test]
            public void WhenABolusIsEditedKeepsTheCalculationAndTheSugarChart()
            {
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                CheckTheCalculationAndTheSugarChartAreThere();
                sut.Subject.NormalBolus = "1.5";
                sut.Subject.SquareWaveBolus = "1.5";
                sut.Subject.SquareWaveBolusHours = "1.5";
                CheckTheCalculationAndTheSugarChartAreThere();
            }

            [Test]
            public void AfterBolusIsEditedDoesStillCalculate()
            {
                InitializeViewModel();
                sut.Subject.NormalBolus = "1.5";
                sut.Subject.SquareWaveBolus = "1.5";
                sut.Subject.SquareWaveBolusHours = "1.5";
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsTrue(sut.IsCalculated);
            }

            [Test]
            public void CalculationCanBeUpdated()
            {
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.AreEqual(replacementAndEstimatedSugars.Replacement.InsulinTotal.NormalBolus,
                    sut.Calculated.Insulin.NormalBolus);
                replacementAndEstimatedSugars.Replacement.InsulinTotal.NormalBolus--;
                sut.ChangesProperty("Calculated", () => ChooseCircumstance());
                Assert.AreEqual(replacementAndEstimatedSugars.Replacement.InsulinTotal.NormalBolus,
                    sut.Calculated.Insulin.NormalBolus);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void WhenSugarAlreadyExistsCalculatesImmediately(bool openedWithNoBolus)
            {
                var sugar = new Sugar { BloodSugar = 100 };
                factories.Finder.FindSugarBeforeInsulin(insulin).Returns(sugar);
                if (openedWithNoBolus)
                    insulin.NormalBolus = insulin.SquareWaveBolus = 0;
                InitializeViewModel();
                Assert.IsTrue(sut.IsCalculated);
                facade.ReceivedWithAnyArgs().GetReplacementAndEstimatedSugars(null, null, null);
            }

            [Test]
            public void UsesRelatedMealIdWhenProvided()
            {
                var relatedMeal = new Meal { Id = Guid.NewGuid() };
                navigator.GetRelatedMealId().Returns(relatedMeal.Id);
                factories.Finder.FindMealById(relatedMeal.Id).Returns(relatedMeal);
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
                Assert.AreEqual(1.5f, sut.Calculated.Insulin.NormalBolus);
            }

            [Test]
            public void CalculationPopulatesSugarChartWithCurrentAndEstimatedSugarsAfter()
            {
                var estimatedSugars = replacementAndEstimatedSugars.EstimatedSugars;
                InitializeViewModel();
                sut.ChangesProperty("SugarChart", () =>
                {
                    sut.CurrentSugar.BloodSugar = "100";
                    Assert.AreEqual(3, sut.SugarChart.Count);
                    Assert.AreEqual(meal.DateTime, sut.SugarChart[0].DateTime.ToUniversalTime());
                    Assert.AreEqual(100, sut.SugarChart[0].BloodSugar);
                    Assert.AreEqual(estimatedSugars[0].DateTime, sut.SugarChart[1].DateTime);
                    Assert.AreEqual(estimatedSugars[0].BloodSugar, sut.SugarChart[1].BloodSugar);
                    Assert.AreEqual(estimatedSugars[1].DateTime, sut.SugarChart[2].DateTime);
                    Assert.AreEqual(estimatedSugars[1].BloodSugar, sut.SugarChart[2].BloodSugar);
                });
            }

            [Test]
            public void SugarChartMinumumReturnsMinimumOfChartMinus10WhenUnitIsMgdL()
            {
                var estimatedSugars = replacementAndEstimatedSugars.EstimatedSugars;
                estimatedSugars[1].BloodSugar = 98.1f;
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.AreEqual(88.1f, sut.SugarChartMinimum);
            }

            [Test]
            public void SugarChartMinumumReturnsMinimumOfChartMinus0_55WhenUnitIsMmolL()
            {
                var estimatedSugars = replacementAndEstimatedSugars.EstimatedSugars;
                estimatedSugars[1].BloodSugar = 95.1f;
                sugar.BloodSugar = 100;
                InitializeViewModel();
                settings.SugarUnit = SugarUnit.mmolL;
                ChooseCircumstance();
                Assert.AreEqual(94.55f, (double)sut.SugarChartMinimum, 0.001);
            }

            [Test]
            public void SugarChartMaximumReturnsMaximumOfChartPlus55WhenUnitIsMgdL()
            {
                var estimatedSugars = replacementAndEstimatedSugars.EstimatedSugars;
                estimatedSugars[1].BloodSugar = 155.1f;
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.AreEqual(210.1f, sut.SugarChartMaximum);
            }

            [Test]
            public void SugarChartMaximumReturnsMaximumOfChartPlus3_05WhenUnitIsMmolL()
            {
                var estimatedSugars = replacementAndEstimatedSugars.EstimatedSugars;
                estimatedSugars[1].BloodSugar = 155.1f;
                sugar.BloodSugar = 100;
                InitializeViewModel();
                settings.SugarUnit = SugarUnit.mmolL;
                ChooseCircumstance();
                Assert.AreEqual(158.15f, (double)sut.SugarChartMaximum, 0.001);
            }

            [Test]
            public void SugarChartMinimumAndMaximumReturn100WhenChartIsEmpty()
            {
                InitializeViewModel();
                Assert.AreEqual(100, sut.SugarChartMinimum);
                Assert.AreEqual(100, sut.SugarChartMaximum);
            }

            [TestCase(SugarUnit.mgdL)]
            [TestCase(SugarUnit.mmolL)]
            public void SugarChartAsTextReturnsTextualRepresentationOfChart(SugarUnit useSugarUnit)
            {
                sugar.BloodSugar = 100;
                settings.SugarUnit = useSugarUnit;
                string sugarUnit;
                if (useSugarUnit == SugarUnit.mgdL)
                    sugarUnit = Translations.BloodSugarMgdL;
                else
                    sugarUnit = Translations.BloodSugarMmolL;
                InitializeViewModel();
                var expected = Translations.EstimatedBloodSugar + Environment.NewLine;
                for (int i = 0; i < sut.SugarChart.Count; i++)
                {
                    expected += Environment.NewLine;
                    expected += sut.SugarChart[i].DateTime.ToString("t")
                        + "   " + sugarUnit.Replace("{0}", sut.SugarChart[i].BloodSugar.ToString());
                }
                Assert.IsNotEmpty(expected);
                var actual = sut.SugarChartAsText;
                Assert.AreEqual(expected, actual);
            }

            [Test]
            public void CalculationChangesMinimumAndMaximum()
            {
                var estimatedSugars = replacementAndEstimatedSugars.EstimatedSugars;
                InitializeViewModel();
                sut.ChangesProperty("SugarChartMinimum", () =>
                {
                    sut.ChangesProperty("SugarChartMaximum", () =>
                    {
                        sut.CurrentSugar.BloodSugar = "100";
                    });
                });
            }

            [Test]
            public void TombstoneAndUntombstoneCalculationResults()
            {
                stateProvider.State.Returns(new Dictionary<string, object>());
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                sut.Tombstone();
                Assert.IsFalse(stateProvider.State.Any(kvp => kvp.Value is Insulin),
                    "Don't wait for the runtime to serialize it.");
                CreateSut();
                InitializeViewModel();
                Assert.IsTrue(sut.IsCalculated);
                Assert.IsTrue(sut.IsCalculationIncomplete);
                Assert.IsNotEmpty(sut.Calculated.Text);
                Assert.AreEqual(3, sut.SugarChart.Count);
                Assert.AreEqual(100, sut.SugarChart[0].BloodSugar);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void CalculationWorksAfterTombstoneAndUntombstone(bool bolusWasEdited)
            {
                stateProvider.State.Returns(new Dictionary<string, object>());
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                if (bolusWasEdited)
                    sut.Subject.NormalBolus = "1";
                sut.Tombstone();
                CreateSut();
                facade.ClearReceivedCalls();
                InitializeViewModel();
                facade.DidNotReceive().GetReplacementAndEstimatedSugars(Arg.Any<Meal>(), Arg.Any<Insulin>(),
                    Arg.Any<Sugar>());
                ChooseCircumstance();
                facade.Received().GetReplacementAndEstimatedSugars(Arg.Any<Meal>(), Arg.Any<Insulin>(),
                    Arg.Any<Sugar>());
                CheckTheCalculationAndTheSugarChartAreThere();
            }

            [Test]
            public void CalculationDetailsAndCloseCalculationDetailsSetCalculationDetailsVisible()
            {
                var fixture = new Fixture();
                replacementAndEstimatedSugars.Replacement.Items = fixture.CreateMany<ReplacementItem>().ToList();
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsFalse(sut.CalculationDetailsVisible);
                sut.ChangesProperty("CalculationDetailsVisible", () =>
                {
                    sut.CalculationDetails();
                });
                Assert.IsTrue(sut.CalculationDetailsVisible);
                sut.ChangesProperty("CalculationDetailsVisible", () =>
                {
                    sut.CloseCalculationDetails();
                });
                Assert.IsFalse(sut.CalculationDetailsVisible);
            }

            [Test]
            public void CalculationDetailsPopulatesReplacementItems()
            {
                var fixture = new Fixture();
                var expected = fixture.CreateMany<ReplacementItem>().ToList();
                expected[1].Pattern.Insulin.InitializeCircumstances(new List<Guid>());
                expected[1].Alternatives[1].Insulin.InitializeCircumstances(new List<Guid>());
                expected[1].Pattern.Factor = 1.177F;
                expected[2].Alternatives.Clear();
                replacementAndEstimatedSugars.Replacement.Items = expected;
                InitializeViewModel();
                sut.CurrentSugar.BloodSugar = "100";
                Assert.IsEmpty(sut.ReplacementItems);
                sut.ChangesProperty("ReplacementItems", () =>
                {
                    sut.CalculationDetails();
                });
                var actual = sut.ReplacementItems;
                CheckPatternViewModel(expected[1].Pattern, actual[1].Pattern, alternatives: actual[1].Alternatives);
                CheckPatternViewModel(expected[1].Alternatives[1], actual[1].Alternatives[1],
                    alternatives: new List<PatternViewModel>());
                Assert.IsTrue(actual[1].Pattern.HasAlternatives);
                Assert.IsFalse(actual[2].Pattern.HasAlternatives);
                Assert.IsFalse(actual[1].Alternatives[1].HasAlternatives);
                Assert.AreEqual("118%", actual[1].Pattern.Factor);
            }

            [Test]
            public void CalculationDetailsThrowsExceptionIfNoCalculation()
            {
                InitializeViewModel();
                Assert.Throws<InvalidOperationException>(() => sut.CalculationDetails());
            }
        }

        public class SugarChartItemViewModelTests
        {
            [Test]
            public void ConvertsTimeToLocal()
            {
                var sugar = new Sugar { DateTime = DateTime.UtcNow };
                var sut = new InsulinEditingViewModel.SugarChartItemViewModel(sugar);
                Assert.AreEqual(sugar.DateTime.ToLocalTime(), sut.DateTime);
            }
        }
    }
}
