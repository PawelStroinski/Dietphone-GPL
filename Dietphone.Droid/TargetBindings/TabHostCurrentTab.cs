// The SubscribeToEvents & Dispose methods based on http://stackoverflow.com/a/19221385
using System;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;

namespace Dietphone.TargetBindings
{
    public class TabHostCurrentTab : MvxAndroidTargetBinding
    {
        private bool subscribed;
        private readonly TabHost target;

        public TabHostCurrentTab(TabHost target)
            : base(target)
        {
            this.target = target;
        }

        public override void SubscribeToEvents()
        {
            target.TabChanged += Target_TabChanged;
            subscribed = true;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && subscribed)
            {
                target.TabChanged -= Target_TabChanged;
                subscribed = false;
            }
            base.Dispose(isDisposing);
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