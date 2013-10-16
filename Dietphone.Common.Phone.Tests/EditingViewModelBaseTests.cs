using System;
using System.Collections.Generic;
using Dietphone.ViewModels;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Dietphone.Tools;
using NSubstitute;
using Dietphone.Models;

namespace Dietphone.Common.Phone.Tests
{
    public class EditingViewModelBaseTests
    {
        private TestModel modelCopy;
        private TestViewModel sut;
        private StateProvider stateProvider;

        [SetUp]
        public void TestInitialize()
        {
            modelCopy = new Fixture().Create<TestModel>();
            sut = new TestViewModel(modelCopy);
            stateProvider = Substitute.For<StateProvider>();
            sut.StateProvider = stateProvider;
            var state = new Dictionary<string, object>();
            stateProvider.State.Returns(state);
        }

        [Test]
        public void TombstoneModel()
        {
            sut.Load();
            sut.Tombstone();
            Assert.AreEqual(modelCopy.Serialize(string.Empty),
                stateProvider.State[typeof(TestModel).ToString()]);
        }

        [Test]
        public void UntombstoneModel()
        {
            var tombstoned = new Fixture().Create<TestModel>();
            tombstoned.Id = modelCopy.Id;
            stateProvider.State[typeof(TestModel).ToString()] = tombstoned.Serialize(string.Empty);
            sut.Load();
            Assert.AreEqual(tombstoned.Foo, modelCopy.Foo);
        }

        public class TestViewModel : EditingViewModelBase<TestModel, ViewModelBase>
        {
            public TestViewModel(TestModel modelCopy)
                : base(Substitute.For<Factories>())
            {
                this.modelCopy = modelCopy;
            }

            protected override void FindAndCopyModel()
            {
            }

            protected override void MakeViewModel()
            {
            }

            protected override string Validate()
            {
                throw new NotImplementedException();
            }
        }

        public class TestModel : EntityWithId
        {
            public string Foo { get; set; }
        }
    }
}
