using Dietphone.Views;
using NUnit.Framework;

namespace Dietphone.Models.Tests
{
    public class UnitAbbreviationsTests
    {
        [Test]
        public void GetAbbreviation()
        {
            Assert.AreEqual(Translations.G, Unit.Gram.GetAbbreviation());
            Assert.AreEqual(Translations.Ml, Unit.Mililiter.GetAbbreviation());
            Assert.AreEqual(Translations.Serving, Unit.ServingSize.GetAbbreviation());
        }
    }
}
