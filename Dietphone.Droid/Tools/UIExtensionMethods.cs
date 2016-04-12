// The HideSoftInputOnTouchOutside method is from http://stackoverflow.com/a/28939113
using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Dietphone.ViewModels;
using Dietphone.Views.Adapters;
using MvvmCross.Droid.Views;

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

        public static void HideSoftInputOnTouchOutside(this Activity activity, MotionEvent ev)
        {
            activity.HideSoftInputOnTouchOutside(ev, GetGlobalVisibleRect);
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

        public static void InitializeTabs(this MvxActivity activity, params string[] texts)
        {
            var actionBar = activity.ActionBar;
            actionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            var viewModel = (PivotTombstoningViewModel)activity.ViewModel;
            actionBar.AddTabs(viewModel, texts);
            actionBar.SelectTabOnViewModelChange(viewModel);
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

        private static Rect GetGlobalVisibleRect(View view)
        {
            var rect = new Rect();
            view.GetGlobalVisibleRect(rect);
            return rect;
        }

        private static void AddTabs(this ActionBar actionBar, PivotTombstoningViewModel viewModel, string[] texts)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                var tab = actionBar.NewTab();
                tab.SetText(texts[i]);
                var pivot = i;
                tab.TabSelected += delegate { viewModel.Pivot = pivot; };
                actionBar.AddTab(tab);
            }
        }

        private static void SelectTabOnViewModelChange(this ActionBar actionBar, PivotTombstoningViewModel viewModel)
        {
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != "Pivot")
                    return;
                var tab = actionBar.GetTabAt(viewModel.Pivot);
                if (tab != actionBar.SelectedTab)
                    actionBar.SelectTab(tab);
            };
        }
    }
}
