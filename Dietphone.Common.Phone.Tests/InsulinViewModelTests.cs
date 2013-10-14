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
        private InsulinViewModel sut;

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
        }

        [Test]
        public void TrivialProperties()
        {
            sut = new InsulinViewModel(insulin, factories, allCircumstances: null);
            Assert.AreEqual(insulin.Id, sut.Id);
            var universal = DateTime.UtcNow;
            sut.DateTime = universal;
            Assert.AreEqual(universal.ToLocalTime(), sut.DateTime);
            Assert.AreEqual(universal.ToLocalTime().Date, sut.DateOnly);
            Assert.AreEqual(universal.ToLocalTime().ToShortDateInAlternativeFormat()
                + " " + universal.ToLocalTime().ToShortTimeString(), sut.DateAndTime);
            Assert.AreEqual(universal.ToLocalTime().ToShortTimeString(), sut.Time);
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
            sut = new InsulinViewModel(insulin, factories, allCircumstances: null);
            sut.NormalBolus = "4";
            Assert.AreEqual("3", sut.NormalBolus);
            sut.SquareWaveBolus = "4";
            Assert.AreEqual("3", sut.SquareWaveBolus);
            sut.SquareWaveBolusHours = "9";
            Assert.AreEqual("8", sut.SquareWaveBolusHours);
        }

        //[Test]
        //public void CircumstancesGetterReturnsCicumstanceViewModels()
        //{
        //    var circumstanceIds = factories.InsulinCircumstances.Take(3)
        //        .Select(circumstance => circumstance.Id).ToList();
        //    var actual = sut.Circumstances;
        //    Assert.AreEqual(circumstanceIds, actual.Select(circumstance => circumstance.Id).ToList());
        //}

        //[Test]
        //public void CircumstancesGetterReusesOnceComputedResult()
        //{
        //    var actual1 = sut.Circumstances;
        //    var actual2 = sut.Circumstances;
        //    Assert.AreEqual(actual1, actual2);
        //    Assert.AreSame(actual1, actual2);
        //}

        //[Test]
        //public void CircumstancesSetterDoesNotChangeWhenAssignedValueIsNotChanged()
        //{
        //    var previous = new ObservableCollection<InsulinCircumstanceViewModel>(sut.Circumstances.ToList());
        //    sut.Circumstances = previous;
        //    Assert.AreEqual(previous.Select(circumstance => circumstance.Id),
        //        insulin.Circumstances.Select(circumstance => circumstance.Id));
        //}

        //[Test]
        //public void CircumstancesSetterAddsWhenAssignedValueHasNewItems()
        //{
        //    var previous = new ObservableCollection<InsulinCircumstanceViewModel>(sut.Circumstances.ToList());
        //    previous.Add(new InsulinCircumstanceViewModel(factories.InsulinCircumstances.Last(), factories));
        //    sut.Circumstances = previous;
        //    Assert.AreEqual(previous.Select(circumstance => circumstance.Id),
        //        insulin.Circumstances.Select(circumstance => circumstance.Id));
        //}

        //[Test]
        //public void CircumstancesSetterRemovesWhenAssignedValueHasRemovedItems()
        //{
        //    var previous = new ObservableCollection<InsulinCircumstanceViewModel>(sut.Circumstances.ToList());
        //    previous.Remove(previous.Last());
        //    sut.Circumstances = previous;
        //    Assert.AreEqual(previous.Select(circumstance => circumstance.Id),
        //        insulin.Circumstances.Select(circumstance => circumstance.Id));
        //}

        //[Test]
        //public void CircumstancesSetterThrowsExceptionWhenAssignedValueIsNull()
        //{
        //    Assert.Throws<NullReferenceException>(() =>
        //    {
        //        sut.Circumstances = null;
        //    });
        //}
    }
}
