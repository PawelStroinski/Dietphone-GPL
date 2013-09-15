using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;

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
            var state = new Dictionary<string, object>();
            stateProvider.State.Returns(state);
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
        public void TombstoneModel()
        {
            factories.CreateInsulin().Returns(insulin);
            sut.Load();
            sut.Tombstone();
            Assert.AreEqual(insulin.Serialize(string.Empty), stateProvider.State["INSULIN"]);
        }

        [Test]
        public void UntombstoneModel()
        {
            factories.CreateInsulin().Returns(insulin);
            var tombstoned = new Fixture().Create<Insulin>();
            tombstoned.Id = insulin.Id;
            stateProvider.State["INSULIN"] = tombstoned.Serialize(string.Empty);
            sut.Load();
            Assert.AreEqual(tombstoned.Note, sut.Insulin.Insulin.Note);
        }
    }
}
