// Based on http://stackoverflow.com/a/23560895
using System;
using Android.Webkit;

namespace Dietphone.Views.Adapters
{
    public class WebViewClientImpl : WebViewClient
    {
        private readonly Action<string> onNavigating;

        public WebViewClientImpl(Action<string> onNavigating)
        {
            this.onNavigating = onNavigating;
        }

        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            onNavigating(url);
            return base.ShouldOverrideUrlLoading(view, url);
        }
    }
}