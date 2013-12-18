using System;
using System.Collections.Generic;
using System.Linq;
using Dietphone.Models;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Dietphone.Common.Phone.Tests
{
    public class ProductViewModelTests
    {
        private Factories factories;
        private Fixture fixture;
        private ProductListingViewModel viewModel;
        private List<CategoryViewModel> categories;
        private Product product;
        private CategoryViewModel overrideCategory;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            fixture = new Fixture();
            factories.Categories.Returns(fixture.CreateMany<Category>().ToList());
            categories = factories.Categories.Select(category => new CategoryViewModel(category, factories)).ToList();
            product = new Product { CategoryId = factories.Categories.First().Id };
            product.SetOwner(factories);
            overrideCategory = new CategoryViewModel(new Category(), factories);
        }

        [Test]
        public void Category()
        {
            var sut = new ProductViewModel(product);
            sut.Categories = categories;
            Assert.AreSame(categories.First(), sut.Category);           
        }

        [Test]
        public void OverrideCategoryReturnsItAsCategory()
        {
            var sut = new ProductViewModel(product);
            sut.Categories = categories;            
            sut.OverrideCategory = overrideCategory;
            Assert.AreSame(overrideCategory, sut.Category);
        }

        [Test]
        public void OverrideCategoryPreventsCategorySet()
        {
            var sut = new ProductViewModel(product);
            sut.Categories = categories;
            sut.OverrideCategory = overrideCategory;
            Assert.Throws<InvalidOperationException>(() => sut.Category = categories.First());
        }
    }
}
