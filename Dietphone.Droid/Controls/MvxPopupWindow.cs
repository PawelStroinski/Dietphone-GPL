// Based on https://github.com/MvvmCross/MvvmCross/blob/4.0/MvvmCross/Binding/Droid/Views/MvxFrameControl.cs
// and http://stackoverflow.com/a/1967886, http://stackoverflow.com/a/27147729
using System;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Dietphone.Tools;
using MvvmCross.Binding.Attributes;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Platform;

namespace Dietphone.Controls
{
    public sealed class MvxPopupWindow : View, IMvxBindingContextOwner
    {
        public bool SoftInput { get; set; }
        public event EventHandler IsVisibleChanged;
        public event EventHandler Dissmissed;
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
            popup.AnimationStyle = Android.Resource.Style.AnimationTranslucent;
            popup.ShowAtLocation(this, GravityFlags.NoGravity, 0, 0);
            if (SoftInput)
                ShowSoftInput();
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
                OnDissmissed(EventArgs.Empty);
            }
        }

        private void ShowSoftInput()
        {
            var inputManager = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
            inputManager.ToggleSoftInput(ShowFlags.Implicit, HideSoftInputFlags.NotAlways);
        }

        private void OnIsVisibleChanged(EventArgs e)
        {
            if (IsVisibleChanged != null)
            {
                IsVisibleChanged(this, e);
            }
        }

        private void OnDissmissed(EventArgs e)
        {
            if (Dissmissed != null)
            {
                Dissmissed(this, e);
            }
        }
    }
}