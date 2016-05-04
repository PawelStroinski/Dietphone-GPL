using Android.Widget;
using Dietphone.Tools;
using MvvmCross.Binding;

namespace Dietphone.TargetBindings
{
    public class ImageButtonOpaqueEnabled : TargetBindingBase<ImageButton, bool>
    {
        public ImageButtonOpaqueEnabled(ImageButton target)
            : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void DoSetValue(ImageButton target, bool value)
        {
            target.Enabled = value;
            target.Clickable = value;
            target.ImageAlpha = value.ToAlpha();
        }
    }
}