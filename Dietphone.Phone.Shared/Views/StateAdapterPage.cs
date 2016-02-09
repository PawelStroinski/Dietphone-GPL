using System.Collections.Generic;
using System.Windows.Navigation;
using MvvmCross.Core.ViewModels;

namespace Dietphone.Views
{
    public abstract class StateAdapterPage : InitializedPage
    {
        private const string STATE_KEY = "bundle";

        protected override IMvxBundle LoadStateBundle(NavigationEventArgs navigationEventArgs)
        {
            if (State.ContainsKey(STATE_KEY))
                return new MvxBundle((IDictionary<string, string>)State[STATE_KEY]);
            else
                return null;
        }

        protected override void SaveStateBundle(NavigationEventArgs navigationEventArgs, IMvxBundle bundle)
        {
            State[STATE_KEY] = bundle.Data;
        }
    }
}