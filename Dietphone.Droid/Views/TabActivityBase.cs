// The HideSoftInputOnTouchOutside method is from http://stackoverflow.com/a/28939113
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Dietphone.ViewModels;
using MvvmCross.Droid.Views;

namespace Dietphone.Views
{
    public abstract class TabActivityBase<TViewModel> : MvxTabActivity<TViewModel>
        where TViewModel : ViewModelBase
    {
        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            HideSoftInputOnTouchOutside(ev);
            return base.DispatchTouchEvent(ev);
        }

        private void HideSoftInputOnTouchOutside(MotionEvent ev)
        {
            if (ev.Action == MotionEventActions.Down)
            {
                var view = CurrentFocus;
                if (view is EditText)
                {
                    var rect = GetGlobalVisibleRect(view);
                    if (!rect.Contains((int)ev.GetX(), (int)ev.GetY()))
                    {
                        view.ClearFocus();
                        var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                        inputManager.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
                    }
                }
            }
        }

        protected virtual Rect GetGlobalVisibleRect(View view)
        {
            var rect = new Rect();
            view.GetGlobalVisibleRect(rect);
            return rect;
        }
    }
}