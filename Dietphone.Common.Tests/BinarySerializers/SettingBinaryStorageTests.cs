using System;
using System.Linq;
using NUnit.Framework;
using Dietphone.Models;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison.Fluent;

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
                .ShouldEqual(readedSettings);
        }
    }
}
