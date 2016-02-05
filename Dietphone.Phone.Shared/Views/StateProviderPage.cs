using System.Windows.Navigation;
using Dietphone.Tools;
using MvvmCross.WindowsPhone.Views;

namespace Dietphone.Views
{
    public class StateProviderPage : MvxPhonePage, StateProvider
    {
        public bool IsOpened { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            IsOpened = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IsOpened = false;
        }
    }
}