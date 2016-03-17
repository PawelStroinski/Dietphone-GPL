using System;
using Android.Views;

namespace Dietphone.Views.ListenerAdapters
{
    public class MenuItemClick : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
        private readonly Action onClick;

        public MenuItemClick(Action onClick)
        {
            this.onClick = onClick;
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            onClick();
            return true;
        }
    }
}