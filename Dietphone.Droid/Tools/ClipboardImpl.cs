// Based on http://stackoverflow.com/a/238297
using Android.App;
using Android.Content;

namespace Dietphone.Tools
{
    public class ClipboardImpl : Clipboard
    {
        public void Set(string text)
        {
            var context = Application.Context;
            var clipboard = (ClipboardManager)context.GetSystemService(Context.ClipboardService);
            var clip = ClipData.NewPlainText(text, text);
            clipboard.PrimaryClip = clip;
        }
    }
}