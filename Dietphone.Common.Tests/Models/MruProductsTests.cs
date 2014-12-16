using System;
using System.Collections.Generic;
using System.Linq;
using Dietphone.Models;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Dietphone.Common.Tests.Models
{
    public class MruProductsTests
    {
        List<Product> products;
        Factories factories;

        [SetUp]
        public void TestInitialize()
        {
            products = new Fixture().CreateMany<Product>(MruProductsImpl.MAX_COUNT + 1).ToList();
            factories = Substitute.For<Factories>();
            factories.Products.Returns(products);
        }

        [Test]
        public void Products()
        {
            var sut = new MruProductsImpl(new List<Guid> { products[1].Id, products[2].Id }, factories);
            var expected = new Product[] { products[1], products[2] };
            var actual = sut.Products;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ProductsWhenProductDoesNotExistSkipsIt()
        {
            var sut = new MruProductsImpl(new List<Guid> { Guid.NewGuid(), products[3].Id }, factories);
            var expected = new Product[] { products[3] };
            var actual = sut.Products;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AddProduct()
        {
            var sut = new MruProductsImpl(new List<Guid> { products[0].Id }, factories);
            sut.AddProduct(products[1]);
            var expected = new Product[] { products[1], products[0] };
            var actual = sut.Products;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AddProductWhenAlreadyHasMaxCountAndAddedNewProduct()
        {
            var initialProductIds = products.Take(MruProductsImpl.MAX_COUNT).Select(product => product.Id).ToList();
            var sut = new MruProductsImpl(initialProductIds, factories);
            sut.AddProduct(products[MruProductsImpl.MAX_COUNT]);
            var expected = products.Take(MruProductsImpl.MAX_COUNT - 1).ToList();
            expected.Insert(0, products[MruProductsImpl.MAX_COUNT]);
            var actual = sut.Products;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AddProductWhenAlreadyHasMaxCountAndAddedExistingProduct()
        {
            var initialProductIds = products.Take(MruProductsImpl.MAX_COUNT).Select(product => product.Id).ToList();
            var sut = new MruProductsImpl(initialProductIds, factories);
            sut.AddProduct(products[5]);
            var expected = products.Take(MruProductsImpl.MAX_COUNT).ToList();
            expected.Remove(products[5]);
            expected.Insert(0, products[5]);
            var actual = sut.Products;
            Assert.AreEqual(expected, actual);
        }
    }
}
