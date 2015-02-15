using System;
using Dietphone.Models;

namespace Dietphone.ViewModels
{
    public interface TrialViewModel
    {
        void Run();
    }

    public class TrialViewModelImpl : TrialViewModel
    {
        private readonly Factories factories;
        private readonly Func<bool> isTrial;
        private readonly Action show;
        internal const byte PERIOD = 50;

        public TrialViewModelImpl(Factories factories, Func<bool> isTrial, Action show)
        {
            this.factories = factories;
            this.isTrial = isTrial;
            this.show = show;
        }

        public void Run()
        {
            var settings = factories.Settings;
            var modulo = settings.TrialCounter % PERIOD;
            var isInPeriod = modulo == 0 && settings.TrialCounter > 0;
            if (isInPeriod && isTrial())
                show();
            settings.TrialCounter = (byte)(isInPeriod ? 0 : settings.TrialCounter + 1);
        }
    }
}
