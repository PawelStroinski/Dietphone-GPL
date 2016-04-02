using System;

namespace Dietphone.ViewModels
{
    public abstract class SearchSubViewModel : SubViewModel
    {
        public event EventHandler DescriptorsUpdating;
        public event EventHandler UpdateFilterDescriptors;
        public event EventHandler DescriptorsUpdated;

        protected override void OnSearchChanged()
        {
            OnDescriptorsUpdating();
            OnUpdateFilterDescriptors();
            OnDescriptorsUpdated();
        }

        protected void OnDescriptorsUpdating()
        {
            if (DescriptorsUpdating != null)
            {
                DescriptorsUpdating(this, EventArgs.Empty);
            }
        }

        protected virtual void OnUpdateFilterDescriptors()
        {
            if (UpdateFilterDescriptors != null)
            {
                UpdateFilterDescriptors(this, EventArgs.Empty);
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
