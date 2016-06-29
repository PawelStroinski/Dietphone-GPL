using System;
using Android.Views;

namespace Dietphone.Adapters
{
    public class ActionExpandListener : Java.Lang.Object, IMenuItemOnActionExpandListener
    {
        private readonly Action onExpand;
        private readonly Action onCollapse;

        public ActionExpandListener(Action onExpand, Action onCollapse)
        {
            this.onExpand = onExpand;
            this.onCollapse = onCollapse;
        }

        public bool OnMenuItemActionExpand(IMenuItem item)
        {
            onExpand();
            return true;
        }

        public bool OnMenuItemActionCollapse(IMenuItem item)
        {
            onCollapse();
            return true;
        }
    }
}