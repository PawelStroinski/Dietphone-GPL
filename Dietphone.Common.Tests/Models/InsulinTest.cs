using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dietphone.Models.Tests
{
    [TestClass]
    public class InsulinTest
    {
        [TestClass]
        public class InsulinCircumstances
        {
            private Mock<Factories> factories;
            private InsulinCircumstance circumstance;

            [TestInitialize()]
            public void TestInitialize()
            {
                factories = new Mock<Factories>();
                circumstance = new InsulinCircumstance { Id = Guid.NewGuid() };
                factories.Setup(f => f.InsulinCircumstances).Returns(new List<InsulinCircumstance>() { circumstance });
                factories.Setup(f => f.Finder).Returns(new FinderImpl(factories.Object));
            }

            [TestMethod]
            public void Can_Initialize_Circumstances_With_Empty()
            {
                var sut = new Insulin();
                var newCircumstances = new List<Guid>();
                sut.InitializeCircumstances(newCircumstances);
            }

            [TestMethod]
            public void Can_Initialize_Circumstances_With_Non_Empty()
            {
                var sut = new Insulin();
                var newCircumstances = new List<Guid>() { Guid.NewGuid() };
                sut.InitializeCircumstances(newCircumstances);
            }

            [ExpectedException(typeof(InvalidOperationException))]
            [TestMethod]
            public void Cannot_Initialize_Circumstances_Two_Times()
            {
                var sut = new Insulin();
                var newCircumstances = new List<Guid>();
                sut.InitializeCircumstances(newCircumstances);
                sut.InitializeCircumstances(newCircumstances);
            }

            [ExpectedException(typeof(InvalidOperationException))]
            [TestMethod]
            public void Cannot_Get_Circumstances_When_Not_Initialized()
            {
                var sut = new Insulin();
                var temp = sut.Circumstances.ToList();
            }

            [TestMethod]
            public void Can_Get_Circumstances_When_Initialized()
            {
                var defaultCircumstance = new InsulinCircumstance();
                factories.Setup(f => f.DefaultEntities.InsulinCircumstance).Returns(defaultCircumstance);
                var insulin = new Insulin();
                insulin.SetOwner(factories.Object);
                insulin.InitializeCircumstances(new List<Guid> { Guid.NewGuid(), circumstance.Id });
                var circumstances = insulin.Circumstances;
                Assert.AreEqual(2, circumstances.Count());
                Assert.AreSame(defaultCircumstance, circumstances.ElementAt(0));
                Assert.AreSame(circumstance, circumstances.ElementAt(1));
            }

            [ExpectedException(typeof(InvalidOperationException))]
            [TestMethod]
            public void Cannot_Add_Circumstance_When_Not_Initialized()
            {
                var insulin = new Insulin();
                insulin.SetOwner(factories.Object);
                insulin.AddCircumstance(circumstance);
            }

            [TestMethod]
            public void Can_Add_Circumstance_When_Initialized()
            {
                var insulin = new Insulin();
                insulin.SetOwner(factories.Object);
                insulin.InitializeCircumstances(new List<Guid>());
                insulin.AddCircumstance(circumstance);
                var circumstances = insulin.Circumstances;
                Assert.AreSame(circumstance, circumstances.Single());
            }

            [ExpectedException(typeof(InvalidOperationException))]
            [TestMethod]
            public void Cannot_Delete_Circumstance_When_Not_Initialized()
            {
                var insulin = new Insulin();
                insulin.SetOwner(factories.Object);
                insulin.DeleteCircumstance(circumstance);
            }

            [TestMethod]
            public void Can_Delete_Circumstance_When_Initialized()
            {
                var insulin = new Insulin();
                insulin.SetOwner(factories.Object);
                insulin.InitializeCircumstances(new List<Guid> { circumstance.Id });
                insulin.DeleteCircumstance(circumstance);
                var circumstances = insulin.Circumstances;
                Assert.AreEqual(0, circumstances.Count());
            }
        }
    }
}
