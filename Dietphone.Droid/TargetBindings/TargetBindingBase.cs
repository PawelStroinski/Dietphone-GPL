using System;
using MvvmCross.Binding.Droid.Target;

namespace Dietphone.TargetBindings
{
    public abstract class TargetBindingBase<TTarget, TValue> : MvxAndroidTargetBinding
    {
        public TargetBindingBase(TTarget target)
            : base(target)
        {
        }

        public override Type TargetType => typeof(TValue);

        protected override void SetValueImpl(object target, object value)
        {
            if (target != null)
                DoSetValue((TTarget)target, (TValue)value);
        }

        protected virtual void DoSetValue(TTarget target, TValue value)
        {
        }

        protected void Do(Action<TTarget> action)
        {
            var target = Target;
            if (target != null)
                action((TTarget)target);
        }
    }
}