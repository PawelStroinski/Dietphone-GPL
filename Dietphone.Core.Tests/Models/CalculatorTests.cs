using NUnit.Framework;
using System;

namespace Dietphone.Models.Tests
{
    public class CalculatorTests
    {
        [Test]
        public void Can_Count_Integers()
        {
            var calculator = new Calculator()
            {
                Protein = 10,
                Fat = 100,
                DigestibleCarbs = 1000
            };
            Assert.AreEqual(4940, calculator.Energy);
            Assert.AreEqual(100, calculator.Cu);
            Assert.AreEqual(9.4f, calculator.Fpu);
        }

        [Test]
        public void Can_Count_Fractions()
        {
            var calculator = new Calculator()
            {
                Protein = 1.5f,
                Fat = 2.6f,
                DigestibleCarbs = 3.7f
            };
            Assert.AreEqual(44, calculator.Energy);
            Assert.AreEqual(0.4F, calculator.Cu);
            Assert.AreEqual(0.3F, calculator.Fpu);
        }

        [Test]
        public void Accepts_Zeros()
        {
            var calculator = new Calculator()
            {
                Protein = 0,
                Fat = 0,
                DigestibleCarbs = 0
            };
            Assert.AreEqual(0, calculator.Energy);
            Assert.AreEqual(0, calculator.Cu);
            Assert.AreEqual(0, calculator.Fpu);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Nulls_Generate_Exception_For_Energy()
        {
            var calculator = new Calculator();
            var dummy = calculator.Energy;
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Nulls_Generate_Exception_For_Cu()
        {
            var calculator = new Calculator();
            var dummy = calculator.Cu;
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Nulls_Generate_Exception_For_Fpu()
        {
            var calculator = new Calculator();
            var dummy = calculator.Fpu;
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Protein_Null_Generates_Exception()
        {
            var calculator = new Calculator()
            {
                DigestibleCarbs = 0,
                Fat = 0
            };
            var dummy = calculator.Energy;
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Fat_Null_Generates_Exception()
        {
            var calculator = new Calculator()
            {
                DigestibleCarbs = 0,
                Protein = 0
            };
            var dummy = calculator.Energy;
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void DigestibleCarbs_Null_Generates_Exception()
        {
            var calculator = new Calculator()
            {
                Fat = 0,
                Protein = 0
            };
            var dummy = calculator.Energy;
        }
    }
}
