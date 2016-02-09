using NUnit.Framework;
using NSubstitute;
using Dietphone.Models;
using Dietphone.ViewModels;
using Dietphone.Tools;

namespace Dietphone.Smartphone.Tests
{
    public class TrialViewModelTests
    {
        [TestCase(false, false, 0, true)]
        [TestCase(false, true, TrialViewModelImpl.PERIOD, false)]
        [TestCase(true, true, TrialViewModelImpl.PERIOD, true)]
        [TestCase(true, true, TrialViewModelImpl.PERIOD * 2, true)]
        [TestCase(false, false, TrialViewModelImpl.PERIOD + 1, true)]
        [TestCase(false, false, TrialViewModelImpl.PERIOD - 1, true)]
        public void Run(bool expectShow, bool expectZeroTrialCounter, byte initialTrialCounter, bool isTrialSetup)
        {
            var factories = Substitute.For<Factories>();
            factories.Settings.Returns(new Settings());
            factories.Settings.TrialCounter = initialTrialCounter;
            var trial = Substitute.For<Trial>();
            trial.IsTrial().Returns(isTrialSetup);
            var sut = new TrialViewModelImpl(factories, trial);
            sut.Run();
            Assert.AreEqual(expectZeroTrialCounter ? 0 : initialTrialCounter + 1, factories.Settings.TrialCounter);
            if (expectShow)
                trial.Received().Show();
            else
                trial.DidNotReceive().Show();
        }
    }
}
