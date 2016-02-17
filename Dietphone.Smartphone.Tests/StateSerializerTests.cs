using System;
using System.Collections.Generic;
using Dietphone.Tools;
using NUnit.Framework;

namespace Dietphone.Smartphone.Tests
{
    public class StateSerializerTests
    {
        [Test]
        public void Indexer()
        {
            var sut = new StateSerializer(new Dictionary<string, string>());
            sut["foo"] = "bar";
            Assert.AreEqual("bar", sut["foo"]);
        }

        [Test]
        public void ValueTypes()
        {
            var sut = new StateSerializer(new Dictionary<string, string>());
            var someDate = DateTime.Now.AddDays(1);
            sut["foo"] = someDate;
            Assert.AreEqual(someDate, sut["foo"]);
            double floatingPointNumber = 1.5;
            sut["foo"] = floatingPointNumber;
            Assert.AreEqual(floatingPointNumber, sut["foo"]);
            int integerNumber = 2;
            sut["foo"] = integerNumber;
            Assert.AreEqual(integerNumber, sut["foo"]);
            var booleanValue = true;
            sut["foo"] = booleanValue;
            Assert.AreEqual(booleanValue, sut["foo"]);
            var someGuid = Guid.NewGuid();
            sut["foo"] = someGuid;
            Assert.AreEqual(someGuid, sut["foo"]);
        }

        [Test]
        public void ComplexType()
        {
            var sut = new StateSerializer(new Dictionary<string, string>());
            TestField = "bar";
            sut["foo"] = this;
            Assert.AreEqual("bar", ((StateSerializerTests)sut["foo"]).TestField);
        }

        [Test]
        public void ContainsKey()
        {
            var sut = new StateSerializer(new Dictionary<string, string>());
            Assert.IsFalse(sut.ContainsKey("foo"));
            sut["foo"] = "bar";
            Assert.IsTrue(sut.ContainsKey("foo"));
        }

        [Test]
        public void Remove()
        {
            var sut = new StateSerializer(new Dictionary<string, string>());
            sut["foo"] = "bar";
            Assert.IsTrue(sut.Remove("foo"));
            Assert.IsFalse(sut.ContainsKey("foo"));
        }

        public string TestField { get; set; }
    }
}
