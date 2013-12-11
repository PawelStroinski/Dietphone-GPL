using System;
using Dietphone.ViewModels;
using NUnit.Framework;

namespace Dietphone.Common.Phone.Tests
{
    public class ViewModelWithDateAndTextTests
    {
        [Test]
        public void FilterIn()
        {
            var sut = new Sut();
            Assert.IsTrue(sut.FilterIn("foo"));
            Assert.IsFalse(sut.FilterIn("z"));
        }

        class Sut : ViewModelWithDateAndText
        {
            public override string Text
            {
                get { return "Foo bar"; }
            }

            public override DateTime DateTime
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }
    }
}
