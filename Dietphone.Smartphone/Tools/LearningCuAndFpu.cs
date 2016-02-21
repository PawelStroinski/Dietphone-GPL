using Dietphone.Views;

namespace Dietphone.Tools
{
    public interface LearningCuAndFpu
    {
        void LearnCu();
        void LearnCuAndFpu();
        void LearnFpu();
    }

    public class LearningCuAndFpuImpl : LearningCuAndFpu
    {
        private readonly MessageDialog messageDialog;

        public LearningCuAndFpuImpl(MessageDialog messageDialog)
        {
            this.messageDialog = messageDialog;
        }

        public void LearnCuAndFpu()
        {
            var both = string.Format("{0}\r\n\r\n{1}", Translations.CuIs, Translations.FpuIs);
            messageDialog.Show(both);
        }

        public void LearnCu()
        {
            messageDialog.Show(Translations.CuIs);
        }

        public void LearnFpu()
        {
            messageDialog.Show(Translations.FpuIs);
        }
    }
}
