using System;

namespace Dietphone.ViewModels
{
    public abstract class SearchSubViewModel : SubViewModel
    {
        public event EventHandler DescriptorsUpdating;
        public event EventHandler DescriptorsUpdated;

        protected override void OnSearchChanged()
        {
            OnDescriptorsUpdating();
            UpdateFilterDescriptors();
            OnDescriptorsUpdated();
        }

        protected virtual void UpdateFilterDescriptors()
        {
        }

        protected void OnDescriptorsUpdating()
        {
            if (DescriptorsUpdating != null)
            {
                DescriptorsUpdating(this, EventArgs.Empty);
            }
        }

        protected void OnDescriptorsUpdated()
        {
            if (DescriptorsUpdated != null)
            {
                DescriptorsUpdated(this, EventArgs.Empty);
            }
        }
    }
}
