using Dietphone.Tools;
using MvvmCross.Core.ViewModels;

namespace Dietphone.ViewModels
{
    public class ViewModelWithStateProvider : ViewModelBase
    {
        public StateProvider StateProvider { get; internal set; }

        public ViewModelWithStateProvider()
        {
            StateProvider = new MvxStateProvider();
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            ((MvxStateProvider)StateProvider).SaveStateToBundle(bundle);
        }

        protected override void ReloadFromBundle(IMvxBundle bundle)
        {
            ((MvxStateProvider)StateProvider).ReloadFromBundle(bundle);
        }
    }
}
