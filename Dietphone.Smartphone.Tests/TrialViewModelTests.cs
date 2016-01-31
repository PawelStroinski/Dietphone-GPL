using System;
using NUnit.Framework;
using NSubstitute;
using Dietphone.Models;
using Dietphone.ViewModels;

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
            var isTrial = Substitute.For<Func<bool>>();
            isTrial.Invoke().Returns(isTrialSetup);
            var show = Substitute.For<Action>();
            var sut = new TrialViewModelImpl(factories, isTrial, show);
            sut.Run();
            Assert.AreEqual(expectZeroTrialCounter ? 0 : initialTrialCounter + 1, factories.Settings.TrialCounter);
            if (expectShow)
                show.Received().Invoke();
            else
                show.DidNotReceive().Invoke();
        }
    }
}
