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
            Grouping = new GroupingViewModel<ProductViewModel, CategoryViewModel>(this, () => Products,
                            keySelector: product => product.Category,
                            predicate: product => product.Name.ContainsIgnoringCase(search));
        }
    }
}
