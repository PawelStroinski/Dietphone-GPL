using System;
using System.Collections.Generic;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System.Linq;

namespace Dietphone.Common.Phone.Tests
{
    public class InsulinEditingViewModelTests
    {
        private Factories factories;
        private Navigator navigator;
        private StateProvider stateProvider;
        private InsulinEditingViewModel sut;
        private Insulin insulin;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            navigator = Substitute.For<Navigator>();
            stateProvider = Substitute.For<StateProvider>();
            sut = new InsulinEditingViewModel(factories);
            sut.Navigator = navigator;
            sut.StateProvider = stateProvider;
            insulin = new Fixture().Create<Insulin>();
            insulin.InitializeCircumstances(new List<Guid>());
        }

        private void InitializeViewModel()
        {
            factories.CreateInsulin().Returns(insulin);
            sut.Load();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FindAndCopyModel_And_MakeViewModel(bool editingExisting)
        {
            if (editingExisting)
            {
                navigator.GetInsulinIdToEdit().Returns(insulin.Id);
                factories.Finder.FindInsulinById(insulin.Id).Returns(insulin);
            }
            else
                factories.CreateInsulin().Returns(insulin);
            sut.Load();
            Assert.AreEqual(insulin.Id, sut.Insulin.Id);
        }

        [Test]
        public void CurrentSugarCanBeWrittenAndRead()
        {
            factories.Settings.Returns(new Settings());
            InitializeViewModel();
            sut.CurrentSugar.BloodSugar = "110";
            Assert.AreEqual("110", sut.CurrentSugar.BloodSugar);
        }

        //[Test]
        //public void InsulinCircumstancesReturnsCicumstanceViewModels()
        //{
        //    factories.InsulinCircumstances.Returns(new Fixture().CreateMany<InsulinCircumstance>(3).ToList());
        //    InitializeViewModel();
        //    var expected = factories.InsulinCircumstances;
        //    var actual = sut.Circumstances;
        //    Assert.AreEqual(expected.Select(circumstance => circumstance.Id).ToList(),
        //        actual.Select(circumstance => circumstance.Id).ToList());
        //}
    }
}
