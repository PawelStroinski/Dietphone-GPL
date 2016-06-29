using Dietphone.Models;
using Dietphone.ViewModels;
using NUnit.Framework;
using NSubstitute;
using System.Linq;
using Dietphone.Tools;
using System;
using System.Collections.Generic;
using Ploeh.AutoFixture;
using Dietphone.Views;

namespace Dietphone.Smartphone.Tests
{
    public class ExportAndImportViewModelTests : TestBase
    {
        private Factories factories;
        private CloudProviderFactory cloudProviderFactory;
        private CloudProvider cloudProvider;
        private Settings settings;
        private Vibration vibration;
        private Cloud cloud;
        private MessageDialog messageDialog;
        private CloudMessages cloudMessages;
        private BackgroundWorkerSyncFactory workerFactory;
        private ExportAndImportViewModel sut;
        private string navigatedTo;

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
            messageDialog = Substitute.For<MessageDialog>();
            cloudMessages = new Fixture().Create<CloudMessages>();
            workerFactory = new BackgroundWorkerSyncFactory();
            sut = new ExportAndImportViewModel(factories, cloudProviderFactory, vibration, cloud, messageDialog,
                cloudMessages, workerFactory);
            cloudProvider.GetAcquiredToken().Returns(new CloudToken { Secret = "foo", Token = "bar" });
            cloudProvider.GetTokenAcquiringUrl(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL).Returns("go");
            cloud.ListImports().Returns(new List<string>());
            navigatedTo = string.Empty;
            sut.NavigateInBrowser += (_, url) => { navigatedTo = url; };
        }

        public class ExportToCloud : ExportAndImportViewModelTests
        {
            [TestCase(true, "foo", "")]
            [TestCase(true, "", "bar")]
            [TestCase(true, "foo", "bar")]
            [TestCase(false, "", "")]
            public void IfItHasATokenThenItConfirmsExportToCloudDeactivationOtherwiseItShowsTheTokenAcquirePage(
                bool hasAToken, string secret, string token)
            {
                settings.CloudSecret = secret;
                settings.CloudToken = token;
                sut.ExportToCloud.Call();
                messageDialog.Received(hasAToken ? 1 : 0)
                    .Confirm(cloudMessages.ConfirmExportToCloudDeactivation, string.Empty);
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
                messageDialog.Confirm(cloudMessages.ConfirmExportToCloudDeactivation, string.Empty)
                    .Returns(confirmed);
                sut.ExportToCloud.Call();
                Assert.AreEqual(confirmed ? string.Empty : "foo", settings.CloudSecret);
                Assert.AreEqual(confirmed ? string.Empty : "bar", settings.CloudToken);
                Assert.AreEqual(confirmed ? DateTime.MinValue : DateTime.Today, settings.CloudExportDue);
            }

            [Test]
            public void UsesSingleInstanceOfTokenProvider()
            {
                sut.ExportToCloud.Call();
                sut.BrowserIsNavigating(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL);
                Assert.AreEqual(1, cloudProviderFactory.ReceivedCalls().Count());
            }

            [TestCase("foo")]
            [TestCase("")]
            public void Vibrates(string token)
            {
                settings.CloudToken = token;
                sut.ExportToCloud.Call();
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
                messageDialog.Confirm(cloudMessages.ConfirmExportToCloudDeactivation, string.Empty)
                    .Returns(confirmedDeactivation);
                sut.ChangesProperty(propertyName, () => sut.ExportToCloud.Call());
                if (!expected)
                {
                    sut.ChangesProperty(propertyName, () =>
                    {
                        sut.BrowserIsNavigating(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL);
                    });
                }
            }

            [Test]
            public void ErrorAtGetTokenAcquiringUrl()
            {
                cloudProvider.GetTokenAcquiringUrl(null).ReturnsForAnyArgs(_ => { throw new Exception(); });
                sut.ExportToCloud.Call();
                messageDialog.Received().Show(cloudMessages.CloudError);
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
                        sut.ImportFromCloud.Call();
                    }));
                Assert.AreSame(cloud.ListImports(), sut.ImportFromCloudItems);
                Assert.IsTrue(sut.ImportFromCloudVisible);
            }

            [Test]
            public void IfTokenIsNotPresentThenShowsTheTokenAcquirePage()
            {
                sut.ImportFromCloud.Call();
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
                    sut.ImportFromCloud.Call();
                });
                messageDialog.Received().Show(cloudMessages.CloudError);
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
                sut.ChangesProperty("IsBusy", () => sut.ImportFromCloudWithSelection.Call());
                Assert.IsFalse(sut.ImportFromCloudVisible);
                cloud.Received().Import(sut.ImportFromCloudSelectedItem);
                messageDialog.Received().Show(cloudMessages.ImportFromCloudSuccessful);
            }

            [TestCase("")]
            [TestCase(null)]
            public void IfNothingIsSelectedThenDoesNothing(string selection)
            {
                sut.ImportFromCloudVisible = true;
                sut.ImportFromCloudSelectedItem = selection;
                sut.ImportFromCloudWithSelection.Call();
                Assert.IsFalse(sut.ImportFromCloudVisible);
                cloud.DidNotReceiveWithAnyArgs().Import(null);
                messageDialog.DidNotReceive().Show(cloudMessages.ImportFromCloudSuccessful);
            }

            [Test]
            public void ErrorAtImport()
            {
                sut.ImportFromCloudSelectedItem = "foo";
                cloud.When(c => c.Import("foo")).Do(_ => { throw new Exception(); });
                sut.ImportFromCloudWithSelection.Call();
                messageDialog.Received().Show(cloudMessages.CloudError);
                messageDialog.DidNotReceive().Show(cloudMessages.ImportFromCloudSuccessful);
            }
        }

        public class BrowserIsNavigating : ExportAndImportViewModelTests
        {
            private void Clean()
            {
                cloud.ClearReceivedCalls();
                settings.CloudSecret = string.Empty;
                settings.CloudToken = string.Empty;
                messageDialog.ClearReceivedCalls();
            }

            private void IfBrowserIsNavigatingToTokenAcquiringCallbackUrlAfterExportToCloudThenStoresTheTokenAndExports(
                bool expectedUrl, string url)
            {
                sut.ExportToCloud.Call();
                cloud.When(c => c.Export()).Do(_ =>
                {
                    Assert.AreEqual(ExportAndImportViewModel.TOKEN_ACQUIRING_NAVIGATE_AWAY_URL, navigatedTo);
                    Assert.AreEqual("foo", settings.CloudSecret);
                    Assert.AreEqual("bar", settings.CloudToken);
                });
                sut.BrowserVisible = true;
                sut.BrowserIsNavigating(url == null ? null : url.ToUpper() + "?");
                if (expectedUrl)
                    cloud.Received().Export();
                else
                {
                    cloudProvider.DidNotReceive().GetAcquiredToken();
                    cloud.DidNotReceive().Export();
                }
                Assert.AreNotEqual(expectedUrl, sut.BrowserVisible);
                messageDialog.Received(expectedUrl ? 1 : 0).Show(cloudMessages.ExportToCloudActivationSuccessful);
            }

            private void IfBrowserIsNavigatingToTokenAcquiringCallbackUrlAfterImportFromCloudThenStoresTheTokenAndShowsImportItems(
                bool expectedUrl, string url)
            {
                sut.ImportFromCloud.Call();
                sut.BrowserVisible = true;
                sut.BrowserIsNavigating(url == null ? null : url.ToUpper() + "?");
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
                messageDialog.DidNotReceive().Show(cloudMessages.ExportToCloudActivationSuccessful);
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
            }

            [TestCase(true)]
            [TestCase(false)]
            public void ErrorAtGetAcquiredTokenOrAtExport(bool errorAtGetAcquiredToken)
            {
                if (errorAtGetAcquiredToken)
                    cloudProvider.GetAcquiredToken().ReturnsForAnyArgs(_ => { throw new Exception(); });
                else
                    cloud.When(c => c.Export()).Do(_ => { throw new Exception(); });
                sut.ExportToCloud.Call();
                sut.BrowserIsNavigating(ExportAndImportViewModel.TOKEN_ACQUIRING_CALLBACK_URL);
                messageDialog.Received().Show(cloudMessages.CloudError);
                Assert.IsFalse(sut.BrowserVisible);
                messageDialog.DidNotReceive().Show(cloudMessages.ExportToCloudActivationSuccessful);
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
                cloud.Received().Export();
                messageDialog.Received().Show(cloudMessages.ExportToCloudSuccessful);
                Assert.IsFalse(sut.IsBusy);
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void IfExportToCloudIsNotActiveThrowsException()
            {
                sut.ExportToCloudNow();
            }

            [Test]
            public void ErrorAtExport()
            {
                settings.CloudSecret = "foo";
                cloud.When(c => c.Export()).Do(_ => { throw new Exception(); });
                sut.ExportToCloudNow();
                messageDialog.Received().Show(cloudMessages.CloudError);
                messageDialog.DidNotReceive().Show(cloudMessages.ExportToCloudSuccessful);
                Assert.IsFalse(sut.IsBusy);
            }
        }

        [Test]
        public void BrowserVisible()
        {
            sut.ChangesProperty("BrowserVisible", () => sut.BrowserVisible = true);
        }

        [TestCase("foo@bar.baz", true)]
        [TestCase("?", false)]
        public void AskToExportToEmail(string email, bool shouldCreateWorker)
        {
            workerFactory.NoOp = true;
            messageDialog.Input(string.Empty, caption: Translations.SendToAnEMailAddress, value: string.Empty,
                type: InputType.Email).Returns(email);
            sut.AskToExportToEmail.Call();
            Assert.AreEqual(shouldCreateWorker, workerFactory.Called);
        }

        [TestCase("http://foo.bar", true)]
        [TestCase("-", false)]
        public void AskToImportFromAddress(string url, bool shouldCreateWorker)
        {
            workerFactory.NoOp = true;
            messageDialog.Input(string.Empty, caption: Translations.DownloadFileFromAddress,
                value: ExportAndImportViewModel.INITIAL_URL, type: InputType.Url).Returns(url);
            sut.AskToImportFromAddress.Call();
            Assert.AreEqual(shouldCreateWorker, workerFactory.Called);
        }
    }
}
