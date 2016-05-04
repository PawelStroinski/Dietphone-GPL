using System;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;

namespace Dietphone.TargetBindings
{
    public class EditTextSelectRight : MvxAndroidTargetBinding
    {
        private readonly EditText target;

        public EditTextSelectRight(EditText target)
            : base(target)
        {
            this.target = target;
        }

        public override Type TargetType => typeof(bool);

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneTime;

        protected override void SetValueImpl(object target, object value)
        {
            if (!(bool)value)
                return;
            var text = this.target.Text;
            if (string.IsNullOrEmpty(text))
                return;
            this.target.SetSelection(text.Length, text.Length);
        }
    }
}