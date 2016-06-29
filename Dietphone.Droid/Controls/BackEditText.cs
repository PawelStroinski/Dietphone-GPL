// Based on http://stackoverflow.com/a/5119485
using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Dietphone.Controls
{
    public sealed class BackEditText : EditText
    {
        public event EventHandler Back;

        public BackEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Up)
                OnBack(EventArgs.Empty);
            return base.OnKeyPreIme(keyCode, e);
        }

        private void OnBack(EventArgs e)
        {
            if (Back != null)
                Back(this, e);
        }
    }
}