// The HideSoftInputOnTouchOutside method is from http://stackoverflow.com/a/28939113
using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Dietphone.Views.Adapters;

namespace Dietphone.Tools
{
    public static class UIExtensionMethods
    {
        public static IMenuItem SetTitleCapitalized(this IMenuItem item, string title)
        {
            item.SetTitle(title.Capitalize());
            return item;
        }

        public static IMenuItem SetOnMenuItemClick(this IMenuItem item, Action onClick)
        {
            item.SetOnMenuItemClickListener(new MenuItemClickListener(onClick));
            return item;
        }

        public static void HideSoftInputOnTouchOutside(this Activity activity, MotionEvent ev,
            Func<View, Rect> getGlobalVisibleRect)
        {
            if (ev.Action == MotionEventActions.Down)
            {
                var view = activity.CurrentFocus;
                if (view is EditText)
                {
                    var rect = getGlobalVisibleRect(view);
                    if (!rect.Contains((int)ev.GetX(), (int)ev.GetY()))
                    {
                        view.ClearFocus();
                        var inputManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
                        inputManager.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
                    }
                }
            }
        }

        public static bool IsParentOf(this View candidateParent, View view)
        {
            do
            {
                view = view.Parent as View;
                if (view == candidateParent)
                    return true;
            } while (view != null);
            return false;
        }
    }
}
