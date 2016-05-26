using Dietphone.Tools;
using Dietphone.Views;

namespace Dietphone.ViewModels
{
    public class EmbeddedAboutViewModel : ViewModelBase
    {
        public string Title => $"{Translations.DiabetesSpyTitleCase} {new AppVersion().GetAppVersion()}";
    }
}
