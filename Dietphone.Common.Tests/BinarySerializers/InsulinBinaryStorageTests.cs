using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dietphone.Models;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison.Fluent;

namespace Dietphone.BinarySerializers.Tests
{
    [TestClass]
    public class InsulinBinaryStorageTests : BinaryStorageTestsBase
    {
        [TestMethod]
        public void Can_Write_And_Read()
        {
            var insulinToWrite = fixture.Create<Insulin>();
            var circumstances = fixture.CreateMany<Guid>().ToList();
            insulinToWrite.InitializeCircumstances(circumstances);
            var storage = new InsulinBinaryStorage();
            var readInsulin = WriteAndRead(storage, insulinToWrite);
            insulinToWrite.AsSource().OfLikeness<Insulin>()
                .Without(p => p.Circumstances)
                .ShouldEqual(readInsulin);
            Assert.IsTrue(Enumerable.SequenceEqual(insulinToWrite.ReadCircumstances(), 
                readInsulin.ReadCircumstances()));
        }
    }
}
