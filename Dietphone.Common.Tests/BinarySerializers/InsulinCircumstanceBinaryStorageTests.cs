using System;
using System.Linq;
using NUnit.Framework;
using Dietphone.Models;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison.Fluent;
using Moq;
using System.Collections.Generic;
using System.IO;
using Dietphone.Views;

namespace Dietphone.BinarySerializers.Tests
{
    public class InsulinCircumstanceBinaryStorageTests : BinaryStorageTestsBase
    {
        [TestCase(InsulinCircumstanceKind.Custom)]
        [TestCase(InsulinCircumstanceKind.Exercise)]
        public void Can_Write_And_Read(InsulinCircumstanceKind kind)
        {
            var circumstanceToWrite = fixture.Create<InsulinCircumstance>();
            circumstanceToWrite.Kind = kind;
            var storage = new InsulinCircumstanceBinaryStorage();
            var readedCircumstance = WriteAndRead(storage, circumstanceToWrite);
            circumstanceToWrite.AsSource().OfLikeness<InsulinCircumstance>()
                .ShouldEqual(readedCircumstance);
        }

        [Test]
        public void DoesNotWriteNameIfKindIsNotCustom()
        {
            var circumstanceToWrite = new InsulinCircumstance();
            circumstanceToWrite.Kind = InsulinCircumstanceKind.Exercise;
            var storage = new InsulinCircumstanceBinaryStorage();
            var stream = new NonDisposableMemoryStream();
            var streamProvider = new Mock<BinaryStreamProvider>();
            streamProvider.Setup(p => p.GetOutputStream(It.IsAny<string>())).Returns(new OutputStreamStub(stream));
            storage.StreamProvider = streamProvider.Object;
            storage.Save(new List<InsulinCircumstance> { circumstanceToWrite });
            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                Assert.IsFalse(result.Contains(Translations.Exercise));
            }
            stream.Dispose();
        }
    }
}
