using System;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;

namespace Dietphone.Tools
{
    public class TabHostCurrentTabTargetBinding : MvxAndroidTargetBinding
    {
        private readonly TabHost target;

        public TabHostCurrentTabTargetBinding(TabHost target)
            : base(target)
        {
            this.target = target;
            target.TabChanged += Target_TabChanged;
        }

        public override Type TargetType => typeof(int);

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void SetValueImpl(object target, object value)
        {
            this.target.CurrentTab = (int)value;
        }

        private void Target_TabChanged(object sender, TabHost.TabChangeEventArgs e)
        {
            FireValueChanged(target.CurrentTab);
        }
    }
}