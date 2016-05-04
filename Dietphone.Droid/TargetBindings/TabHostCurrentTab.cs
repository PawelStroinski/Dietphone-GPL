// The SubscribeToEvents & Dispose methods based on http://stackoverflow.com/a/19221385
using Android.Widget;
using MvvmCross.Binding;

namespace Dietphone.TargetBindings
{
    public class TabHostCurrentTab : TargetBindingBase<TabHost, int>
    {
        private bool subscribed;

        public TabHostCurrentTab(TabHost target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            Do(target => target.TabChanged += Target_TabChanged);
            subscribed = true;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && subscribed)
            {
                Do(target => target.TabChanged -= Target_TabChanged);
                subscribed = false;
            }
            base.Dispose(isDisposing);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void DoSetValue(TabHost target, int value)
        {
            target.CurrentTab = value;
        }

        private void Target_TabChanged(object sender, TabHost.TabChangeEventArgs e)
        {
            Do(target => FireValueChanged(target.CurrentTab));
        }
    }
}