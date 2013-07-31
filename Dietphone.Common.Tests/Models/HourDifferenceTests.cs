using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Dietphone.Models.Tests
{
    public class HourDifferenceTests
    {
        [TestCase("00:00", "00:00", 0)]
        [TestCase("01:00", "00:00", 1)]
        [TestCase("00:00", "01:00", 1)]
        [TestCase("23:00", "00:00", 1)]
        [TestCase("23:00", "01:00", 2)]
        [TestCase("13:00", "11:00", 2)]
        [TestCase("00:00", "23:00", 1)]
        [TestCase("00:30", "00:00", 0)]
        [TestCase("00:31", "00:00", 1)]
        [TestCase("23:25", "01:55", 2)]
        [TestCase("00:00", "12:00", 12)]
        [TestCase("12:35", "01:05", 11)]
        [TestCase("07:00", "13:30", 6)]
        [TestCase("14:00", "01:00", 11)]
        [TestCase("15:00", "20:00", 5)]
        public void GetDifference(string left, string right, int expected)
        {
            var sut = new HourDifferenceImpl();
            var actual = sut.GetDifference(TimeSpan.Parse(left), TimeSpan.Parse(right));
            Assert.AreEqual(expected, actual);
        }
    }
}
