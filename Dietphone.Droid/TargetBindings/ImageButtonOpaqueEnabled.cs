using System;
using Android.Widget;
using Dietphone.Tools;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;

namespace Dietphone.TargetBindings
{
    public class ImageButtonOpaqueEnabled : MvxAndroidTargetBinding
    {
        private readonly ImageButton target;

        public ImageButtonOpaqueEnabled(ImageButton target)
            : base(target)
        {
            this.target = target;
        }

        public override Type TargetType => typeof(bool);

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void SetValueImpl(object target, object value)
        {
            var enabled = (bool)value;
            this.target.Enabled = enabled;
            this.target.ImageAlpha = enabled.ToAlpha();
        }
    }
}