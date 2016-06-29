using Dietphone.Views;
using NUnit.Framework;

namespace Dietphone.Models.Tests
{
    public class UnitTests
    {
        [Test]
        public void GetAbbreviation()
        {
            Assert.AreEqual(Translations.G, Unit.Gram.GetAbbreviation());
            Assert.AreEqual(Translations.Ml, Unit.Mililiter.GetAbbreviation());
            Assert.AreEqual(Translations.Serving, Unit.ServingSize.GetAbbreviation());
            Assert.AreEqual(Translations.Oz, Unit.Ounce.GetAbbreviation());
            Assert.AreEqual(Translations.Lb, Unit.Pound.GetAbbreviation());
        }

        [Test]
        public void AsAByte()
        {
            Assert.AreEqual(0, (byte)Unit.Gram);
            Assert.AreEqual(1, (byte)Unit.Mililiter);
            Assert.AreEqual(2, (byte)Unit.ServingSize);
            Assert.AreEqual(3, (byte)Unit.Ounce);
            Assert.AreEqual(4, (byte)Unit.Pound);
        }
    }
}
