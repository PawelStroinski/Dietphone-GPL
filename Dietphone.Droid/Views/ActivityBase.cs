using Dietphone.ViewModels;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    public abstract class ActivityBase<TViewModel> : MvxActivity<TViewModel>
        where TViewModel : ViewModelBase
    {
    }
}