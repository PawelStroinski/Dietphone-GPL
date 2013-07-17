using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dietphone.Models;
using Ploeh.AutoFixture;
using Dietphone.BinarySerializers;
using System.IO;
using Ploeh.SemanticComparison.Fluent;

namespace Dietphone.Common.Tests.BinarySerializers
{
    [TestClass]
    public class InsulinBinaryStorageTests
    {
        [TestMethod]
        public void Can_Write_And_Read()
        {
            var fixture = new Fixture();
            var insulinToWrite = fixture.Create<Insulin>();
            var circumstances = fixture.CreateMany<Guid>().ToList();
            insulinToWrite.InitializeCircumstances(circumstances);
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            var storage = new InsulinBinaryStorage();
            storage.WriteItem(writer, insulinToWrite);
            var insulinToRead = new Insulin();
            var reader = new BinaryReader(stream);
            stream.Position = 0;
            storage.ReadItem(reader, insulinToRead);
            insulinToWrite.AsSource().OfLikeness<Insulin>()
                .Without(p => p.Circumstances)
                .ShouldEqual(insulinToRead);
            Assert.IsTrue(Enumerable.SequenceEqual(insulinToWrite.ReadCircumstances(), 
                insulinToRead.ReadCircumstances()));
        }
    }
}
