using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dietphone.Models;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison.Fluent;

namespace Dietphone.BinarySerializers.Tests
{
    [TestClass]
    public class SugarBinaryStorageTests : BinaryStorageTestsBase
    {
        [TestMethod]
        public void Can_Write_And_Read()
        {
            var sugarToWrite = fixture.Create<Sugar>();
            var storage = new SugarBinaryStorage();
            var readSugar = WriteAndRead(storage, sugarToWrite);
            sugarToWrite.AsSource().OfLikeness<Sugar>()
                .ShouldEqual(readSugar);
        }
    }
}
