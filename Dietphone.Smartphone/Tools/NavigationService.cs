using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dietphone.Tools
{
    public interface NavigationService
    {
        bool CanGoBack { get; }
        bool Navigate(Uri source);
        void GoBack();
    }

    public interface NavigationContext
    {
        IDictionary<string, string> QueryString { get; }
    }
}
