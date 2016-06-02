using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.Views;

namespace Dietphone.ViewModels
{
    public interface TrialViewModel
    {
        void Run();
    }

    public class TrialViewModelImpl : TrialViewModel
    {
        private readonly Factories factories;
        private readonly Trial trial;
        private readonly MessageDialog messageDialog;
        internal const byte PERIOD = 50;

        public TrialViewModelImpl(Factories factories, Trial trial, MessageDialog messageDialog)
        {
            this.factories = factories;
            this.trial = trial;
            this.messageDialog = messageDialog;
        }

        public void Run()
        {
            var settings = factories.Settings;
            messageDialog.Show("In TrialViewModelImpl.Run, settings.TrialCounter = " + settings.TrialCounter);
            var modulo = settings.TrialCounter % PERIOD;
            var isInPeriod = modulo == 0 && settings.TrialCounter > 0;
            if (isInPeriod)
                RunInPeriod(settings);
            else
                settings.TrialCounter = (byte)(settings.TrialCounter + 1);
        }

        private void RunInPeriod(Settings settings)
        {
            messageDialog.Show("In TrialViewModelImpl.RunInPeriod");
            trial.IsTrial((isTrial) =>
            {
                messageDialog.Show("In TrialViewModelImpl.RunInPeriod, isTrial = " + isTrial);
                if (isTrial)
                    ConfirmAndShow();
                settings.TrialCounter = 0;
            });
        }

        private void ConfirmAndShow()
        {
            messageDialog.Show("In TrialViewModelImpl.ConfirmAndShow");
            if (messageDialog.Confirm(Translations.HelloThanksForTryingOut, Translations.ThisIsAnUnregisteredCopy))
                trial.Show();
        }
    }
}
