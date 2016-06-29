using System;

namespace Dietphone.Tools
{
    public interface NavigationService
    {
        bool CanGoBack { get; }
        bool Navigate(Uri source);
        void GoBack();
    }
}
