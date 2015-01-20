using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using System;
using System.Linq;
using System.Threading;
using System.Globalization;

namespace Dietphone.Rarely.Phone.Tests
{
    public class SettingsViewModelTests
    {
        private static string mgdL = Dietphone.Views.Translations.MgdL;
        private static string mmolL = Dietphone.Views.Translations.MmolL;
        private Factories factories;
        private Settings settings;
        private SettingsViewModel sut;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            settings = new Settings();
            factories.Settings.Returns(settings);
            sut = new SettingsViewModel(factories);
        }

        [Test]
        public void AllSugarUnitsReturnsList()
        {
            Assert.AreEqual(new[] { mgdL, mmolL }, sut.AllSugarUnits);
        }

        [Test]
        public void SugarUnitCanBeGetAndSet()
        {
            Assert.AreEqual(mgdL, sut.SugarUnit);
            sut.SugarUnit = mmolL;
            Assert.AreEqual(mmolL, sut.SugarUnit);
            Assert.AreEqual(SugarUnit.mmolL, settings.SugarUnit);
            sut.ChangesProperty("SugarUnit", () => sut.SugarUnit = mgdL);
            Assert.AreEqual(SugarUnit.mgdL, settings.SugarUnit);
            sut.SugarUnit = string.Empty;
            Assert.AreEqual(mgdL, sut.SugarUnit);
        }

        [Test]
        public void MaxBolusCanBeGetAndSet()
        {
            settings.MaxBolus = 3;
            Assert.AreEqual(sut.MaxBolus.ToString(), sut.MaxBolus);
            sut.ChangesProperty("MaxBolus", () => sut.MaxBolus = "3.5");
            Assert.AreEqual(sut.MaxBolus.ToString(), sut.MaxBolus);
            Assert.AreEqual(3.5f, settings.MaxBolus);
            sut.MaxBolus = "0";
            Assert.AreEqual(0.1f, settings.MaxBolus);
            sut.MaxBolus = "5..";
            Assert.AreEqual(0.1f, settings.MaxBolus);
        }

        [Test]
        public void MruProductMaxCountCanBeGetAndSet()
        {
            settings.MruProductMaxCount = 15;
            Assert.AreEqual("15", sut.MruProductMaxCount);
            sut.ChangesProperty("MruProductMaxCount", () => sut.MruProductMaxCount = "5.5");
            Assert.AreEqual(5, settings.MruProductMaxCount);
            sut.MruProductMaxCount = "-1";
            Assert.AreEqual("0", sut.MruProductMaxCount);
            sut.MruProductMaxCount = "110";
            Assert.AreEqual(100, settings.MruProductMaxCount);
            sut.MruProductMaxCount = "1..";
            Assert.AreEqual(100, settings.MruProductMaxCount);
        }

        [Test]
        public void SugarsAfterInsulinHoursCanBeGetAndSet()
        {
            settings.SugarsAfterInsulinHours = 5;
            Assert.AreEqual("5", sut.SugarsAfterInsulinHours);
            sut.ChangesProperty("SugarsAfterInsulinHours", () => sut.SugarsAfterInsulinHours = "2.5");
            Assert.AreEqual(2, settings.SugarsAfterInsulinHours);
            sut.SugarsAfterInsulinHours = "-1";
            Assert.AreEqual(1, settings.SugarsAfterInsulinHours);
            sut.SugarsAfterInsulinHours = "20";
            Assert.AreEqual(12, settings.SugarsAfterInsulinHours);
            sut.SugarsAfterInsulinHours = "5..";
            Assert.AreEqual(12, settings.SugarsAfterInsulinHours);
        }

        [Test]
        public void UiCulturesAndProductCulturesAndUiCultureAndProductCulture()
        {
            var thread = Thread.CurrentThread;
            thread.CurrentCulture = new CultureInfo("en-US");
            sut = new SettingsViewModel(factories);
            Assert.AreEqual(sut.UiCultures.Count, sut.UiCultures.Distinct().Count());
            Assert.AreEqual(sut.ProductCultures.Count, sut.ProductCultures.Distinct().Count());
            Assert.IsTrue(sut.UiCultures
                .Count(item => item.ToLower().StartsWith("english")) == 1);
            Assert.IsTrue(sut.ProductCultures
                .Count(item => item.ToLower().StartsWith("english")) > 1);
            foreach (var culture in sut.UiCultures)
            {
                sut.UiCulture = culture;
                Assert.AreEqual(culture, sut.UiCulture);
            }
            foreach (var culture in sut.ProductCultures)
            {
                sut.ProductCulture = culture;
                Assert.AreEqual(culture, sut.ProductCulture);
            }
        }
    }
}
