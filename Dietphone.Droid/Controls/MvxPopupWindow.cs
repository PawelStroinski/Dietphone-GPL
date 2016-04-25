// Based on https://github.com/MvvmCross/MvvmCross/blob/4.0/MvvmCross/Binding/Droid/Views/MvxFrameControl.cs
// and http://stackoverflow.com/a/1967886
using System;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Attributes;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Platform;

namespace Dietphone.Controls
{
    public sealed class MvxPopupWindow : View, IMvxBindingContextOwner
    {
        public event EventHandler IsVisibleChanged;
        public event EventHandler AutoDissmissed;
        private object cachedDataContext;
        private bool isVisible, dissmissing;
        private PopupWindow popup;
        private readonly int templateId;
        private readonly IMvxAndroidBindingContext bindingContext;

        public MvxPopupWindow(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            templateId = MvxAttributeHelpers.ReadTemplateId(context, attrs);
            if (!(context is IMvxLayoutInflaterHolder))
                throw Mvx.Exception("The owning Context for a MvxPopupWindow must implement IMvxLayoutInflaterHolder");
            bindingContext = new MvxAndroidBindingContext(context, (IMvxLayoutInflaterHolder)context);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ClearAllBindings();
                cachedDataContext = null;
            }
            base.Dispose(disposing);
        }

        public IMvxBindingContext BindingContext
        {
            get { return bindingContext; }
            set { throw new NotImplementedException("BindingContext is readonly in MvxPopupWindow"); }
        }

        [MvxSetToNullAfterBinding]
        public object DataContext
        {
            get
            {
                return bindingContext.DataContext;
            }
            set
            {
                if (isVisible)
                    bindingContext.DataContext = value;
                else
                {
                    cachedDataContext = value;
                    if (bindingContext.DataContext != null)
                        bindingContext.DataContext = null;
                }
            }
        }

        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    if (value)
                        Show();
                    else
                        Dismiss();
                }
            }
        }

        private void Show()
        {
            if (cachedDataContext != null && DataContext == null)
                bindingContext.DataContext = cachedDataContext;
            popup = new PopupWindow(
                bindingContext.BindingInflate(templateId, null),
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                true);
            popup.SetBackgroundDrawable(new BitmapDrawable()); // This is needed, see http://stackoverflow.com/a/3122696
            popup.DismissEvent += delegate { HandleDismiss(); };
            popup.ShowAtLocation(this, GravityFlags.NoGravity, 0, 0);
            dissmissing = false;
        }

        private void Dismiss()
        {
            dissmissing = true;
            popup.Dismiss();
        }

        private void HandleDismiss()
        {
            if (popup == null)
                return;
            cachedDataContext = DataContext;
            bindingContext.DataContext = null;
            popup = null;
            if (!dissmissing)
            {
                isVisible = false;
                OnIsVisibleChanged(EventArgs.Empty);
                OnAutoDissmissed(EventArgs.Empty);
            }
        }

        private void OnIsVisibleChanged(EventArgs e)
        {
            if (IsVisibleChanged != null)
            {
                IsVisibleChanged(this, e);
            }
        }

        private void OnAutoDissmissed(EventArgs e)
        {
            if (AutoDissmissed != null)
            {
                AutoDissmissed(this, e);
            }
        }
    }
}