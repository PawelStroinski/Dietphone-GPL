using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using System.Linq;
using Dietphone.Tools;
using System.Threading;
using System;

namespace Dietphone.Rarely.Phone.Tests
{
    public class ExportAndImportViewModelTests
    {
        private Factories factories;
        private CloudProviderFactory cloudProviderFactory;
        private CloudProvider cloudProvider;
        private Settings settings;
        private Vibration vibration;
        private ExportAndImportViewModel sut;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            cloudProviderFactory = Substitute.For<CloudProviderFactory>();
            cloudProvider = Substitute.For<CloudProvider>();
            cloudProviderFactory.Create().Returns(cloudProvider);
            settings = new Settings();
            factories.Settings.Returns(settings);
            vibration = Substitute.For<Vibration>();
            sut = new ExportAndImportViewModel(factories, cloudProviderFactory, vibration);
        }

        public class ExportToCloud : ExportAndImportViewModelTests
        {
            [TestCase(true, "foo", "")]
            [TestCase(true, "", "bar")]
            [TestCase(true, "foo", "bar")]
            [TestCase(false, "", "")]
            public void IfItHasATokenThenItInvokesConfirmExportToCloudDeactivationOtherwiseItShowsTheTokenAcquirePage(
                bool hasAToken, string secret, string token)
            {
                settings.CloudSecret = secret;
                settings.CloudToken = token;
                var confirmed = false;
                var navigatedTo = string.Empty;
                sut.ConfirmExportToCloudDeactivation += (_, __) => { confirmed = true; };
                sut.NavigateInBrowser += (_, url) => { navigatedTo = url; };
                cloudProvider.GetTokenAcquiringUrl(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL).Returns("go");
                sut.ExportToCloud();
                Thread.Sleep(10); // Not ideal but a try to use BackgroundWorkerSync failed because sut is not in a PCL
                Assert.AreEqual(hasAToken, confirmed);
                Assert.AreEqual(!hasAToken, sut.BrowserVisible);
                Assert.AreEqual(hasAToken ? string.Empty : "go", navigatedTo);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void IfExportToCloudDeactivationWasConfirmedThenItRemovesTheTokenInformation(bool confirmed)
            {
                settings.CloudSecret = "foo";
                settings.CloudToken = "bar";
                sut.ConfirmExportToCloudDeactivation += (_, eventArgs) => { eventArgs.Confirm = confirmed; };
                sut.ExportToCloud();
                Assert.AreEqual(confirmed ? string.Empty : "foo", settings.CloudSecret);
                Assert.AreEqual(confirmed ? string.Empty : "bar", settings.CloudToken);
            }

            [TestCase(false, "http://foo")]
            [TestCase(false, null)]
            [TestCase(true, ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL)]
            public void IfBrowserIsNavigatingToTokenAcquiringCallbackUrlThenStoresTheToken(bool store, string url)
            {
                sut.ExportToCloud();
                Thread.Sleep(10);
                cloudProvider.GetAcquiredToken().Returns(new CloudToken { Secret = "foo", Token = "bar" });
                sut.BrowserVisible = true;
                var successful = false;
                sut.ExportToCloudActivationSuccessful += (_, __) => { successful = true; };
                sut.BrowserIsNavigating(url == null ? null : url.ToUpper() + "?");
                Thread.Sleep(10);
                if (store)
                {
                    Assert.AreEqual("foo", settings.CloudSecret);
                    Assert.AreEqual("bar", settings.CloudToken);
                }
                else
                    cloudProvider.DidNotReceive().GetAcquiredToken();
                Assert.AreNotEqual(store, sut.BrowserVisible);
                Assert.AreEqual(store, successful);
            }

            [Test]
            public void UsesSingleInstanceOfTokenProvider()
            {
                sut.ExportToCloud();
                Thread.Sleep(10);
                sut.BrowserIsNavigating(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL);
                Thread.Sleep(10);
                Assert.AreEqual(1, cloudProviderFactory.ReceivedCalls().Count());
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void IfBrowserIsNavigatingIsCalledButExportToCloudWasNotCalledThrowsException()
            {
                sut.BrowserIsNavigating(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL);
                Thread.Sleep(10);
            }

            [TestCase("foo")]
            [TestCase("")]
            public void Vibrates(string token)
            {
                settings.CloudToken = token;
                sut.ExportToCloud();
                vibration.Received().VibrateOnButtonPress();
            }

            [TestCase(true, "foo", "", true)]
            [TestCase(true, "", "bar", false)]
            [TestCase(true, "foo", "bar", false)]
            [TestCase(false, "", "", false)]
            public void IsExportToCloudActive(bool expected, string secret, string token, bool confirmedDeactivation)
            {
                var propertyName = "IsExportToCloudActive";
                settings.CloudSecret = secret;
                settings.CloudToken = token;
                Assert.AreEqual(expected, sut.IsExportToCloudActive);
                sut.ConfirmExportToCloudDeactivation += (_, eventArgs) => { eventArgs.Confirm = confirmedDeactivation; };
                sut.ChangesProperty(propertyName, () => sut.ExportToCloud());
                if (!expected)
                {
                    Thread.Sleep(10);
                    cloudProvider.GetAcquiredToken().Returns(new CloudToken { Secret = "foo", Token = "bar" });
                    sut.ChangesProperty(propertyName, () =>
                    {
                        sut.BrowserIsNavigating(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL);
                        Thread.Sleep(10);
                    });
                }
            }
        }

        [Test]
        public void BrowserVisible()
        {
            sut.ChangesProperty("BrowserVisible", () => sut.BrowserVisible = true);
        }
    }
}
