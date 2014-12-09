using NUnit.Framework;
using Dietphone.Models;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison.Fluent;
using System;

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
    }
}
