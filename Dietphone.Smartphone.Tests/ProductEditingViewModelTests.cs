using System;
using System.Collections.Generic;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using Dietphone.Views;
using NSubstitute;
using NUnit.Framework;

namespace Dietphone.Smartphone.Tests
{
    public class ProductEditingViewModelTests : TestBase
    {
        private Factories factories;
        private MessageDialog messageDialog;
        private LearningCuAndFpu learningCuAndFpu;
        private ProductEditingViewModel sut;
        private Product product;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            messageDialog = Substitute.For<MessageDialog>();
            learningCuAndFpu = Substitute.For<LearningCuAndFpu>();
            sut = new ProductEditingViewModel(factories, new BackgroundWorkerSyncFactory(), messageDialog,
                learningCuAndFpu);
            sut.Navigator = Substitute.For<Navigator>();
            sut.Init(new ProductEditingViewModel.Navigation());
            product = new Product { Name = "Foo" };
            factories.Finder.FindProductById(Guid.Empty).Returns(product);
            factories.Categories.Returns(new List<Category> { new Category(), new Category() });
        }

        [Test]
        public void Messages()
        {
            Assert.AreEqual(Translations.AreYouSureYouWantToSaveThisProduct, sut.Messages.CannotSaveCaption);
        }

        [TestCase("foo")]
        [TestCase(null)]
        public void AddCategory(string name)
        {
            sut.Load();
            factories.CreateCategory().Returns(new Category());
            messageDialog.Input(Translations.Name, Translations.AddCategory).Returns(name);
            var beforeAddingEditingCategoryCalled = false;
            var afterAddedEditedCategoryCalled = false;
            sut.BeforeAddingEditingCategory += delegate { beforeAddingEditingCategoryCalled = true; };
            sut.AfterAddedEditedCategory += delegate { afterAddedEditedCategoryCalled = true; };
            sut.AddCategory.Call();
            factories.Received(name == null ? 0 : 1).CreateCategory();
            Assert.IsTrue(beforeAddingEditingCategoryCalled);
            Assert.AreEqual(name != null, afterAddedEditedCategoryCalled);
        }

        [TestCase("foo")]
        [TestCase(null)]
        public void EditCategory(string newName)
        {
            sut.Load();
            sut.CategoryName = "bar";
            var beforeAddingEditingCategoryCalled = false;
            var afterAddedEditedCategoryCalled = false;
            sut.BeforeAddingEditingCategory += delegate { beforeAddingEditingCategoryCalled = true; };
            sut.AfterAddedEditedCategory += delegate { afterAddedEditedCategoryCalled = true; };
            messageDialog.Input(Translations.Name, Translations.EditCategory, value: sut.CategoryName).Returns(newName);
            sut.EditCategory.Call();
            Assert.IsTrue(beforeAddingEditingCategoryCalled);
            Assert.AreEqual(newName != null, afterAddedEditedCategoryCalled);
            if (newName != null)
                Assert.AreEqual(newName, sut.CategoryName);
            else
                Assert.AreNotEqual(newName, sut.CategoryName);
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void DeleteCategory(bool otherProductsInCategory, bool confirmSetup)
        {
            sut.Load();
            factories.Finder.FindProductsByCategory(Guid.Empty).Returns(otherProductsInCategory
                ? new List<Product> { new Product() } : new List<Product>());
            var categoryDeleteCalled = false;
            var expected = sut.Categories.Count - 1;
            sut.CategoryDelete += (_, action) =>
            {
                var actualBefore = sut.Categories.Count;
                Assert.AreEqual(expected + 1, actualBefore);
                action();
                var actualAfter = sut.Categories.Count;
                Assert.AreEqual(expected, actualAfter);
                categoryDeleteCalled = true;
            };
            var confirmCalled = false;
            messageDialog.Confirm(string.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisCategory,
                sut.CategoryName), Translations.DeleteCategory).Returns(_ =>
                {
                    confirmCalled = true;
                    return confirmSetup;
                });
            sut.DeleteCategory.Call();
            messageDialog.Received(otherProductsInCategory ? 1 : 0)
                .Show(Translations.ThisCategoryIncludesOtherProducts, Translations.CannotDelete);
            Assert.AreNotEqual(otherProductsInCategory, confirmCalled);
            Assert.AreEqual(confirmCalled && confirmSetup, categoryDeleteCalled);
        }

        [Test]
        public void DeleteCategoryWhenCategoryDeleteEventNotHandled()
        {
            sut.Load();
            factories.Finder.FindProductsByCategory(Guid.Empty).Returns(new List<Product>());
            var expected = sut.Categories.Count - 1;
            messageDialog.Confirm(null, null).ReturnsForAnyArgs(true);
            sut.DeleteCategory.Call();
            var actual = sut.Categories.Count;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LearnCu()
        {
            sut.LearnCu.Call();
            learningCuAndFpu.Received().LearnCu();
        }

        [Test]
        public void LearnFpu()
        {
            sut.LearnFpu.Call();
            learningCuAndFpu.Received().LearnFpu();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DeleteAndSaveAndReturn(bool confirm)
        {
            factories.Products.Returns(new List<Product> { product });
            sut.Load();
            var subject = sut.Subject;
            messageDialog.Confirm(string.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisProduct,
                subject.Name), Translations.DeleteProduct).Returns(confirm);
            sut.DeleteAndSaveAndReturn();
            Assert.AreEqual(confirm ? 0 : 1, factories.Products.Count);
        }

        [Test]
        public void ShouldFocusName()
        {
            sut.Load();
            Assert.IsFalse(sut.ShouldFocusName);
            sut.Subject.Name = string.Empty;
            Assert.IsTrue(sut.ShouldFocusName);
        }
    }
}
