using System;
using System.Linq;
using NUnit.Framework;
using Dietphone.Models;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison.Fluent;

namespace Dietphone.BinarySerializers.Tests
{
    public class SugarBinaryStorageTests : BinaryStorageTestsBase
    {
        [Test]
        public void Can_Write_And_Read()
        {
            var sugarToWrite = fixture.Create<Sugar>();
            var storage = new SugarBinaryStorage();
            var readSugar = WriteAndRead(storage, sugarToWrite);
            sugarToWrite.AsSource().OfLikeness<Sugar>()
                .Without(s => s.BloodSugarInMgdL)
                .ShouldEqual(readSugar);
        }
    }
}
