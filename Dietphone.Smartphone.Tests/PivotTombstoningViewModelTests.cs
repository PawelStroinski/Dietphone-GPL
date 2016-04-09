using Dietphone.ViewModels;
using NUnit.Framework;

namespace Dietphone.Smartphone.Tests
{
    public class PivotTombstoningViewModelTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void PivotDerivedProperties(int pivot)
        {
            var sut = new PivotTombstoningViewModel();
            sut.Pivot = -1;
            sut.ChangesProperty("FirstPivot", () =>
            {
                sut.ChangesProperty("SecondPivot", () =>
                {
                    sut.ChangesProperty("ThirdPivot", () =>
                    {
                        sut.Pivot = pivot;
                    });
                });
            });
            Assert.AreEqual(pivot == 0, sut.FirstPivot);
            Assert.AreEqual(pivot == 1, sut.SecondPivot);
            Assert.AreEqual(pivot == 2, sut.ThirdPivot);
        }
    }
}
