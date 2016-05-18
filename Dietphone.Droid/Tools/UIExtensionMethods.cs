// The HideSoftInputOnTouchOutside method is from http://stackoverflow.com/a/28939113
// and the ToPx from http://stackoverflow.com/a/6327095
using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Dietphone.Models;
using Dietphone.ViewModels;
using Dietphone.Adapters;
using MvvmCross.Droid.Views;
using OxyPlot;
using Android.Util;
using System.Collections.Generic;

namespace Dietphone.Tools
{
    public static class contextUIExtensionMethods
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

        public static IMenuItem BindSaveEnabled<TModel, TViewModel>(this IMenuItem save,
                EditingViewModelBase<TModel, TViewModel> viewModel)
            where TModel : EntityWithId
            where TViewModel : ViewModelBase
        {
            Action set = () => { save.SetOpaqueEnabled(viewModel.IsDirty); };
            viewModel.IsDirtyChanged += delegate { set(); };
            set();
            return save;
        }

        public static IMenuItem SetOpaqueEnabled(this IMenuItem item, bool enabled)
        {
            item.SetEnabled(enabled);
            var icon = item.Icon;
            if (icon != null)
                icon.Alpha = enabled.ToAlpha();
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

        public static List<View> GetChildren(this ViewGroup parent)
        {
            var children = new List<View>();
            for (int i = 0; i < parent.ChildCount; i++)
                children.Add(parent.GetChildAt(i));
            return children;
        }

        public static int ToAlpha(this bool enabled)
        {
            return enabled ? 255 : 100;
        }

        public static string ResourceColorToHex(this ContextWrapper context, int resourceId)
        {
            return string.Format("{0:x}", context.ResourceColorToArgb(resourceId)).Substring(2);
        }

        public static OxyColor ResourceColorToOxyColor(this ContextWrapper context, int resourceId)
        {
            return OxyColor.FromUInt32((uint)context.ResourceColorToArgb(resourceId));
        }

        public static float ToPx(this double value, ComplexUnitType unit, ContextWrapper context)
        {
            var resources = context.Resources;
            return TypedValue.ApplyDimension(unit, (float)value, resources.DisplayMetrics);
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

        private static int ResourceColorToArgb(this ContextWrapper context, int resourceId)
        {
            var resources = context.Resources;
#pragma warning disable CS0618 // Type or member is obsolete
            var color = resources.GetColor(resourceId);
#pragma warning restore CS0618 // Type or member is obsolete
            return color.ToArgb();
        }
    }
}
