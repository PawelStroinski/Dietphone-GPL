using System;
using Android.Views;

namespace Dietphone.Views.Adapters
{
    public class MenuItemClickListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
        private readonly Action onClick;

        public MenuItemClickListener(Action onClick)
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