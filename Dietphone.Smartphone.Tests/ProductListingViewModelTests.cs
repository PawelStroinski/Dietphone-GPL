using System.Collections.Generic;
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

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void HasMru(bool mruProductsPresent, bool addMru)
        {
            var factories = Substitute.For<Factories>();
            factories.Products.Returns(new List<Product>());
            factories.Categories.Returns(new List<Category>());
            factories.MruProducts.Products.Returns(new List<Product>());
            factories.Settings.Returns(new Settings());
            factories.CreateCategory().Returns(new Category());
            if (mruProductsPresent)
                factories.MruProducts.Products.Add(new Product());
            var sut = new ProductListingViewModel(factories, new BackgroundWorkerSyncFactory());
            sut.AddMru = addMru;
            sut.Load();
            Assert.AreEqual(mruProductsPresent && addMru, sut.HasMru);
        }
    }
}
