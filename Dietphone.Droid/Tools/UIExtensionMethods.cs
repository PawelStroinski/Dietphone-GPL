using Android.Views;

namespace Dietphone.Tools
{
    public static class UIExtensionMethods
    {
        public static string Capitalize(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            else
                return input.Substring(0, 1).ToUpper() + input.Substring(1);
        }

        public static IMenuItem SetTitleCapitalized(this IMenuItem item, string title)
        {
            item.SetTitle(title.Capitalize());
            return item;
        }
    }
}
