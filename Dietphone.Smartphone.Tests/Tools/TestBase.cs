using MvvmCross.Test.Core;
using NUnit.Framework;

namespace Dietphone.Smartphone.Tests
{
    public class TestBase : MvxIoCSupportingTest
    {
        [SetUp]
        public void TestBaseInitialize()
        {
            base.Setup();
        }
    }
}
