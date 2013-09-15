using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dietphone.Models;
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
        private InsulinEditingViewModel sut;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            navigator = Substitute.For<Navigator>();
            sut = new InsulinEditingViewModel(factories);
            sut.Navigator = navigator;
        }

        [Test]
        public void FindAndCopyModel_WhenEditingExisting()
        {
            var insulin = new Fixture().Create<Insulin>();
            navigator.GetInsulinIdToEdit().Returns(insulin.Id);
            //factories.Finder.FindInsulinById(
        }
    }
}
