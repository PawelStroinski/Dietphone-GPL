using Dietphone.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dietphone.ViewModels
{
    public class ProductListingViewModel : SubViewModel
    {
        public ObservableCollection<ProductViewModel> Products { get; private set; }
        public ObservableCollection<CategoryViewModel> Categories { get; private set; }
        public event EventHandler DescriptorsUpdating;
        public event EventHandler DescriptorsUpdated;
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
                var loader = new CategoriesAndProductsLoader(this);
                loader.LoadAsync();
                loader.Loaded += delegate { OnLoaded(); };
            }
        }

        public override void Refresh()
        {
            if (Categories != null && Products != null)
            {
                maxCuAndFpu.Reset();
                var loader = new CategoriesAndProductsLoader(this);
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

        public override void Add()
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

        protected override void OnSearchChanged()
        {
            OnDescriptorsUpdating();
            UpdateFilterDescriptors();
            OnDescriptorsUpdated();
        }

        protected virtual void UpdateFilterDescriptors()
        {
        }

        protected void OnChoosed(ChoosedEventArgs e)
        {
            if (Choosed != null)
            {
                Choosed(this, e);
            }
        }

        protected void OnDescriptorsUpdating()
        {
            if (DescriptorsUpdating != null)
            {
                DescriptorsUpdating(this, EventArgs.Empty);
            }
        }

        protected void OnDescriptorsUpdated()
        {
            if (DescriptorsUpdated != null)
            {
                DescriptorsUpdated(this, EventArgs.Empty);
            }
        }

        public class CategoriesAndProductsLoader : LoaderBase
        {
            private ObservableCollection<CategoryViewModel> categories;
            private ObservableCollection<ProductViewModel> products;
            private MaxCuAndFpuInCategories maxCuAndFpu;

            public CategoriesAndProductsLoader(ProductListingViewModel viewModel)
                : base(viewModel.workerFactory)
            {
                this.viewModel = viewModel;
                factories = viewModel.factories;
                maxCuAndFpu = viewModel.maxCuAndFpu;
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
        }
    }

    public class ChoosedEventArgs : EventArgs
    {
        public ProductViewModel Product { get; set; }
        public bool Handled { get; set; }
    }
}
