using NUnit.Framework;
using NSubstitute;
using Dietphone.Models;
using Dietphone.ViewModels;
using Dietphone.Tools;
using Dietphone.Views;

namespace Dietphone.Smartphone.Tests
{
    public class TrialViewModelTests
    {
        [TestCase(false, false, 0, true, false)]
        [TestCase(false, true, TrialViewModelImpl.PERIOD, false, false)]
        [TestCase(true, true, TrialViewModelImpl.PERIOD, true, false)]
        [TestCase(true, true, TrialViewModelImpl.PERIOD * 2, true, false)]
        [TestCase(true, true, TrialViewModelImpl.PERIOD, true, true)]
        [TestCase(false, false, TrialViewModelImpl.PERIOD + 1, true, false)]
        [TestCase(false, false, TrialViewModelImpl.PERIOD - 1, true, false)]
        public void Run(bool expectConfirm, bool expectZeroTrialCounter, byte initialTrialCounter, bool isTrialSetup,
            bool confirmSetup)
        {
            var factories = Substitute.For<Factories>();
            factories.Settings.Returns(new Settings());
            factories.Settings.TrialCounter = initialTrialCounter;
            var trial = Substitute.For<Trial>();
            trial.IsTrial().Returns(isTrialSetup);
            var messageDialog = Substitute.For<MessageDialog>();
            var confirmCalled = false;
            messageDialog.Confirm(Translations.HelloThanksForTryingOut, Translations.ThisIsAnUnregisteredCopy)
                .Returns(_ => { confirmCalled = true; return confirmSetup; });
            var sut = new TrialViewModelImpl(factories, trial, messageDialog);
            sut.Run();
            Assert.AreEqual(expectZeroTrialCounter ? 0 : initialTrialCounter + 1, factories.Settings.TrialCounter);
            Assert.AreEqual(expectConfirm, confirmCalled);
            trial.Received(confirmSetup ? 1 : 0).Show();
        }
    }
}
