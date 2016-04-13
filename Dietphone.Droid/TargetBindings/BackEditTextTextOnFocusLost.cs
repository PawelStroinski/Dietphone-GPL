// The SubscribeToEvents & Dispose methods based on http://stackoverflow.com/a/19221385
using System;
using Android.Views;
using Dietphone.Controls;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;

namespace Dietphone.TargetBindings
{
    public class BackEditTextTextOnFocusLost : MvxAndroidTargetBinding
    {
        private bool subscribed;
        private readonly BackEditText target;

        public BackEditTextTextOnFocusLost(BackEditText target)
            : base(target)
        {
            this.target = target;
        }

        public override void SubscribeToEvents()
        {
            target.FocusChange += Target_FocusChange;
            target.Back += Target_Back;
            subscribed = true;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && subscribed)
            {
                target.FocusChange -= Target_FocusChange;
                target.Back -= Target_Back;
                subscribed = false;
            }
            base.Dispose(isDisposing);
        }

        public override Type TargetType => typeof(string);

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void SetValueImpl(object target, object value)
        {
            this.target.Text = (string)value;
        }

        private void Target_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
                FireValueChanged(target.Text);
        }

        private void Target_Back(object sender, EventArgs e)
        {
            FireValueChanged(target.Text);
        }
    }
}