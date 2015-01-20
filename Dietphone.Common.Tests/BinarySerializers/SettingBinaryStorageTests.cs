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
            var storage = new SettingsBinaryStorage();
            var readedSettings = WriteAndRead(storage, settingsToWrite);
            settingsToWrite.AsSource().OfLikeness<Settings>()
                .Without(settings => settings.MruProductIds)
                .ShouldEqual(readedSettings);
            Assert.AreEqual(settingsToWrite.MruProductIds, readedSettings.MruProductIds);
        }

        [Test]
        public void Default_SugarsAfterInsulinHours()
        {
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readedSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 3);
            Assert.AreEqual(4, readedSettings.SugarsAfterInsulinHours);
        }

        [Test]
        public void Default_MaxBolus()
        {
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readedSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 3);
            Assert.AreEqual(5, readedSettings.MaxBolus);
        }

        [Test]
        public void Default_Cloud()
        {
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readedSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 3);
            Assert.IsEmpty(readedSettings.CloudSecret);
            Assert.IsEmpty(readedSettings.CloudToken);
            Assert.AreEqual(DateTime.MinValue, readedSettings.CloudExportDue);
        }

        [Test]
        public void Default_MruProductMaxCount()
        {
            var settingsToWrite = new Settings();
            var storage = new SettingsBinaryStorage();
            var readedSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 3);
            Assert.AreEqual(10, readedSettings.MruProductMaxCount);
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
            var readedSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 3);
            Assert.AreEqual(unit, readedSettings.SugarUnit);
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
            var readedSettings = WriteAndRead(storage, settingsToWrite, overrideVersion: 2);
            Assert.AreEqual(expectedUiCulture, readedSettings.CurrentUiCulture);
            Assert.AreEqual(expectedProductCulture, readedSettings.CurrentProductCulture);
        }
    }
}
