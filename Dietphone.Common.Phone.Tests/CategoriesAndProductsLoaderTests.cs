using System;
using System.Collections.Generic;
using System.Linq;
using Dietphone.Models;
using Dietphone.ViewModels;
using Dietphone.Views;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Dietphone.Common.Phone.Tests
{
    public class CategoriesAndProductsLoaderTests
    {
        private Factories factories;
        private Fixture fixture;
        private ProductListingViewModel viewModel;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            fixture = new Fixture();
            factories.Finder.Returns(new FinderImpl(factories));
            factories.CreateCategory().Returns(new Category());
            factories.Categories.Returns(fixture.CreateMany<Category>().ToList());
            factories.Products.Returns(fixture.CreateMany<Product>(5).ToList());
            foreach (var product in factories.Products)
            {
                product.CategoryId = factories.Categories[1].Id;
                product.SetOwner(factories);
            }
            factories.MruProducts.Returns(new MruProducts(new List<Guid>(), factories));
            viewModel = new ProductListingViewModel(factories, new BackgroundWorkerSyncFactory());
        }

        [Test]
        public void WhenMruProductsAreNotPresentDoesNotAddMruCategory()
        {
            var sut = new ProductListingViewModel.CategoriesAndProductsLoader(viewModel, true);
            sut.LoadAsync();
            Assert.AreEqual(factories.Categories.Count, sut.Categories.Count);
        }

        [Test]
        public void WhenCreatedWithoutViewModelDoesNotAddMruCategory()
        {
            factories.MruProducts.AddProduct(factories.Products.First());
            var sut = new ProductListingViewModel.CategoriesAndProductsLoader(factories,
                new BackgroundWorkerSyncFactory());
            Assert.AreEqual(factories.Categories.Count, sut.Categories.Count);
        }

        [Test]
        public void WhenFlagIsFalseDoesNotAddMruCategory()
        {
            factories.MruProducts.AddProduct(factories.Products.First());
            var sut = new ProductListingViewModel.CategoriesAndProductsLoader(viewModel, false);
            sut.LoadAsync();
            Assert.AreEqual(factories.Categories.Count, sut.Categories.Count);
        }

        [Test]
        public void WhenMruProductsArePresentDoesAddMruCategory()
        {
            factories.MruProducts.AddProduct(factories.Products.First());
            var sut = new ProductListingViewModel.CategoriesAndProductsLoader(viewModel, true);
            sut.LoadAsync();
            Assert.AreEqual(factories.Categories.Count + 1, sut.Categories.Count);
            Assert.AreEqual(Translations.RecentlyUsed, sut.Categories.First().Name);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ProductListingViewModelForwardsFlag(bool addMru)
        {
            var expectedCategoriesCount = addMru ? factories.Categories.Count + 1 : factories.Categories.Count;
            factories.MruProducts.AddProduct(factories.Products.First());
            if (addMru)
                viewModel.AddMru = true;
            viewModel.Load();
            Assert.AreEqual(expectedCategoriesCount, viewModel.Categories.Count);
            viewModel.Refresh();
            Assert.AreEqual(expectedCategoriesCount, viewModel.Categories.Count);
        }

        [Test]
        public void AddsMruProductWithMruCategory()
        {
            factories.MruProducts.AddProduct(factories.Products.First());
            var sut = new ProductListingViewModel.CategoriesAndProductsLoader(viewModel, true);
            sut.LoadAsync();
            Assert.AreEqual(factories.Products.Count + 1, viewModel.Products.Count);
            var mruProducts = viewModel.Products
                .Where(product => product.Product == factories.Products.First()).ToList();
            Assert.AreEqual(2, mruProducts.Count);
            Assert.AreNotEqual(viewModel.Categories.First(), mruProducts[0].Category);
            Assert.AreEqual(viewModel.Categories.First(), mruProducts[1].Category);
            Assert.AreEqual(mruProducts[0].WidthOfFilledCuRect, mruProducts[1].WidthOfFilledCuRect);
        }

        [Test]
        public void AddsAllMruProducts()
        {
            for (int i = 0; i < 2; i++)
                factories.MruProducts.AddProduct(factories.Products[i]);
            var sut = new ProductListingViewModel.CategoriesAndProductsLoader(viewModel, true);
            sut.LoadAsync();
            Assert.AreEqual(factories.Products.Count + 2, viewModel.Products.Count);
            Assert.AreEqual(factories.Products, viewModel.Products.Select(vm => vm.Product).Distinct());
        }
    }
}
