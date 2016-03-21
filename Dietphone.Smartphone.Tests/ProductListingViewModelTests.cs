using Dietphone.Models;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace Dietphone.Smartphone.Tests
{
    public class ProductListingViewModelTests
    {
        [Test]
        public void Grouping()
        {
            var sut = new GroupingProductListingViewModel(Substitute.For<Factories>(),
                new BackgroundWorkerSyncFactory());
            Assert.IsNull(sut.Grouping.Groups);
        }
    }
}
