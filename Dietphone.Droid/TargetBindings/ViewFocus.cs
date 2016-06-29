using Android.Views;
using MvvmCross.Binding;

namespace Dietphone.TargetBindings
{
    public class ViewFocus : TargetBindingBase<View, bool>
    {
        public ViewFocus(View target)
            : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneTime;

        protected override void DoSetValue(View target, bool value)
        {
            if (value)
                target.RequestFocus();
        }
    }
}