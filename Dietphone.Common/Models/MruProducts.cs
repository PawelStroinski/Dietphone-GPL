using System;
using System.Collections.Generic;

namespace Dietphone.Models
{
    public class MruProducts
    {
        public const int MAX_COUNT = 10;
        private IList<Guid> productIds;
        private Factories factories;

        public MruProducts(IList<Guid> productIds, Factories factories)
        {
            this.productIds = productIds;
            this.factories = factories;
        }

        public IList<Product> Products
        {
            get
            {
                var products = new List<Product>();
                foreach (var id in productIds)
                {
                    var found = this.factories.Products.FindById(id);
                    if (found != null)
                        products.Add(found);
                }
                return products;
            }
        }

        public void AddProduct(Product product)
        {
            if (productIds.Contains(product.Id))
                productIds.Remove(product.Id);
            productIds.Insert(0, product.Id);
            while (productIds.Count > MAX_COUNT)
                productIds.RemoveAt(MAX_COUNT);
        }
    }
}
