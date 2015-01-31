using NUnit.Framework;
using NSubstitute;
using Dietphone.Tools;
using System;

namespace Dietphone.BinarySerializers.Tests
{
    public class PhoneOutputStreamTests
    {
        private FileFactory fileFactory;
        private PhoneOutputStream sut;
        private File foo;
        private File fooTemp1;
        private File fooTemp2;
        private File fooTemp3;
        private System.IO.Stream fooTemp1WritingStream;
        private System.IO.Stream fooTemp1ReadingStream;

        [SetUp]
        public void TestInitialize()
        {
            fileFactory = Substitute.For<FileFactory>();
            foo = Substitute.For<File>();
            fileFactory.Create("foo").Returns(foo);
            fooTemp1 = Substitute.For<File>();
            fileFactory.Create(Arg.Is<string>(arg => arg.StartsWith("foo_temp1_"))).Returns(fooTemp1);
            fooTemp2 = Substitute.For<File>();
            fileFactory.Create(Arg.Is<string>(arg => arg.StartsWith("foo_temp2_"))).Returns(fooTemp2);
            fooTemp3 = Substitute.For<File>();
            fileFactory.Create(Arg.Is<string>(arg => arg.StartsWith("foo_temp3_"))).Returns(fooTemp3);
            fooTemp1WritingStream = Substitute.For<System.IO.Stream>();
            fooTemp1.GetWritingStream().Returns(fooTemp1WritingStream);
            fooTemp1ReadingStream = Substitute.For<System.IO.Stream>();
            fooTemp1.GetReadingStream().Returns(fooTemp1ReadingStream);
            foo.Exists.Returns(true);
            fooTemp1ReadingStream.Length.Returns(1000);
            sut = new PhoneOutputStream(fileFactory, "foo");
        }

        [Test]
        public void Stream()
        {
            Assert.AreSame(fooTemp1WritingStream, sut.Stream);
        }

        public class Commit : PhoneOutputStreamTests
        {
            [Test]
            public void ChecksTheSizeOfFooTemp1AndThenMovesIt()
            {
                fooTemp1
                    .WhenForAnyArgs(FooTemp1 => FooTemp1.GetReadingStream())
                    .Do(_ => fooTemp1.DidNotReceiveWithAnyArgs().MoveTo(null));
                sut.Commit(1000);
                fooTemp1.Received().GetReadingStream();
                fooTemp1.Received().MoveTo(fooTemp2);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void MovesFooTemp1AndThenMovesFoo(bool fooExists)
            {
                fooTemp1
                    .WhenForAnyArgs(FooTemp1 => FooTemp1.MoveTo(null))
                    .Do(_ => foo.DidNotReceiveWithAnyArgs().MoveTo(null));
                foo.Exists.Returns(fooExists);
                sut.Commit(1000);
                fooTemp1.Received().MoveTo(fooTemp2);
                if (fooExists)
                    foo.Received().MoveTo(fooTemp3);
                else
                    foo.DidNotReceiveWithAnyArgs().MoveTo(null);
            }

            [Test]
            public void MovesFooAndThenMovesFooTemp2()
            {
                foo
                    .WhenForAnyArgs(Foo => Foo.MoveTo(null))
                    .Do(_ => fooTemp2.DidNotReceiveWithAnyArgs().MoveTo(null));
                sut.Commit(1000);
                foo.Received().MoveTo(fooTemp3);
                fooTemp2.Received().MoveTo(foo);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void MovesFooTemp2AndThenDeletesFooTemp3(bool fooExists)
            {
                fooTemp2
                    .WhenForAnyArgs(FooTemp2 => FooTemp2.MoveTo(null))
                    .Do(_ => fooTemp3.DidNotReceive().Delete());
                foo.Exists.Returns(fooExists);
                sut.Commit(1000);
                fooTemp2.Received().MoveTo(foo);
                if (fooExists)
                    fooTemp3.Received().Delete();
                else
                    fooTemp3.DidNotReceive().Delete();
            }

            [Test]
            public void WhenTheSizeOfFooTemp1IsNotRightDoesNotMoveOrDeleteFooButThrows()
            {
                Assert.Throws<InvalidOperationException>(() => sut.Commit(1001));
                foo.DidNotReceiveWithAnyArgs().MoveTo(null);
                foo.DidNotReceive().Delete();
            }

            [Test]
            public void WhenMovingFooTemp1ThrowsDoesNotMoveOrDeleteFoo()
            {
                fooTemp1
                    .WhenForAnyArgs(FooTemp1 => FooTemp1.MoveTo(null))
                    .Do(_ => { throw new ArgumentException(); });
                Assert.Throws<ArgumentException>(() => sut.Commit(1000));
                foo.DidNotReceiveWithAnyArgs().MoveTo(null);
                foo.DidNotReceive().Delete();
            }

            [TestCase(true)]
            [TestCase(false)]
            public void WhenMovingFooTemp2ThrowsMovesFooTemp3Back(bool fooExists)
            {
                fooTemp2
                    .WhenForAnyArgs(FooTemp2 => FooTemp2.MoveTo(null))
                    .Do(_ => { throw new ArgumentException(); });
                foo.Exists.Returns(fooExists);
                Assert.Throws<ArgumentException>(() => sut.Commit(1000));
                if (fooExists)
                    fooTemp3.Received().MoveTo(foo);
                else
                    fooTemp3.DidNotReceiveWithAnyArgs().MoveTo(null);
                fooTemp3.DidNotReceive().Delete();
            }
        }
    }
}
