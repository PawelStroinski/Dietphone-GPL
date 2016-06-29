using System;
using Dietphone.Views;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;

namespace Dietphone.Tools
{
    public static class ExceptionLogging
    {
        public static void Register()
        {
            var appDomain = AppDomain.CurrentDomain;
            appDomain.UnhandledException += AppDomain_UnhandledException;
        }

        private static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
            else
            {
                var exception = e.ExceptionObject;
                LogException(exception.ToString());
            }
        }

        private static void LogException(string exception)
        {
            Logger.Error(exception);
            var messageDialog = new MessageDialogImpl();
            if (messageDialog.Confirm(Translations.BugInAppReportToTheAuthor, Translations.Bug))
                EmailException(exception);
        }

        private static void EmailException(string exception)
        {
            var body = string.Format(Translations.IWouldLikeToReportTheFollowingBug, exception);
            var subjectAndBody = new Tuple<string, string>(Translations.Bug, body);
            var activityHolder = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = activityHolder.Activity;
            if (activity != null)
                activity.LaunchEmail("info@pabloware.com", subjectAndBody);
        }
    }
}