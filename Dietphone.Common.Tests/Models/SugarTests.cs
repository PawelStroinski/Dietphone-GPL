using Dietphone.Models;
using Dietphone.Models.Tests;
using NUnit.Framework;

namespace Dietphone.Common.Tests.Models
{
    public class SugarTests
    {
        [TestCase(100, SugarUnit.mgdL, 100)]
        [TestCase(5.6f, SugarUnit.mmolL, 101)]
        [TestCase(5.0f, SugarUnit.mmolL, 90)]
        [TestCase(5.3f, SugarUnit.mmolL, 95)]
        [TestCase(6.7f, SugarUnit.mmolL, 121)]
        public void BloodSugarInMgdL(float from, SugarUnit unit, float expectedTo)
        {
            var factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            factories.Settings.SugarUnit = unit;
            var sut = new Sugar { BloodSugar = from };
            sut.SetOwner(factories);
            Assert.AreEqual(expectedTo, sut.BloodSugarInMgdL);
        }
    }
}
