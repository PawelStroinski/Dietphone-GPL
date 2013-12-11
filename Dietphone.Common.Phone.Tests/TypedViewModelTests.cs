using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;

namespace Dietphone.Common.Phone.Tests
{
    public class TypedViewModelTests
    {
        private InsulinViewModel insulin;
        private SugarViewModel sugar;

        [SetUp]
        public void TestInitialize()
        {
            insulin = new InsulinViewModel(new Insulin(), Substitute.For<Factories>(), null);
            sugar = new SugarViewModel(new Sugar(), Substitute.For<Factories>());
        }

        [Test]
        public void IsInsulin()
        {
            Assert.IsTrue(insulin.IsInsulin);
            Assert.IsFalse(sugar.IsInsulin);
        }

        [Test]
        public void IsSugar()
        {
            Assert.IsFalse(insulin.IsSugar);
            Assert.IsTrue(sugar.IsSugar);
        }
    }
}
