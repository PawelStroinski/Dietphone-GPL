using System;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;

namespace Dietphone.Tools
{
    public class EditTextTextOnFocusLostTargetBinding : MvxAndroidTargetBinding
    {
        private readonly EditText target;

        public EditTextTextOnFocusLostTargetBinding(EditText target)
            : base(target)
        {
            this.target = target;
            target.FocusChange += Target_FocusChange;
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
    }
}