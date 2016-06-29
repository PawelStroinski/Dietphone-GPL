// Used info on subclassing the Application class from http://stackoverflow.com/a/21431198
using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Dietphone.Tools;
using MvvmCross.Platform;

namespace Dietphone
{
#if DEBUG
    [Application(Debuggable = true)]
#else
    [Application(Debuggable = false)]
#endif
    public sealed class DroidApplication : Application
    {
        public DroidApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            ExceptionLogging.Register();
            var callbacks = new ActivityLifecycleCallbacks();
            RegisterActivityLifecycleCallbacks(callbacks);
        }

        private sealed class ActivityLifecycleCallbacks : Java.Lang.Object, IActivityLifecycleCallbacks
        {
            private int startedActivitiesCount;

            public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
            {
            }

            public void OnActivityDestroyed(Activity activity)
            {
                TrialImpl.Disconnect();
            }

            public void OnActivityPaused(Activity activity)
            {
            }

            public void OnActivityResumed(Activity activity)
            {
            }

            public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
            {
            }

            public void OnActivityStarted(Activity activity)
            {
                startedActivitiesCount++;
                Mvx.Trace("Increased startedActivitiesCount to " + startedActivitiesCount);
            }

            public void OnActivityStopped(Activity activity)
            {
                startedActivitiesCount--;
                Mvx.Trace("Decreased startedActivitiesCount to " + startedActivitiesCount);
                if (startedActivitiesCount <= 0)
                {
                    var factories = MyApp.Factories;
                    factories.Save();
                }
            }
        }
    }
}