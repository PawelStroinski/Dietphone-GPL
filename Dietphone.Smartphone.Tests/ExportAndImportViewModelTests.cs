using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using System.Linq;
using Dietphone.Tools;
using System.Threading;
using System;
using System.Collections.Generic;

namespace Dietphone.Smartphone.Tests
{
    public class ExportAndImportViewModelTests
    {
        private Factories factories;
        private CloudProviderFactory cloudProviderFactory;
        private CloudProvider cloudProvider;
        private Settings settings;
        private Vibration vibration;
        private Cloud cloud;
        private ExportAndImportViewModel sut;
        private string navigatedTo;
        private bool calledExportToCloudActivationSuccessful, calledExportToCloudSuccessful,
            calledImportFromCloudSuccessful, calledCloudError;

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
            cloud = Substitute.For<Cloud>();
            sut = new ExportAndImportViewModel(factories, cloudProviderFactory, vibration, cloud);
            cloudProvider.GetAcquiredToken().Returns(new CloudToken { Secret = "foo", Token = "bar" });
            cloudProvider.GetTokenAcquiringUrl(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL).Returns("go");
            cloud.ListImports().Returns(new List<string>());
            navigatedTo = string.Empty;
            sut.NavigateInBrowser += (_, url) => { navigatedTo = url; };
            calledExportToCloudActivationSuccessful = calledImportFromCloudSuccessful = calledCloudError = false;
            sut.ExportToCloudActivationSuccessful += (_, __) => { calledExportToCloudActivationSuccessful = true; };
            sut.ExportToCloudSuccessful += (_, __) => { calledExportToCloudSuccessful = true; };
            sut.ImportFromCloudSuccessful += (_, __) => { calledImportFromCloudSuccessful = true; };
            sut.CloudError += (_, __) => { calledCloudError = true; };
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
                sut.ConfirmExportToCloudDeactivation += (_, __) => { confirmed = true; };
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
                settings.CloudExportDue = DateTime.Today;
                sut.ConfirmExportToCloudDeactivation += (_, eventArgs) => { eventArgs.Confirm = confirmed; };
                sut.ExportToCloud();
                Assert.AreEqual(confirmed ? string.Empty : "foo", settings.CloudSecret);
                Assert.AreEqual(confirmed ? string.Empty : "bar", settings.CloudToken);
                Assert.AreEqual(confirmed ? DateTime.MinValue : DateTime.Today, settings.CloudExportDue);
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
                    sut.ChangesProperty(propertyName, () =>
                    {
                        sut.BrowserIsNavigating(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL);
                        Thread.Sleep(10);
                    });
                }
            }

            [Test]
            public void ErrorAtGetTokenAcquiringUrl()
            {
                cloudProvider.GetTokenAcquiringUrl(null).ReturnsForAnyArgs(_ => { throw new Exception(); });
                sut.ExportToCloud();
                Thread.Sleep(10);
                Assert.IsTrue(calledCloudError);
                Assert.IsFalse(sut.BrowserVisible);
            }
        }

        public class ImportFromCloud : ExportAndImportViewModelTests
        {
            [Test]
            public void IfTokenPresentThenPopulatesImportFromCloudItemsAndSetsImportFromCloudVisibleToTrue()
            {
                settings.CloudSecret = "foo";
                sut.ChangesProperty("ImportFromCloudItems", () =>
                    sut.ChangesProperty("ImportFromCloudVisible", () =>
                    {
                        sut.ImportFromCloud();
                        Thread.Sleep(10);
                    }));
                Assert.AreSame(cloud.ListImports(), sut.ImportFromCloudItems);
                Assert.IsTrue(sut.ImportFromCloudVisible);
            }

            [Test]
            public void IfTokenIsNotPresentThenShowsTheTokenAcquirePage()
            {
                sut.ImportFromCloud();
                Thread.Sleep(10);
                Assert.IsTrue(sut.BrowserVisible);
                Assert.AreEqual("go", navigatedTo);
            }

            [Test]
            public void ErrorAtListImports()
            {
                settings.CloudSecret = "foo";
                cloud.ListImports().ReturnsForAnyArgs(_ => { throw new Exception(); });
                sut.NotChangesProperty("ImportFromCloudItems", () =>
                {
                    sut.ImportFromCloud();
                    Thread.Sleep(10);
                });
                Assert.IsTrue(calledCloudError);
                Assert.IsFalse(sut.ImportFromCloudVisible);
            }
        }

        public class ImportFromCloudWithSelection : ExportAndImportViewModelTests
        {
            [Test]
            public void IfSomethingIsSelectedThenImportsItAndDisplaysAMessage()
            {
                sut.ImportFromCloudVisible = true;
                sut.ImportFromCloudSelectedItem = "foo";
                sut.ChangesProperty("IsBusy", () => sut.ImportFromCloudWithSelection());
                Assert.IsFalse(sut.ImportFromCloudVisible);
                Thread.Sleep(10);
                cloud.Received().Import(sut.ImportFromCloudSelectedItem);
                Assert.IsTrue(calledImportFromCloudSuccessful);
            }

            [TestCase("")]
            [TestCase(null)]
            public void IfNothingIsSelectedThenDoesNothing(string selection)
            {
                sut.ImportFromCloudVisible = true;
                sut.ImportFromCloudSelectedItem = selection;
                sut.ImportFromCloudWithSelection();
                Assert.IsFalse(sut.ImportFromCloudVisible);
                Thread.Sleep(10);
                cloud.DidNotReceiveWithAnyArgs().Import(null);
                Assert.IsFalse(calledImportFromCloudSuccessful);
            }

            [Test]
            public void ErrorAtImport()
            {
                sut.ImportFromCloudSelectedItem = "foo";
                cloud.When(c => c.Import("foo")).Do(_ => { throw new Exception(); });
                sut.ImportFromCloudWithSelection();
                Thread.Sleep(10);
                Assert.IsTrue(calledCloudError);
                Assert.IsFalse(calledImportFromCloudSuccessful);
            }
        }

        public class BrowserIsNavigating : ExportAndImportViewModelTests
        {
            private void Clean()
            {
                cloud.ClearReceivedCalls();
                settings.CloudSecret = string.Empty;
                settings.CloudToken = string.Empty;
                calledExportToCloudActivationSuccessful = false;
            }

            private void IfBrowserIsNavigatingToTokenAcquiringCallbackUrlAfterExportToCloudThenStoresTheTokenAndExports(
                bool expectedUrl, string url)
            {
                sut.ExportToCloud();
                Thread.Sleep(10);
                cloud.When(c => c.Export()).Do(_ =>
                {
                    Assert.AreEqual(ExportAndImportViewModel.TOKEN_ACQUIRING_NAVIGATE_AWAY_URL, navigatedTo);
                    Assert.AreEqual("foo", settings.CloudSecret);
                    Assert.AreEqual("bar", settings.CloudToken);
                });
                sut.BrowserVisible = true;
                sut.BrowserIsNavigating(url == null ? null : url.ToUpper() + "?");
                Thread.Sleep(10);
                if (expectedUrl)
                    cloud.Received().Export();
                else
                {
                    cloudProvider.DidNotReceive().GetAcquiredToken();
                    cloud.DidNotReceive().Export();
                }
                Assert.AreNotEqual(expectedUrl, sut.BrowserVisible);
                Assert.AreEqual(expectedUrl, calledExportToCloudActivationSuccessful);
            }

            private void IfBrowserIsNavigatingToTokenAcquiringCallbackUrlAfterImportFromCloudThenStoresTheTokenAndShowsImportItems(
                bool expectedUrl, string url)
            {
                sut.ImportFromCloud();
                Thread.Sleep(10);
                sut.BrowserVisible = true;
                sut.BrowserIsNavigating(url == null ? null : url.ToUpper() + "?");
                Thread.Sleep(10);
                if (expectedUrl)
                {
                    Assert.AreSame(cloud.ListImports(), sut.ImportFromCloudItems);
                    Assert.AreEqual(ExportAndImportViewModel.TOKEN_ACQUIRING_NAVIGATE_AWAY_URL, navigatedTo);
                    Assert.AreEqual("foo", settings.CloudSecret);
                    Assert.AreEqual("bar", settings.CloudToken);
                }
                else
                    cloudProvider.DidNotReceive().GetAcquiredToken();
                Assert.AreNotEqual(expectedUrl, sut.BrowserVisible);
                Assert.AreEqual(expectedUrl, sut.ImportFromCloudVisible);
                cloud.DidNotReceive().Export();
                Assert.IsFalse(calledExportToCloudActivationSuccessful);
            }

            [TestCase(false, "http://foo")]
            [TestCase(false, null)]
            [TestCase(true, ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL)]
            public void ExportFirst(bool expectedUrl, string url)
            {
                IfBrowserIsNavigatingToTokenAcquiringCallbackUrlAfterExportToCloudThenStoresTheTokenAndExports(
                    expectedUrl, url);
                Clean();
                IfBrowserIsNavigatingToTokenAcquiringCallbackUrlAfterImportFromCloudThenStoresTheTokenAndShowsImportItems(
                    expectedUrl, url);
            }

            [TestCase(false, "http://foo")]
            [TestCase(false, null)]
            [TestCase(true, ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL)]
            public void ImportFirst(bool expectedUrl, string url)
            {
                IfBrowserIsNavigatingToTokenAcquiringCallbackUrlAfterImportFromCloudThenStoresTheTokenAndShowsImportItems(
                    expectedUrl, url);
                Clean();
                IfBrowserIsNavigatingToTokenAcquiringCallbackUrlAfterExportToCloudThenStoresTheTokenAndExports(
                    expectedUrl, url);
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void IfBrowserIsNavigatingIsCalledButExportOrImportToCloudWasNotCalledThrowsException()
            {
                sut.BrowserIsNavigating(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL);
                Thread.Sleep(10);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void ErrorAtGetAcquiredTokenOrAtExport(bool errorAtGetAcquiredToken)
            {
                if (errorAtGetAcquiredToken)
                    cloudProvider.GetAcquiredToken().ReturnsForAnyArgs(_ => { throw new Exception(); });
                else
                    cloud.When(c => c.Export()).Do(_ => { throw new Exception(); });
                sut.ExportToCloud();
                Thread.Sleep(10);
                sut.BrowserIsNavigating(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL);
                Thread.Sleep(10);
                Assert.IsTrue(calledCloudError);
                Assert.IsFalse(sut.BrowserVisible);
                Assert.IsFalse(calledExportToCloudActivationSuccessful);
                if (errorAtGetAcquiredToken)
                    cloud.DidNotReceive().Export();
            }
        }

        public class ExportToCloudNow : ExportAndImportViewModelTests
        {
            [Test]
            public void InvokesMakeItExportAndThenExport()
            {
                settings.CloudSecret = "foo";
                cloud.When(c => c.Export()).Do(_ => cloud.Received().MakeItExport());
                sut.ChangesProperty("IsBusy", () => sut.ExportToCloudNow());
                Thread.Sleep(10);
                cloud.Received().Export();
                Assert.IsTrue(calledExportToCloudSuccessful);
                Assert.IsFalse(sut.IsBusy);
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void IfExportToCloudIsNotActiveThrowsException()
            {
                sut.ExportToCloudNow();
                Thread.Sleep(10);
            }

            [Test]
            public void ErrorAtExport()
            {
                settings.CloudSecret = "foo";
                cloud.When(c => c.Export()).Do(_ => { throw new Exception(); });
                sut.ExportToCloudNow();
                Thread.Sleep(10);
                Assert.IsTrue(calledCloudError);
                Assert.IsFalse(calledExportToCloudSuccessful);
                Assert.IsFalse(sut.IsBusy);
            }
        }

        [Test]
        public void BrowserVisible()
        {
            sut.ChangesProperty("BrowserVisible", () => sut.BrowserVisible = true);
        }
    }
}
