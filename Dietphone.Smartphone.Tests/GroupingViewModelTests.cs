using System;
using System.Collections.Generic;
using System.Linq;
using Dietphone.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace Dietphone.Smartphone.Tests
{
    public class GroupingViewModelTests
    {
        private SearchSubViewModelStub viewModel;
        private GroupingViewModel<string, int> sut;
        private bool filterResult;
        private Action<string> choose;

        [SetUp]
        public void TestInitialize()
        {
            viewModel = new SearchSubViewModelStub();
            choose = Substitute.For<Action<string>>();
            sut = new GroupingViewModel<string, int>(viewModel, () => viewModel.Items, item => item.Length,
                item => filterResult, choose);
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void GroupsCanBeLoadedAndRefreshed(bool load, bool refresh)
        {
            Assert.IsNull(sut.Groups);
            sut.ChangesProperty("Groups", () =>
            {
                if (load)
                    viewModel.Load();
                if (refresh)
                    viewModel.Refresh();
            });
            var actual = sut.Groups;
            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(4, actual[0].Key);
            Assert.AreEqual(2, actual[1].Count());
        }

        [Test]
        public void GroupsCanBeSearched()
        {
            viewModel.Load();
            sut.ChangesProperty("Groups", () =>
            {
                viewModel.Search = "baz";
            });
            Assert.IsEmpty(sut.Groups);
            sut.ChangesProperty("Groups", () =>
            {
                viewModel.Search = string.Empty;
            });
            Assert.IsNotEmpty(sut.Groups);
            filterResult = true;
            sut.ChangesProperty("Groups", () =>
            {
                viewModel.Search = "baz";
            });
            Assert.IsNotEmpty(sut.Groups);
        }

        [Test]
        public void IgnoresSearchingIfNotLoaded()
        {
            viewModel.Search = "foo";
        }

        [Test]
        public void CanSort()
        {
            var unsorted = new GroupingViewModel<string, int>(viewModel, () => viewModel.Items,
                item => item.Length, item => filterResult, choose);
            viewModel.Load();
            Assert.AreEqual(4, unsorted.Groups[0].Key);
            Assert.AreEqual("foo", unsorted.Groups[1].First());
            var sorted = new SortedGroupingViewModel<string, int, string, int>(viewModel, () => viewModel.Items,
                item => item.Length, item => filterResult, choose, itemSort: item => item,
                groupSort: group => -group.Key);
            viewModel.Refresh();
            Assert.AreEqual(4, sorted.Groups[0].Key);
            Assert.AreEqual("bar", sorted.Groups[1].First());
        }

        [Test]
        public void CanChoose()
        {
            viewModel.Load();
            choose.DidNotReceiveWithAnyArgs().Invoke(null);
            sut.ChangesProperty("Choice", () => sut.Choice = "foo");
            choose.Received().Invoke("foo");
            Assert.IsNull(sut.Choice);
        }

        private class SearchSubViewModelStub : SearchSubViewModel
        {
            public List<string> Items { get; set; }

            public override void Load()
            {
                SetItems();
                OnLoaded();
            }

            public override void Refresh()
            {
                SetItems();
                OnRefreshed();
            }

            private void SetItems()
            {
                Items = new List<string> { "bar2", "foo", "bar" };
            }
        }
    }
}
