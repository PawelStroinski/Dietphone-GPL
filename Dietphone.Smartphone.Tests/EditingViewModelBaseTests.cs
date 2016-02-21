using System;
using System.Collections.Generic;
using Dietphone.ViewModels;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Dietphone.Tools;
using NSubstitute;
using Dietphone.Models;

namespace Dietphone.Smartphone.Tests
{
    public class EditingViewModelBaseTests
    {
        private TestModel modelCopy;
        private TestViewModel sut;
        private StateProvider stateProvider;
        private MessageDialog messageDialog;

        [SetUp]
        public void TestInitialize()
        {
            modelCopy = new Fixture().Create<TestModel>();
            messageDialog = Substitute.For<MessageDialog>();
            sut = new TestViewModel(modelCopy, messageDialog);
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

        [Test]
        public void SaveAndReturn()
        {
            sut.SaveAndReturn();
            Assert.AreEqual(1, sut.DoSaveAndReturnCallCount);
            sut.ValidateSetup = "foo";
            sut.MessagesSetup.CannotSaveCaption = "bar";
            messageDialog.DidNotReceiveWithAnyArgs().Confirm(null, null);
            var confirm = false;
            messageDialog.Confirm("foo", "bar").Returns(_ => confirm);
            sut.SaveAndReturn();
            Assert.AreEqual(1, sut.DoSaveAndReturnCallCount);
            confirm = true;
            sut.SaveAndReturn();
            Assert.AreEqual(2, sut.DoSaveAndReturnCallCount);
        }

        public class TestViewModel : EditingViewModelBase<TestModel, ViewModelBase>
        {
            public string ValidateSetup;
            public Messages MessagesSetup = new Messages();
            public int DoSaveAndReturnCallCount;

            public TestViewModel(TestModel modelCopy, MessageDialog messageDialog)
                : base(Substitute.For<Factories>(), messageDialog)
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
                return ValidateSetup;
            }

            protected override void DoSaveAndReturn()
            {
                DoSaveAndReturnCallCount++;
            }

            internal override Messages Messages
            {
                get
                {
                    return MessagesSetup;
                }
            }
        }

        public class TestModel : EntityWithId
        {
            public string Foo { get; set; }
        }
    }
}
