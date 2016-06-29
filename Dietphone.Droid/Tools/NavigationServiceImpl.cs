// GoBack method based on https://github.com/MvvmCross/MvvmCross-Tutorials/blob/master/Sample%20-%20CustomerManagement/CustomerManagement%20-%20AutoViews/CustomerManagement.Droid/SimpleDroidViewModelCloser.cs
using System;
using Android.App;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;

namespace Dietphone.Tools
{
    public class NavigationServiceImpl : NavigationService
    {
        public bool CanGoBack
        {
            get { return Activity != null; }
        }

        public bool Navigate(Uri source)
        {
            throw new PlatformNotSupportedException();
        }

        public void GoBack()
        {
            Activity.Finish();
        }

        private Activity Activity
        {
            get
            {
                var holder = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
                return holder.Activity;
            }
        }
    }
}
