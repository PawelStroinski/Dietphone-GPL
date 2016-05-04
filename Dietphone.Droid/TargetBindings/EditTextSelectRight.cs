using Android.Widget;
using MvvmCross.Binding;

namespace Dietphone.TargetBindings
{
    public class EditTextSelectRight : TargetBindingBase<EditText, bool>
    {
        public EditTextSelectRight(EditText target)
            : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneTime;

        protected override void DoSetValue(EditText target, bool value)
        {
            if (!value)
                return;
            var text = target.Text;
            if (string.IsNullOrEmpty(text))
                return;
            target.SetSelection(text.Length, text.Length);
        }
    }
}