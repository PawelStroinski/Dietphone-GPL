using System.Collections.Generic;
using MvvmCross.Core.ViewModels;

namespace Dietphone.Tools
{
    public interface StateProvider
    {
        IDictionary<string, object> State { get; }
        bool IsOpened { get; }
    }

    public class MvxStateProvider : StateProvider
    {
        public bool IsOpened { get; private set; }
        public IDictionary<string, object> State { get; private set; }
        private IDictionary<string, string> storage;

        public MvxStateProvider()
        {
            IsOpened = true;
            InitializeFrom(new Dictionary<string, string>());
        }

        public void ReloadFromBundle(IMvxBundle bundle)
        {
            IsOpened = true;
            InitializeFrom(bundle.Data);
        }

        public void SaveStateToBundle(IMvxBundle bundle)
        {
            IsOpened = false;
            WriteTo(bundle.Data);
        }

        private void InitializeFrom(IDictionary<string, string> source)
        {
            storage = source;
            State = new StateSerializer(storage);
        }

        private void WriteTo(IDictionary<string, string> destination)
        {
            destination.Clear();
            foreach (var kvp in storage)
                destination.Add(kvp);
        }
    }
}
