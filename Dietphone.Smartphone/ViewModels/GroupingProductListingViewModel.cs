using Dietphone.Models;
using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public class GroupingProductListingViewModel : ProductListingViewModel
    {
        public GroupingViewModel<ProductViewModel, CategoryViewModel> Grouping { get; private set; }

        public GroupingProductListingViewModel(Factories factories, BackgroundWorkerFactory workerFactory)
            : base(factories, workerFactory)
        {
            Grouping = new SortedGroupingViewModel<ProductViewModel, CategoryViewModel, string, string>(this,
                () => Products,
                keySelector: product => product.Category,
                predicate: product => product.Name.ContainsIgnoringCase(search),
                choose: Choose,
                itemSort: product => product.Name,
                groupSort: group => group.Key.Name);
        }
    }
}
