// The SubscribeToEvents & Dispose methods based on http://stackoverflow.com/a/19221385
// and Target_EditorAction on http://stackoverflow.com/a/5077543
using System;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Dietphone.Controls;
using MvvmCross.Binding;

namespace Dietphone.TargetBindings
{
    public class BackEditTextTextOnFocusLost : TargetBindingBase<BackEditText, string>
    {
        private bool subscribed;

        public BackEditTextTextOnFocusLost(BackEditText target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            Do(target =>
            {
                target.FocusChange += Target_FocusChange;
                target.Back += Target_Back;
                target.EditorAction += Target_EditorAction;
            });
            subscribed = true;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && subscribed)
            {
                Do(target =>
                {
                    target.FocusChange -= Target_FocusChange;
                    target.Back -= Target_Back;
                    target.EditorAction -= Target_EditorAction;
                });
                subscribed = false;
            }
            base.Dispose(isDisposing);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void DoSetValue(BackEditText target, string value)
        {
            target.Text = value;
        }

        private void Target_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
                FireValueChanged();
        }

        private void Target_Back(object sender, EventArgs e)
        {
            FireValueChanged();
        }

        private void Target_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Done)
                FireValueChanged();
            e.Handled = false;
        }

        private void FireValueChanged()
        {
            Do(target => FireValueChanged(target.Text));
        }
    }
}