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
    public class CircumstancesAndInsulinsAndSugarsLoaderTests
    {
        private Factories factories;
        private Fixture fixture;
        private InsulinAndSugarListingViewModel viewModel;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            fixture = new Fixture();
            factories.InsulinCircumstances.Returns(fixture.CreateMany<InsulinCircumstance>().ToList());
            factories.Sugars.Returns(new List<Sugar>().ToList());
            var sugarEditing = Substitute.For<SugarEditingViewModel>();
            viewModel = new InsulinAndSugarListingViewModel(factories, new BackgroundWorkerSyncFactory(), sugarEditing);
        }

        [Test]
        public void LoadsCircumstancesAndReturnsSorted()
        {
            var sut = new InsulinAndSugarListingViewModel.CircumstancesAndInsulinsAndSugarsLoader(factories, true,
                new BackgroundWorkerSyncFactory());
            var expected = factories.InsulinCircumstances
                .OrderBy(circumstance => circumstance.Name);
            var actual = sut.Circumstances;
            Assert.AreEqual(expected.Select(circumstance => circumstance.Id).ToList(),
                actual.Select(circumstance => circumstance.Id).ToList());
        }

        [Test]
        public void LoadsCircumstancesAndReturnsUnsorted()
        {
            var sut = new InsulinAndSugarListingViewModel.CircumstancesAndInsulinsAndSugarsLoader(factories, false,
                new BackgroundWorkerSyncFactory());
            var expected = factories.InsulinCircumstances;
            var actual = sut.Circumstances;
            Assert.AreEqual(expected.Select(circumstance => circumstance.Id).ToList(),
                actual.Select(circumstance => circumstance.Id).ToList());
        }

        [Test]
        public void LoadsInsulinsAndSugars()
        {
            factories.Insulins.Returns(fixture.CreateMany<Insulin>(2).ToList());
            factories.Sugars.Returns(fixture.CreateMany<Sugar>(2).ToList());
            var date1 = DateTime.Today;
            var date2 = DateTime.Today.AddHours(3);
            factories.Sugars.First().DateTime = date2;
            factories.Insulins.First().DateTime = date2;
            factories.Sugars.ElementAt(1).DateTime = date1;
            factories.Insulins.ElementAt(1).DateTime = date1;
            var sut = new InsulinAndSugarListingViewModel.CircumstancesAndInsulinsAndSugarsLoader(viewModel);
            sut.LoadAsync();
            var expected = new DateTime[] { date1, date1, date2, date2 };
            var actual = viewModel.InsulinsAndSugars.Select(insulinAndSugar => insulinAndSugar.DateTime).ToList();
            Assert.AreEqual(expected, actual);
            var enumerator = viewModel.InsulinsAndSugars.GetEnumerator();
            enumerator.MoveNext();
            Assert.IsInstanceOf<SugarViewModel>(enumerator.Current);
            enumerator.MoveNext();
            Assert.IsInstanceOf<InsulinViewModel>(enumerator.Current);
            enumerator.MoveNext();
            Assert.IsInstanceOf<SugarViewModel>(enumerator.Current);
            enumerator.MoveNext();
            Assert.IsInstanceOf<InsulinViewModel>(enumerator.Current);
        }

        [Test]
        public void ProvidesAllCircumstancesToInsulinViewModel()
        {
            factories.Insulins.Returns(fixture.CreateMany<Insulin>(1).ToList());
            var sut = new InsulinAndSugarListingViewModel.CircumstancesAndInsulinsAndSugarsLoader(viewModel);
            sut.LoadAsync();
            Assert.AreEqual(sut.Circumstances, viewModel.InsulinsAndSugars.Cast<InsulinViewModel>()
                .First().AllCircumstances());
        }

        [Test]
        public void MakesDatesAndSortsInsulinsAndSugars()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            factories.Insulins.Returns(fixture.CreateMany<Insulin>(50).ToList());
            factories.Sugars.Returns(fixture.CreateMany<Sugar>(1).ToList());
            factories.Insulins[0].DateTime = yesterday;
            factories.Insulins[1].DateTime = today;
            for (int i = 2; i < 49; i++)
                factories.Insulins[i].DateTime = today.AddDays(-i);
            factories.Insulins[49].DateTime = factories.Insulins[48].DateTime.AddMinutes(2);
            factories.Sugars.First().DateTime = factories.Insulins[48].DateTime;
            var sut = new InsulinAndSugarListingViewModel.CircumstancesAndInsulinsAndSugarsLoader(viewModel);
            sut.LoadAsync();
            Assert.AreEqual(today, viewModel.Dates[0].Date);
            Assert.AreEqual(yesterday, viewModel.Dates[1].Date);
            Assert.AreEqual(today, viewModel.InsulinsAndSugars[0].Date.Date);
            Assert.AreEqual(yesterday, viewModel.InsulinsAndSugars[1].Date.Date);
            Assert.IsFalse(viewModel.Dates[viewModel.Dates.Count - 2].IsGroupOfOlder);
            Assert.IsTrue(viewModel.Dates[viewModel.Dates.Count - 1].IsGroupOfOlder);
            Assert.IsTrue(viewModel.InsulinsAndSugars.IndexOf(
                viewModel.InsulinsAndSugars.First(vm => vm.DateTime == factories.Insulins[48].DateTime)) >
                viewModel.InsulinsAndSugars.IndexOf(
                    viewModel.InsulinsAndSugars.First(vm => vm.DateTime == factories.Insulins[49].DateTime)));
            Assert.IsInstanceOf<SugarViewModel>(viewModel.InsulinsAndSugars
                .ElementAt(viewModel.InsulinsAndSugars.Count - 2));
        }
    }
}
