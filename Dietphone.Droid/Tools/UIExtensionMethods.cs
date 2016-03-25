using System;
using Android.Views;
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
    }
}
