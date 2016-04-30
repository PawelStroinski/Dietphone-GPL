using System;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;

namespace Dietphone.TargetBindings
{
    public class EditTextSelectAllOnFocus : MvxAndroidTargetBinding
    {
        private readonly EditText target;

        public EditTextSelectAllOnFocus(EditText target)
            : base(target)
        {
            this.target = target;
        }

        public override Type TargetType => typeof(bool);

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneTime;

        protected override void SetValueImpl(object target, object value)
        {
            this.target.SetSelectAllOnFocus((bool)value);
        }
    }
}