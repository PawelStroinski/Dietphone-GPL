﻿using System;
using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;

namespace Dietphone.Common.Phone.Tests
{
    public class SugarViewModelTests
    {
        [Test]
        public void WhenBloodSugarSet_ReturnsItsValue()
        {
            var factories = Substitute.For<Factories>();
            factories.Settings.Returns(new Settings());
            var sut = new SugarViewModel(new Models.Sugar(), factories);
            sut.BloodSugar = (100.10).ToString();
            Assert.AreEqual((100.10).ToString(), sut.BloodSugar);
        }

        [TestCase("31", SugarUnit.mmolL, "30")]
        [TestCase("541", SugarUnit.mgdL, "540")]
        public void WhenCurrentSugarSetToOutsideConstainsTheValueIsConstrained(
            string enteredValue, SugarUnit sugarUnit, string expectedValue)
        {
            var factories = Substitute.For<Factories>();
            var settings = new Settings { SugarUnit = sugarUnit };
            factories.Settings.Returns(settings);
            var sut = new SugarViewModel(new Models.Sugar(), factories);
            sut.BloodSugar = enteredValue;
            Assert.AreEqual(expectedValue, sut.BloodSugar);
        }

        [Test]
        public void DateTimeTest()
        {
            var factories = Substitute.For<Factories>();
            var sut = new SugarViewModel(new Models.Sugar(), factories);
            var universal = DateTime.UtcNow;
            sut.ChangesProperty("DateTime", () =>
            {
                sut.DateTime = universal;
            });
            Assert.AreEqual(universal.ToLocalTime(), sut.DateTime);
        }

        [Test]
        public void Text()
        {
            var factories = Substitute.For<Factories>();
            factories.Settings.Returns(new Settings());
            var sut = new SugarViewModel(new Models.Sugar { BloodSugar = 120 }, factories);
            Assert.AreEqual("120 mg/dL", sut.Text);
            factories.Settings.SugarUnit = SugarUnit.mmolL;
            sut.BloodSugar = "5.5";
            Assert.AreEqual(sut.BloodSugar + " mmol/L", sut.Text);
            sut.BloodSugar = "";
            Assert.AreEqual("", sut.Text);
        }
    }
}
