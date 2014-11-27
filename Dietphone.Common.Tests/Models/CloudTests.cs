using NUnit.Framework;
using System;
using NSubstitute;
using System.Linq;
using Dietphone.Views;

namespace Dietphone.Models.Tests
{
    public class CloudTests
    {
        private Factories factories;
        private CloudProviderFactory providerFactory;
        private CloudProvider provider;
        private ExportAndImport exportAndImport;
        private Settings settings;
        private Cloud sut;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            providerFactory = Substitute.For<CloudProviderFactory>();
            provider = Substitute.For<CloudProvider>();
            providerFactory.Create().Returns(provider);
            exportAndImport = Substitute.For<ExportAndImport>();
            settings = new Settings { CloudToken = "foo" };
            factories.Settings.Returns(settings);
            sut = new CloudImpl(providerFactory, factories, exportAndImport);
        }

        public class Export : CloudTests
        {
            [TestCase(false, "", "", 0)]
            [TestCase(false, "foo", "bar", 1)]
            [TestCase(true, "foo", "bar", 0)]
            [TestCase(true, "foo", "", 0)]
            [TestCase(true, "", "bar", 0)]
            [TestCase(true, "foo", "bar", -10)]
            public void DoesntDoIfTokenInfoIsMissingOrDateIsFuture(bool does, string secret, string token, int addDays)
            {
                settings.CloudSecret = secret;
                settings.CloudToken = token;
                settings.CloudExportDue = DateTime.Today.AddDays(addDays);
                sut.Export();
                if (does)
                    providerFactory.Received().Create();
                else
                    providerFactory.DidNotReceive().Create();
            }

            [Test]
            public void ExportsAndUploadsAFile()
            {
                exportAndImport.Export().Returns("_foo_bar_");
                sut.Export();
                provider.Received().UploadFile(DateTime.Now.ToString("yyyy-MM-dd") + ".xml", "_foo_bar_");
            }

            [Test]
            public void UpdatesDateIfUploadSucceeds()
            {
                sut.Export();
                Assert.AreEqual(DateTime.Today.AddDays(CloudImpl.ADD_DAYS_TO_TODAY), settings.CloudExportDue);
            }

            [Test]
            public void DoesntUpdateDateIfUploadFails()
            {
                provider.WhenForAnyArgs(p => p.UploadFile(null, null)).Do(delegate { throw new Exception(); });
                var date = settings.CloudExportDue;
                Assert.Throws<Exception>(() => sut.Export());
                Assert.AreEqual(date, settings.CloudExportDue);
            }
        }

        public class ListImports : CloudTests
        {
            [Test]
            public void ReturnsListOfNamesStartingFromTheNewestFile()
            {
                provider.ListFiles().Returns(
                    new[] { "2014-11-25.xmL", "2014-11-26.xml", "2014-10-26.xml", "foo.xml" }.ToList());
                var expected = new[] { "2014-11-26.xml", "2014-11-25.xmL", "2014-10-26.xml" };
                var actual = sut.ListImports();
                Assert.AreEqual(expected, actual);
            }
        }

        public class Import : CloudTests
        {
            [Test]
            public void ImportsTheSpecifiedFile()
            {
                provider.DownloadFile("FooBar.xml").Returns("FooBar");
                sut.Import("FooBar.xml");
                exportAndImport.Received().Import("FooBar");
                Assert.AreEqual(1, exportAndImport.ReceivedCalls().Count());
            }
        }
    }
}
