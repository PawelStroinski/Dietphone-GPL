using System;
using Android.Content;
using Android.Util;
using Android.Widget;

namespace Dietphone.Controls
{
    public sealed class LockToggleButton : ImageButton
    {
        public event EventHandler IsCheckedChanged;
        private bool isChecked;
        private int image;

        public LockToggleButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Click += View_Click;
        }

        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                SetImage();
            }
        }

        private void View_Click(object sender, System.EventArgs e)
        {
            IsChecked = !IsChecked;
            OnIsCheckedChanged(EventArgs.Empty);
        }

        private void SetImage()
        {
            var newImage = IsChecked ? Resource.Drawable.ic_lock_open : Resource.Drawable.ic_lock;
            if (image != newImage)
            {
                SetImageResource(newImage);
                image = newImage;
            }
        }

        private void OnIsCheckedChanged(EventArgs e)
        {
            if (IsCheckedChanged != null)
            {
                IsCheckedChanged(this, e);
            }
        }
    }
}