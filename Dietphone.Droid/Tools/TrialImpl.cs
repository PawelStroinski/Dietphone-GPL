// The Show method is from http://stackoverflow.com/a/11753070
using System;
using Android.App;
using Android.Content;

namespace Dietphone.Tools
{
    public sealed class TrialImpl : Trial
    {
        private const string MARKET_URI = "market://details?id={0}";
        private const string HTTPS_URL = "https://play.google.com/store/apps/details?id={0}";

        public void IsTrial(Action<bool> callback)
        {
#if DEBUG
            callback(true);
#else
            callback(true); // TODO: use in-app purchase
                            // http://developer.android.com/google/play/billing/billing_integrate.html#QueryPurchases
                            // (LVL can't be used for trial, see http://stackoverflow.com/a/5810198
                            // so in-app purchase has to be used instead)
            // https://components.xamarin.com/gettingstarted/xamarin.inappbilling
#endif
        }

        public void Show()
        {
            // TODO: To be replaced with http://developer.android.com/google/play/billing/billing_integrate.html#Purchase
            try
            {
                StartActivity(MARKET_URI);
            }
            catch (ActivityNotFoundException)
            {
                StartActivity(HTTPS_URL);
            }
        }

        private void StartActivity(string uriTemplate)
        {
            var context = Application.Context;
            var appPackageName = context.PackageName;
            var uri = string.Format(uriTemplate, appPackageName);
            var parsedUri = Android.Net.Uri.Parse(uri);
            var intent = new Intent(Intent.ActionView, parsedUri);
            context.StartActivity(intent);
        }
    }
}
