using System;
using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Dietphone.Tools;
using NSubstitute;

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
            sut = new InsulinViewModel(insulin, factories);
            var settings = new Settings { MaxBolus = 3 };
            factories.Settings.Returns(settings);
        }

        [Test]
        public void TrivialProperties()
        {
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
            sut.NormalBolus = "4";
            Assert.AreEqual("3", sut.NormalBolus);
            sut.SquareWaveBolus = "4";
            Assert.AreEqual("3", sut.SquareWaveBolus);
            sut.SquareWaveBolusHours = "9";
            Assert.AreEqual("8", sut.SquareWaveBolusHours);
        }
    }
}
