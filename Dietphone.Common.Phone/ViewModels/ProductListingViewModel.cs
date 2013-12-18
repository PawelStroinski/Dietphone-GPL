using Dietphone.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dietphone.Views;

namespace Dietphone.ViewModels
{
    public class ProductListingViewModel : SearchSubViewModel
    {
        public ObservableCollection<ProductViewModel> Products { get; private set; }
        public ObservableCollection<CategoryViewModel> Categories { get; private set; }
        public bool AddMru { get; set; }
        public event EventHandler<ChoosedEventArgs> Choosed;
        private readonly Factories factories;
        private readonly MaxCuAndFpuInCategories maxCuAndFpu;
        private readonly BackgroundWorkerFactory workerFactory;

        public ProductListingViewModel(Factories factories, BackgroundWorkerFactory workerFactory)
        {
            this.factories = factories;
            maxCuAndFpu = new MaxCuAndFpuInCategories(factories.Finder);
            this.workerFactory = workerFactory;
        }

        public override void Load()
        {
            if (Categories == null && Products == null)
            {
                var loader = new CategoriesAndProductsLoader(this, AddMru);
                loader.LoadAsync();
                loader.Loaded += delegate { OnLoaded(); };
            }
        }

        public override void Refresh()
        {
            if (Categories != null && Products != null)
            {
                maxCuAndFpu.Reset();
                var loader = new CategoriesAndProductsLoader(this, AddMru);
                loader.LoadAsync();
                loader.Loaded += delegate { OnRefreshed(); };
            }
        }

        public void Choose(ProductViewModel product)
        {
            var e = new ChoosedEventArgs()
            {
                Product = product
            };
            OnChoosed(e);
            if (!e.Handled)
            {
                Navigator.GoToProductEditing(product.Id);
            }
        }

        public override void Add(AddCommand command)
        {
            var product = factories.CreateProduct();
            product.AddedByUser = true;
            Navigator.GoToProductEditing(product.Id);
        }

        public ProductViewModel FindProduct(Guid productId)
        {
            var result = from product in Products
                         where product.Id == productId
                         select product;
            return result.FirstOrDefault();
        }

        public CategoryViewModel FindCategory(Guid categoryId)
        {
            var result = from category in Categories
                         where category.Id == categoryId
                         select category;
            return result.FirstOrDefault();
        }

        protected void OnChoosed(ChoosedEventArgs e)
        {
            if (Choosed != null)
            {
                Choosed(this, e);
            }
        }

        public class CategoriesAndProductsLoader : LoaderBase
        {
            private ObservableCollection<CategoryViewModel> categories;
            private ObservableCollection<ProductViewModel> products;
            private MaxCuAndFpuInCategories maxCuAndFpu;
            private CategoryViewModel mruCategory;
            private IList<Product> mruProducts;
            private bool addMru;

            public CategoriesAndProductsLoader(ProductListingViewModel viewModel, bool addMru)
                : base(viewModel.workerFactory)
            {
                this.viewModel = viewModel;
                factories = viewModel.factories;
                maxCuAndFpu = viewModel.maxCuAndFpu;
                this.addMru = addMru;
            }

            public CategoriesAndProductsLoader(Factories factories,
                BackgroundWorkerFactory workerFactory)
                : base(workerFactory)
            {
                this.factories = factories;
            }

            public ObservableCollection<CategoryViewModel> Categories
            {
                get
                {
                    if (categories == null)
                    {
                        LoadCategories();
                    }
                    return categories;
                }
            }

            protected override void DoWork()
            {
                LoadCategories();
                LoadProducts();
                AddMruCategoryAndProducts();
            }

            protected override void WorkCompleted()
            {
                AssignCategories();
                AssignProducts();
                base.WorkCompleted();
            }

            private void LoadCategories()
            {
                var models = factories.Categories;
                var unsortedViewModels = new List<CategoryViewModel>();
                foreach (var model in models)
                {
                    var viewModel = new CategoryViewModel(model, factories);
                    unsortedViewModels.Add(viewModel);
                }
                var sortedViewModels = unsortedViewModels.OrderBy(category => category.Name);
                categories = new ObservableCollection<CategoryViewModel>();
                foreach (var viewModel in sortedViewModels)
                {
                    categories.Add(viewModel);
                }
            }

            private void LoadProducts()
            {
                var models = factories.Products;
                products = new ObservableCollection<ProductViewModel>();
                foreach (var model in models)
                {
                    var viewModel = new ProductViewModel(model)
                    {
                        Categories = categories,
                        MaxCuAndFpu = maxCuAndFpu
                    };
                    products.Add(viewModel);
                }
            }

            private void AddMruCategoryAndProducts()
            {
                if (!addMru)
                    return;
                mruProducts = factories.MruProducts.Products;
                if (!mruProducts.Any())
                    return;
                AddMruCategory();
                AddMruProducts();
            }

            private void AssignCategories()
            {
                GetViewModel().Categories = categories;
                GetViewModel().OnPropertyChanged("Categories");
            }

            private void AssignProducts()
            {
                GetViewModel().Products = products;
                GetViewModel().OnPropertyChanged("Products");
            }

            private ProductListingViewModel GetViewModel()
            {
                return viewModel as ProductListingViewModel;
            }

            private void AddMruCategory()
            {
                var model = factories.CreateCategory();
                factories.Categories.Remove(model);
                model.Name = Translations.RecentlyUsed;
                mruCategory = new CategoryViewModel(model, factories);
                categories.Insert(0, mruCategory);
            }

            private void AddMruProducts()
            {
                foreach (var model in mruProducts)
                {
                    var viewModel = new ProductViewModel(model)
                    {
                        Categories = categories,
                        MaxCuAndFpu = maxCuAndFpu,
                        OverrideCategory = mruCategory
                    };
                    products.Add(viewModel);
                }
            }
        }
    }

    public class ChoosedEventArgs : EventArgs
    {
        public ProductViewModel Product { get; set; }
        public bool Handled { get; set; }
    }
}
