using System;
using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Dietphone.Tools;
using NSubstitute;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dietphone.Common.Phone.Tests
{
    public class InsulinViewModelTests
    {
        private Insulin insulin;
        private Factories factories;
        private IList<InsulinCircumstanceViewModel> allCircumstances;

        [SetUp]
        public void TestInitialize()
        {
            insulin = new Fixture().Create<Insulin>();
            factories = Substitute.For<Factories>();
            factories.Finder.Returns(new FinderImpl(factories));
            insulin.SetOwner(factories);
            var settings = new Settings { MaxBolus = 3 };
            factories.Settings.Returns(settings);
            factories.InsulinCircumstances.Returns(new Fixture().CreateMany<InsulinCircumstance>(5).ToList());
            var circumstanceIds = factories.InsulinCircumstances.Take(3)
                .Select(circumstance => circumstance.Id).ToList();
            insulin.InitializeCircumstances(circumstanceIds);
            allCircumstances = factories.InsulinCircumstances
                .Select(circumstance => new InsulinCircumstanceViewModel(circumstance, factories)).ToList();
        }

        [Test]
        public void TrivialProperties()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            Assert.AreEqual(insulin.Id, sut.Id);
            var universal = DateTime.UtcNow;
            sut.ChangesProperty("DateTime", () =>
            {
                sut.DateTime = universal;
            });
            Assert.AreEqual(universal.ToLocalTime(), sut.DateTime);
            Assert.AreEqual(universal.ToLocalTime().Date, sut.DateOnly);
            Assert.AreEqual(universal.ToLocalTime().ToShortDateInAlternativeFormat()
                + " " + universal.ToLocalTime().ToShortTimeString(), sut.DateAndTime);
            Assert.AreEqual(universal.ToLocalTime().ToShortTimeString(), sut.Time);
            Assert.AreEqual(universal.ToLocalTime().ToString("dddd") + ", " + sut.DateAndTime, sut.LongDateAndTime);
            Assert.AreEqual(insulin.Note, sut.Note);
            var note = new Fixture().Create<string>();
            sut.Note = note;
            Assert.AreEqual(note, insulin.Note);
            Assert.AreEqual(insulin.NormalBolus.ToStringOrEmpty(), sut.NormalBolus);
            sut.NormalBolus = "1.5";
            Assert.AreEqual(1.5f, insulin.NormalBolus);
            Assert.AreEqual(insulin.SquareWaveBolus.ToStringOrEmpty(), sut.SquareWaveBolus);
            sut.SquareWaveBolus = "2.5";
            Assert.AreEqual(2.5f, insulin.SquareWaveBolus);
            Assert.AreEqual(insulin.SquareWaveBolusHours.ToStringOrEmpty(), sut.SquareWaveBolusHours);
            sut.SquareWaveBolusHours = "3.5";
            Assert.AreEqual(3.5f, insulin.SquareWaveBolusHours);
        }

        [Test]
        public void Constraints()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            sut.NormalBolus = "4";
            Assert.AreEqual("3", sut.NormalBolus);
            sut.SquareWaveBolus = "4";
            Assert.AreEqual("3", sut.SquareWaveBolus);
            sut.SquareWaveBolusHours = "9";
            Assert.AreEqual("8", sut.SquareWaveBolusHours);
        }

        [Test]
        public void CircumstancesGetterReturnsSubsetOfCicumstanceViewModelsProvidedInConstructor()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            var expected = allCircumstances.Take(3);
            var actual = sut.Circumstances;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CircumstancesGetterReusesOnceComputedResult()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            var actual1 = sut.Circumstances;
            var actual2 = sut.Circumstances;
            Assert.AreEqual(actual1, actual2);
            Assert.AreSame(actual1, actual2);
        }

        [Test]
        public void CircumstancesSetterDoesNotChangeWhenAssignedValueIsNotChanged()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            var previous = sut.Circumstances;
            sut.Circumstances = previous.ToList();
            Assert.AreEqual(previous, sut.Circumstances);
            Assert.AreSame(previous, sut.Circumstances);
        }

        [Test]
        public void CircumstancesSetterInvalidatesGetterWhenAssignedValueIsChanged()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            var previous = sut.Circumstances;
            var newValue = previous.ToList();
            newValue.Add(allCircumstances.Last());
            sut.Circumstances = newValue;
            Assert.AreEqual(newValue, sut.Circumstances);
            Assert.AreNotSame(previous, sut.Circumstances);
        }

        [Test]
        public void CircumstancesSetterAddsToModelWhenAssignedValueHasNewItems()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            var previous = sut.Circumstances;
            var newValue = previous.ToList();
            newValue.Add(allCircumstances.Last());
            sut.Circumstances = newValue;
            Assert.AreEqual(newValue.Select(circumstance => circumstance.Id),
                insulin.Circumstances.Select(circumstance => circumstance.Id));
        }

        [Test]
        public void CircumstancesSetterRemovesFromModelWhenAssignedValueHasRemovedItems()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            var previous = sut.Circumstances;
            var newValue = previous.ToList();
            newValue.Remove(newValue.Last());
            sut.Circumstances = newValue;
            Assert.AreEqual(newValue.Select(circumstance => circumstance.Id),
                insulin.Circumstances.Select(circumstance => circumstance.Id));
        }

        [Test]
        public void CircumstancesSetterThrowsExceptionWhenAssignedValueIsNull()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            Assert.Throws<NullReferenceException>(() =>
            {
                sut.Circumstances = null;
            });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CircumstancesSetterThrowsIfAssignedListIsTheSameInstance()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            var newValue = sut.Circumstances;
            sut.Circumstances = newValue;
        }

        [Test]
        public void CircumstancesSetterRaisesOnPropertyChangedOnlyWhenChangeWasMade()
        {
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            sut.ChangesProperty("Circumstances", () =>
            {
                var newValue = sut.Circumstances.ToList();
                newValue.Add(allCircumstances.Last());
                sut.Circumstances = newValue;
            });
            sut.NotChangesProperty("Circumstances", () =>
            {
                sut.Circumstances = sut.Circumstances.ToList();
            });
        }

        [Test]
        public void TextAndText2()
        {
            insulin = new Insulin { Note = string.Empty };
            var sut = new InsulinViewModel(insulin, factories, allCircumstances: allCircumstances);
            sut.NormalBolus = "1";
            Assert.AreEqual("1 U", sut.Text);
            Assert.IsEmpty(sut.Text2);
            sut.SquareWaveBolus = "2";
            Assert.AreEqual("1 U 2 U for ? h", sut.Text);
            Assert.IsEmpty(sut.Text2);
            sut.SquareWaveBolusHours = "3";
            Assert.AreEqual("1 U 2 U for 3 h", sut.Text);
            Assert.IsEmpty(sut.Text2);
            sut.NormalBolus = "";
            Assert.AreEqual("2 U for 3 h", sut.Text);
            Assert.IsEmpty(sut.Text2);
            sut.Note = "note";
            Assert.AreEqual("2 U for 3 h", sut.Text);
            Assert.AreEqual("note", sut.Text2);
            sut.NormalBolus = "1";
            sut.SquareWaveBolus = "";
            Assert.AreEqual("1 U", sut.Text);
            Assert.AreEqual("note", sut.Text2);
        }
    }
}
