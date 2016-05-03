using System;
using Android.Views;

namespace Dietphone.Adapters
{
    public class ClickListener : Java.Lang.Object, View.IOnClickListener
    {
        private readonly Action onClick;

        public ClickListener(Action onClick)
        {
            this.onClick = onClick;
        }

        public void OnClick(View v)
        {
            onClick();
        }
    }
}