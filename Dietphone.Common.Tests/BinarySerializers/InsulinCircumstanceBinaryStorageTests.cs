using System;
using System.Linq;
using NUnit.Framework;
using Dietphone.Models;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison.Fluent;

namespace Dietphone.BinarySerializers.Tests
{
    public class InsulinCircumstanceBinaryStorageTests : BinaryStorageTestsBase
    {
        [Test]
        public void Can_Write_And_Read()
        {
            var circumstanceToWrite = fixture.Create<InsulinCircumstance>();
            var storage = new InsulinCircumstanceBinaryStorage();
            var readCircumstance = WriteAndRead(storage, circumstanceToWrite);
            circumstanceToWrite.AsSource().OfLikeness<InsulinCircumstance>()
                .ShouldEqual(readCircumstance);
        }
    }
}
