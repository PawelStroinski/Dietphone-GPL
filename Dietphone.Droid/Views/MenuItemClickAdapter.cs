using System;
using Android.Views;

namespace Dietphone.Views
{
    public class MenuItemClickAdapter : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
        private readonly Action action;

        public MenuItemClickAdapter(Action action)
        {
            this.action = action;
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            action();
            return true;
        }
    }
}