using System.Windows.Navigation;
using Dietphone.Tools;

namespace Dietphone.Views
{
    public class StateProviderPage : StateAdapterPage, StateProvider
    {
        public bool IsOpened { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            IsOpened = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            IsOpened = false;
        }
    }
}