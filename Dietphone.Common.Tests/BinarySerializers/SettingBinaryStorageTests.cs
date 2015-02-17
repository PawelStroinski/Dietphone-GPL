using NUnit.Framework;
using Dietphone.Models;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison.Fluent;
using System;
using System.Threading;
using System.Globalization;

namespace Dietphone.BinarySerializers.Tests
{
    public class SettingBinaryStorageTests : BinaryStorageTestsBase
    {
        [Test]
        public void Can_Write_And_Read()
        {
            var settingsToWrite = fixture.Create<Settings>();
            settingsToWrite.SugarUnit = SugarUnit.mmolL;
            settingsToWrite.Unit = Unit.Ounce;
            settingsToWrite.ShowWelcomeScreen = false;
            var storage = new SettingsBinaryStorage();
            var readSettings = WriteAndRead(storage, settingsToWrite);
            settingsToWrite.AsSource().OfLikeness<Settings>()
                .Without(settings => settings.MruProductIds)
                .ShouldEqual(readSettings);
            Assert.AreEqual(settingsToWrite.MruProductIds, readSettings.MruProductIds);
        }

        [Test]
        public void Default_SugarsAfterInsulinHours()
        {
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 3);
            Assert.AreEqual(4, readSettings.SugarsAfterInsulinHours);
        }

        [Test]
        public void Default_MaxBolus()
        {
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 3);
            Assert.AreEqual(5, readSettings.MaxBolus);
        }

        [Test]
        public void Default_Cloud()
        {
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 3);
            Assert.IsEmpty(readSettings.CloudSecret);
            Assert.IsEmpty(readSettings.CloudToken);
            Assert.AreEqual(DateTime.MinValue, readSettings.CloudExportDue);
        }

        [Test]
        public void Default_MruProductMaxCount()
        {
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 3);
            Assert.AreEqual(10, readSettings.MruProductMaxCount);
        }

        [TestCase("pl-PL", SugarUnit.mgdL)]
        [TestCase("en-GB", SugarUnit.mmolL)]
        [TestCase("en-US", SugarUnit.mgdL)]
        [TestCase("en-IE", SugarUnit.mmolL)]
        public void Default_SugarUnit(string culture, SugarUnit unit)
        {
            var thread = Thread.CurrentThread;
            thread.CurrentCulture = new CultureInfo(culture);
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 3);
            Assert.AreEqual(unit, readSettings.SugarUnit);
        }

        [TestCase("pl-PL", "pl-PL", "pl-PL")]
        [TestCase("en-GB", "en-US", "en-GB")]
        [TestCase("en-US", "en-US", "en-US")]
        [TestCase("en-IE", "en-US", "en-GB")]
        [TestCase("en-AU", "en-US", "en-US")]
        public void Default_Culture(string systemCulture, string expectedUiCulture, string expectedProductCulture)
        {
            var thread = Thread.CurrentThread;
            thread.CurrentCulture = new CultureInfo(systemCulture);
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 2);
            Assert.AreEqual(expectedUiCulture, readSettings.CurrentUiCulture);
            Assert.AreEqual(expectedProductCulture, readSettings.CurrentProductCulture);
        }

        [TestCase("pl-PL", Unit.Gram)]
        [TestCase("en-US", Unit.Ounce)]
        [TestCase("en-GB", Unit.Gram)]
        public void Default_Unit(string culture, Unit unit)
        {
            var thread = Thread.CurrentThread;
            thread.CurrentCulture = new CultureInfo(culture);
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 5);
            Assert.AreEqual(unit, readSettings.Unit);
        }

        [Test]
        public void Default_ShowWelcomeScreen()
        {
            var settingsToWrite = new Settings { ShowWelcomeScreen = false };
            var storage = new SettingsBinaryStorage();
            var readSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 7);
            Assert.IsTrue(readSettings.ShowWelcomeScreen);
        }

        [Test]
        public void Default_CuSugarsHours_And_FpuSugarsHours()
        {
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readedSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 8);
            Assert.AreEqual(3.25f, readedSettings.CuSugarsHoursToExcludingPlusOneSmoothing);
            Assert.AreEqual(1.75f, readedSettings.FpuSugarsHoursFromExcludingMinusOneSmoothing);
        }
    }
}
