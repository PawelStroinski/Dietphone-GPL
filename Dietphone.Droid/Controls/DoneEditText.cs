// Based on http://stackoverflow.com/a/5077543
using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views.InputMethods;
using Android.Widget;

namespace Dietphone.Controls
{
    public sealed class DoneEditText : EditText
    {
        public event EventHandler Done;

        public DoneEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public override void OnEditorAction([GeneratedEnum] ImeAction actionCode)
        {
            if (actionCode == ImeAction.Done)
                OnDone(EventArgs.Empty);
        }

        private void OnDone(EventArgs e)
        {
            if (Done != null)
                Done(this, e);
        }
    }
}