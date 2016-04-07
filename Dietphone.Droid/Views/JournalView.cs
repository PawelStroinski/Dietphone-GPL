using Android.App;
using Android.OS;
using Dietphone.ViewModels;

namespace Dietphone.Views
{
    [Activity]
    public class JournalView : ListingView<GroupingJournalViewModel>
    {
        private const byte EXPAND_TOP_GROUPS = 5;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            listView.ItemTemplateId = Resource.Layout.Journal_Item;
            for (int i = 0; i < EXPAND_TOP_GROUPS; i++)
                listView.ExpandGroup(i);
        }
    }
}