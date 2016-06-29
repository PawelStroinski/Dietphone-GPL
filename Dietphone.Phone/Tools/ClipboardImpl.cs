namespace Dietphone.Tools
{
    public class ClipboardImpl : Clipboard
    {
        public void Set(string text)
        {
            System.Windows.Clipboard.SetText(text);
        }
    }
}
