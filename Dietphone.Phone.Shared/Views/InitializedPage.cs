using System.Windows.Navigation;
using MvvmCross.WindowsPhone.Views;

namespace Dietphone.Views
{
    public abstract class InitializedPage : MvxPhonePage
    {
        protected virtual void OnInitializePage()
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var needsToInitialize = ViewModel == null;
            base.OnNavigatedTo(e);
            if (needsToInitialize)
                OnInitializePage();
        }
    }
}
