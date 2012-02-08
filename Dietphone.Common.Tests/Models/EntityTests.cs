using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dietphone.Models.Tests
{
    [TestClass]
    public class EntityTests
    {
        private Factories owner;

        [TestInitialize()]
        public void TestInitialize()
        {
            owner = new FactoriesImpl();
        }

        [TestMethod]
        public void Can_Set_Owner()
        {
            var entity = new Entity();
            entity.SetOwner(owner);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void Cannot_Set_Same_Owner_Two_Times()
        {
            var entity = new Entity();
            entity.SetOwner(owner);
            entity.SetOwner(owner);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void Cannot_Change_Owner()
        {
            var entity = new Entity();
            var otherOwner = new FactoriesImpl();
            entity.SetOwner(owner);
            entity.SetOwner(otherOwner);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void Cannot_Remove_Owner()
        {
            var entity = new Entity();
            entity.SetOwner(owner);
            entity.SetOwner(null);
        }

        [ExpectedException(typeof(NullReferenceException))]
        [TestMethod]
        public void Cannot_Set_Null_Owner()
        {
            var entity = new Entity();
            entity.SetOwner(null);
        }
    }
}
