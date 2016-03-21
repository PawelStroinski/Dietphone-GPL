using System.Collections.Generic;
using System.Linq;
using Dietphone.ViewModels;
using NUnit.Framework;

namespace Dietphone.Smartphone.Tests
{
    public class GroupingViewModelTests
    {
        private SearchSubViewModelStub viewModel;
        private GroupingViewModel<string, int> sut;
        private bool filterResult;

        [SetUp]
        public void TestInitialize()
        {
            viewModel = new SearchSubViewModelStub();
            sut = new GroupingViewModel<string, int>(viewModel, () => viewModel.Items, item => item.Length,
                item => filterResult);
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
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(3, actual[0].Key);
            Assert.AreEqual(2, actual[0].Count());
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
                Items = new List<string> { "foo", "bar" };
            }
        }
    }
}
