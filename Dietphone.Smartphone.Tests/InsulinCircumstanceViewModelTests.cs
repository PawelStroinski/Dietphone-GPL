using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dietphone.Models;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace Dietphone.Common.Phone.Tests
{
    public class InsulinCircumstanceViewModelTests
    {
        [Test]
        public void TrivialProperites()
        {
            var model = new InsulinCircumstance();
            var sut = new InsulinCircumstanceViewModel(model, Substitute.For<Factories>());
            model.Id = Guid.NewGuid();
            Assert.AreEqual(model.Id, sut.Id);
            sut.Name = "name";
            Assert.AreEqual("name", sut.Name);
            Assert.AreEqual(sut.Name, sut.ToString());
        }
    }
}
