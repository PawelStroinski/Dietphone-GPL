using System;
using NUnit.Framework;

namespace Dietphone.Models.Tests
{
    public class EntityTests
    {
        private Factories owner;

        [SetUp]
        public void TestInitialize()
        {
            owner = new FactoriesImpl();
        }

        [Test]
        public void Can_Set_Owner()
        {
            var entity = new Entity();
            entity.SetOwner(owner);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Cannot_Set_Same_Owner_Two_Times()
        {
            var entity = new Entity();
            entity.SetOwner(owner);
            entity.SetOwner(owner);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Cannot_Change_Owner()
        {
            var entity = new Entity();
            var otherOwner = new FactoriesImpl();
            entity.SetOwner(owner);
            entity.SetOwner(otherOwner);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Cannot_Remove_Owner()
        {
            var entity = new Entity();
            entity.SetOwner(owner);
            entity.SetOwner(null);
        }

        [ExpectedException(typeof(NullReferenceException))]
        [Test]
        public void Cannot_Set_Null_Owner()
        {
            var entity = new Entity();
            entity.SetOwner(null);
        }
    }
}
