using System;
using Android.Views;

namespace Dietphone.Views.ListenerAdapters
{
    public class ActionExpand : Java.Lang.Object, IMenuItemOnActionExpandListener
    {
        private readonly Action onCollapse;
        private readonly Action onExpand;

        public ActionExpand(Action onCollapse, Action onExpand)
        {
            this.onCollapse = onCollapse;
            this.onExpand = onExpand;
        }

        public bool OnMenuItemActionCollapse(IMenuItem item)
        {
            onCollapse();
            return true;
        }

        public bool OnMenuItemActionExpand(IMenuItem item)
        {
            onExpand();
            return true;
        }
    }
}