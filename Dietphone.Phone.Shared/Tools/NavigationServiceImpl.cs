using System;

namespace Dietphone.Tools
{
    public class NavigationServiceImpl : NavigationService
    {
        private System.Windows.Navigation.NavigationService service;

        public NavigationServiceImpl(System.Windows.Navigation.NavigationService service)
        {
            this.service = service;
        }

        public bool CanGoBack
        {
            get { return service.CanGoBack; }
        }

        public bool Navigate(Uri source)
        {
            return service.Navigate(source);
        }

        public void GoBack()
        {
            service.GoBack();
        }
    }
}
