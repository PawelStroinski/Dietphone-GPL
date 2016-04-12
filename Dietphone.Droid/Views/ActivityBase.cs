using Android.Views;
using Dietphone.Tools;
using Dietphone.ViewModels;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    public abstract class ActivityBase<TViewModel> : MvxActivity<TViewModel>
        where TViewModel : ViewModelBase
    {
        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            this.HideSoftInputOnTouchOutside(ev);
            return base.DispatchTouchEvent(ev);
        }
    }
}