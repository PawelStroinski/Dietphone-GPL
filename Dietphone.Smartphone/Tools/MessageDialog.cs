namespace Dietphone.Tools
{
    public interface MessageDialog
    {
        void Show(string text);
        void Show(string text, string caption);
        bool Confirm(string text, string caption);
        string Input(string text, string caption);
        string Input(string text, string caption, string value);
        string Input(string text, string caption, string value, InputType type);
    }

    public enum InputType { Default, Email, Url }
}
