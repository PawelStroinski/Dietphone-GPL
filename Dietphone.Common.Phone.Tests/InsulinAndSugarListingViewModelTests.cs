using System;
using System.Collections.Generic;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using Dietphone.Views;
using System.Collections.ObjectModel;

namespace Dietphone.Common.Phone.Tests
{
    public class InsulinAndSugarListingViewModelTests
    {
        private Factories factories;
        private InsulinAndSugarListingViewModel sut;

        [SetUp]
        public void TestInitialize()
        {
            factories = Substitute.For<Factories>();
            factories.InsulinCircumstances.Returns(new List<InsulinCircumstance>());
            factories.Insulins.Returns(new List<Insulin>());
            factories.Sugars.Returns(new List<Sugar>());
            sut = new InsulinAndSugarListingViewModel(factories, new BackgroundWorkerSyncFactory());
        }

        [Test]
        public void LoadLoadsWhenNotLoadedAlready()
        {
            var loaded = false;
            sut.Loaded += delegate { loaded = true; };
            sut.ChangesProperty("IsBusy", () =>
            {
                sut.Load();
            });
            Assert.IsTrue(loaded);
        }

        [Test]
        public void LoadDoesntLoadWhenLoadedAlready()
        {
            sut.Load();
            sut.NotChangesProperty("IsBusy", () =>
            {
                sut.Load();
            });
        }

        [Test]
        public void RefreshDoesntLoadWhenNotLoadedAlready()
        {
            sut.NotChangesProperty("IsBusy", () =>
            {
                sut.Refresh();
            });
        }

        [Test]
        public void RefreshLoadsWhenNotLoadedAlready()
        {
            sut.Load();
            var refreshed = false;
            sut.Refreshed += delegate { refreshed = true; };
            sut.ChangesProperty("IsBusy", () =>
            {
                sut.Refresh();
            });
            Assert.IsTrue(refreshed);
        }

        [Test]
        public void OnSearchChanged()
        {
            var sut = new SutAccessor();
            var updating = false;
            var update = true;
            var updated = false;
            sut.DescriptorsUpdating += delegate { updating = true; };
            sut.DescriptorsUpdated += delegate { updated = true; };
            sut.UpdateFilterDescriptorsEvent += delegate
            {
                Assert.IsTrue(updating);
                Assert.IsFalse(updated);
                update = true;
            };
            sut.OnSearchChanged();
            Assert.IsTrue(update);
            Assert.IsTrue(updated);
        }

        class SutAccessor : InsulinAndSugarListingViewModel
        {
            public event EventHandler UpdateFilterDescriptorsEvent;

            public SutAccessor()
                : base(null, null)
            {
            }

            public new void OnSearchChanged()
            {
                base.OnSearchChanged();
            }

            protected override void UpdateFilterDescriptors()
            {
                UpdateFilterDescriptorsEvent(null, null);
            }
        }
    }
}
