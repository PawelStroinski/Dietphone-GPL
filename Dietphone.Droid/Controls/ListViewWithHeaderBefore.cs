// Based on: http://kboyarshinov.com/android/autoinflate-v0-1/
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Dietphone.Tools;
using MvvmCross.Binding.Droid.Views;

namespace Dietphone.Controls
{
    public sealed class ListViewWithHeaderBefore : MvxListView
    {
        public ListViewWithHeaderBefore(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            var parent = Parent as ViewGroup;
            if (parent == null)
                return;
            var children = parent.GetChildren();
            var index = children.IndexOf(this);
            if (index <= 0)
                return;
            var header = children[index - 1];
            parent.RemoveView(header);
            header.LayoutParameters = new LayoutParams(AbsListView.LayoutParams.WrapContent,
                AbsListView.LayoutParams.WrapContent);
            this.AddHeaderView(header);
        }
    }
}