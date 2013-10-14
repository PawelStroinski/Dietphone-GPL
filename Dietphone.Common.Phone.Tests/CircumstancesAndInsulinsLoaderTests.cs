using System;
using System.Linq;
using Dietphone.Models;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Dietphone.Common.Phone.Tests
{
    public class CircumstancesAndInsulinsLoaderTests
    {
        private Factories factories;
        private Fixture fixture;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            fixture = new Fixture();
            factories.InsulinCircumstances.Returns(fixture.CreateMany<InsulinCircumstance>().ToList());
            viewModel = new InsulinListingViewModel(factories);
        }

        [Test]
        public void LoadsCircumstancesAndReturnsSorted()
        {
            var sut = new InsulinListingViewModel.CircumstancesAndInsulinsLoader(factories, true);
            var expected = factories.InsulinCircumstances
                .OrderBy(circumstance => circumstance.Name);
            var actual = sut.Circumstances;
            Assert.AreEqual(expected.Select(circumstance => circumstance.Id).ToList(),
                actual.Select(circumstance => circumstance.Id).ToList());
        }

        [Test]
        public void LoadsCircumstancesAndReturnsUnsorted()
        {
            var sut = new InsulinListingViewModel.CircumstancesAndInsulinsLoader(factories, false);
            var expected = factories.InsulinCircumstances;
            var actual = sut.Circumstances;
            Assert.AreEqual(expected.Select(circumstance => circumstance.Id).ToList(),
                actual.Select(circumstance => circumstance.Id).ToList());
        }

        [Test]
        public void LoadsInsulins()
        {
            factories.Insulins.Returns(fixture.CreateMany<Insulin>(1).ToList());
            var sut = new SutAccessor(viewModel);
            sut.LoadSynchronously();
            var expected = factories.Insulins.First().Id;
            var actual = viewModel.Insulins.First().Id;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ProvidesAllCircumstancesToInsulinViewModel()
        {
            factories.Insulins.Returns(fixture.CreateMany<Insulin>(1).ToList());
            var sut = new SutAccessor(viewModel);
            sut.LoadSynchronously();
            Assert.AreEqual(sut.Circumstances, viewModel.Insulins.First().AllCircumstances());
        }

        [Test]
        public void MakesDatesAndSortsInsulins()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            factories.Insulins.Returns(fixture.CreateMany<Insulin>(100).ToList());
            factories.Insulins[0].DateTime = yesterday;
            factories.Insulins[1].DateTime = today;
            for (int i = 2; i < 100; i++)
                factories.Insulins[i].DateTime = today.AddDays(-i);
            var sut = new SutAccessor(viewModel);
            sut.LoadSynchronously();
            Assert.AreEqual(today, viewModel.Dates[0].Date);
            Assert.AreEqual(yesterday, viewModel.Dates[1].Date);
            Assert.AreEqual(today, viewModel.Insulins[0].Date.Date);
            Assert.AreEqual(yesterday, viewModel.Insulins[1].Date.Date);
            Assert.IsFalse(viewModel.Dates[viewModel.Dates.Count - 2].IsGroupOfOlder);
            Assert.IsTrue(viewModel.Dates[viewModel.Dates.Count - 1].IsGroupOfOlder);
        }

        class SutAccessor : InsulinListingViewModel.CircumstancesAndInsulinsLoader
        {
            public SutAccessor(InsulinListingViewModel viewModel)
                : base(viewModel)
            {
            }

            public void LoadSynchronously()
            {
                base.DoWork();
                base.WorkCompleted();
            }
        }

        public InsulinListingViewModel viewModel { get; set; }
    }
}
