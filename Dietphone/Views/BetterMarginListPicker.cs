// Idea from http://stackoverflow.com/questions/782251/can-you-override-just-part-of-a-control-template-in-silverlight

using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    public class BetterMarginListPicker : ListPicker
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var textBlock = GetTemplateChild("MultipleSelectionModeSummary") as TextBlock;
            var margin = textBlock.Margin;
            textBlock.Margin = new Thickness(margin.Left, 2, margin.Right, margin.Bottom);
        }
    }
}
