using NUnit.Framework;
using Dietphone.Models;
using Dietphone.Views;

namespace Dietphone.Core.Tests.Models
{
    public class InsulinCircumstanceTests
    {
        [Test]
        public void NameGetterUsesKind()
        {
            var sut = new InsulinCircumstance();
            sut.Kind = InsulinCircumstanceKind.Exercise;
            Assert.AreEqual(Translations.Exercise, sut.Name);
            sut.Kind = InsulinCircumstanceKind.Sickness;
            Assert.AreEqual(Translations.Sickness, sut.Name);
            sut.Kind = InsulinCircumstanceKind.Stress;
            Assert.AreEqual(Translations.Stress, sut.Name);
        }

        [Test]
        public void NameSetterChangesKind()
        {
            var sut = new InsulinCircumstance();
            sut.Kind = InsulinCircumstanceKind.Exercise;
            sut.Name = "foo";
            Assert.AreEqual(InsulinCircumstanceKind.Custom, sut.Kind);
            Assert.AreEqual("foo", sut.Name);
        }
    }
}
