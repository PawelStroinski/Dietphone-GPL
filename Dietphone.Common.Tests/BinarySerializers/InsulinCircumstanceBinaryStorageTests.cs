using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dietphone.Models;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison.Fluent;

namespace Dietphone.BinarySerializers.Tests
{
    [TestClass]
    public class InsulinCircumstanceBinaryStorageTests : BinaryStorageTestsBase
    {
        [TestMethod]
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
