using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Dietphone.Models.Tests
{
    [TestClass]
    public class FinderTests
    {
        private Mock<Factories> factories;

        [TestInitialize()]
        public void TestInitialize()
        {
            factories = new Mock<Factories>();
        }

        [TestMethod]
        public void FindInsulinCircumstanceById()
        {
            var expected = new InsulinCircumstance { Id = Guid.NewGuid() };
            factories.Setup(f => f.InsulinCircumstances).Returns(new List<InsulinCircumstance> { expected });
            var finder = new FinderImpl(factories.Object);
            var actual = finder.FindInsulinCircumstanceById(expected.Id);
            Assert.AreSame(expected, actual);
        }
    }
}
