using System.Collections.Generic;
using Dietphone.Models;
using NSubstitute;
using NUnit.Framework;

namespace Dietphone.Common.Tests.Models
{
    public class ExportAndImportTests
    {
        private Factories factories;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            factories.Meals.Returns(new List<Meal>());
        }

        [Test]
        public void ImportsDefaultSettingWhenSettingNotExported()
        {
            factories.Settings.Returns(new Settings { SugarsAfterInsulinHours = 0 });
            var sut = new ExportAndImport(factories);
            var data = sut.Export();
            var removeThis = "<SugarsAfterInsulinHours>0</SugarsAfterInsulinHours>";
            Assert.IsTrue(data.Contains(removeThis), "This is requirement to perform test");
            data = data.Replace(removeThis, string.Empty);
            sut.Import(data);
            Assert.AreNotEqual(0, factories.Settings.SugarsAfterInsulinHours);
        }
    }
}
