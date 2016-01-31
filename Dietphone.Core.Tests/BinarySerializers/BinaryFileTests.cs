using System;
using Dietphone.Models;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;

namespace Dietphone.BinarySerializers.Tests
{
    public class BinaryFileTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void WriteFileDoesCommitOnlyIfThereWasNoError(bool error)
        {
            var streamProvider = Substitute.For<BinaryStreamProvider>();
            var outputStream = Substitute.For<OutputStream>();
            outputStream.Stream.Returns(new MemoryStream());
            streamProvider.GetOutputStream(Arg.Any<string>()).Returns(outputStream);
            var sut = new Sut();
            sut.StreamProvider = streamProvider;
            sut.Error = error;
            if (error)
            {
                Assert.Throws<ArgumentException>(() => sut.InvokeWriteFile());
                outputStream.DidNotReceiveWithAnyArgs().Commit(0);
            }
            else
            {
                sut.InvokeWriteFile();
                outputStream.Received().Commit(21);
            }
        }

        public class Sut : BinaryFile<Meal>
        {
            public bool Error;

            protected override string FileName
            {
                get { return "foo"; }
            }

            protected override byte WritingVersion
            {
                get { return 1; }
            }

            public override void WriteItem(BinaryWriter writer, Meal item)
            {
                if (Error)
                    throw new ArgumentException();
                else
                    writer.Write(Guid.Empty);
            }

            public override void ReadItem(BinaryReader reader, Meal item)
            {
            }

            public void InvokeWriteFile()
            {
                WriteFile(new List<Meal> { new Meal() });
            }
        }
    }
}
