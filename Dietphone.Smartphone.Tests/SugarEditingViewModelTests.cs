using System.Collections.Generic;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace Dietphone.Common.Phone.Tests
{
    public class SugarEditingViewModelTests
    {
        private Sut sut;
        private SugarViewModel sugarViewModel;

        [SetUp]
        public void TestInitialize()
        {
            sut = new Sut();
            var sugar = new Sugar();
            var factories = Substitute.For<Factories>();
            sugarViewModel = new SugarViewModel(sugar, factories);
            var stateProvider = Substitute.For<StateProvider>();
            stateProvider.State.Returns(new Dictionary<string, object>());
            factories.Settings.Returns(new Settings());
            sut.StateProvider = stateProvider;
        }

        [Test]
        public void TombstoneAndUntombstoneAndClearTombstoning()
        {
            sut.Show(sugarViewModel);
            sugarViewModel.BloodSugar = "150";
            sut.Tombstone();
            sugarViewModel.BloodSugar = "120";
            sut.Untombstone();
            Assert.AreEqual("150", sugarViewModel.BloodSugar);
            sugarViewModel.BloodSugar = "120";
            sut.ClearTombstoning();
            sut.Untombstone();
            Assert.AreEqual("120", sugarViewModel.BloodSugar);
        }

        public class Sut : SugarEditingViewModel
        {
            public new void Untombstone()
            {
                base.Untombstone();
            }

            public new void ClearTombstoning()
            {
                base.ClearTombstoning();
            }
        }
    }
}
