using NUnit.Framework;
using NSubstitute;
using Dietphone.Models;
using Dietphone.ViewModels;
using Dietphone.Tools;
using Dietphone.Views;
using System;

namespace Dietphone.Smartphone.Tests
{
    public class TrialViewModelTests
    {
        [TestCase(false, 1, 0, true, false)]
        [TestCase(false, 0, TrialViewModelImpl.PERIOD, false, false)]
        [TestCase(true, 0, TrialViewModelImpl.PERIOD, true, false)]
        [TestCase(true, 0, TrialViewModelImpl.PERIOD * 2, true, false)]
        [TestCase(true, 0, TrialViewModelImpl.PERIOD, true, true)]
        [TestCase(false, TrialViewModelImpl.PERIOD + 2, TrialViewModelImpl.PERIOD + 1, true, false)]
        [TestCase(false, TrialViewModelImpl.PERIOD, TrialViewModelImpl.PERIOD - 1, true, false)]
        [TestCase(false, TrialViewModelImpl.PERIOD, TrialViewModelImpl.PERIOD, null, false)]
        public void Run(bool expectConfirm, byte expectedTrialCounter, byte initialTrialCounter, bool? isTrialSetup,
            bool confirmSetup)
        {
            var factories = Substitute.For<Factories>();
            factories.Settings.Returns(new Settings());
            factories.Settings.TrialCounter = initialTrialCounter;
            var trial = Substitute.For<Trial>();
            Action<bool> isTrialCallback = null;
            trial.WhenForAnyArgs(substitute => substitute.IsTrial(null))
                .Do(args => isTrialCallback = (Action<bool>)args[0]);
            var messageDialog = Substitute.For<MessageDialog>();
            var confirmCalled = false;
            messageDialog.Confirm(Translations.HelloThanksForTryingOut, Translations.ThisIsAnUnregisteredCopy)
                .Returns(_ => { confirmCalled = true; return confirmSetup; });
            var sut = new TrialViewModelImpl(factories, trial, messageDialog);
            sut.Run();
            if (isTrialCallback != null && isTrialSetup != null)
                isTrialCallback(isTrialSetup.Value);
            Assert.AreEqual(expectedTrialCounter, factories.Settings.TrialCounter);
            Assert.AreEqual(expectConfirm, confirmCalled);
            trial.Received(confirmSetup ? 1 : 0).Show();
        }
    }
}
