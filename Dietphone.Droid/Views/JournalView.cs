using Android.App;
using Android.OS;
using Dietphone.ViewModels;

namespace Dietphone.Views
{
    [Activity]
    public class JournalView : ListingView<GroupingJournalViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            listView.ItemTemplateId = Resource.Layout.Journal_Item;
        }
    }
}