using System.Collections.Generic;
using System.Windows.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.WindowsPhone.Views;

namespace Dietphone.Views
{
    public abstract class StateAdapterPage : MvxPhonePage
    {
        private const string stateKey = "bundle";

        protected override IMvxBundle LoadStateBundle(NavigationEventArgs navigationEventArgs)
        {
            if (State.ContainsKey(stateKey))
                return new MvxBundle((IDictionary<string, string>)State[stateKey]);
            else
                return null;
        }

        protected override void SaveStateBundle(NavigationEventArgs navigationEventArgs, IMvxBundle bundle)
        {
            State[stateKey] = bundle.Data;
        }
    }
}