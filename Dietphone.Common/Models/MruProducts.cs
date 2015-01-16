using System;
using System.Collections.Generic;
using System.Linq;

namespace Dietphone.Models
{
    public interface MruProducts
    {
        IList<Product> Products { get; }
        void AddProduct(Product product);
    }

    public class MruProductsImpl : MruProducts
    {
        private IList<Guid> productIds;
        private Factories factories;
        private byte maxCount;

        public MruProductsImpl(IList<Guid> productIds, Factories factories, byte maxCount)
        {
            this.productIds = productIds;
            this.factories = factories;
            this.maxCount = maxCount;
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
                return products
                    .Take(maxCount)
                    .ToList();
            }
        }

        public void AddProduct(Product product)
        {
            if (productIds.Contains(product.Id))
                productIds.Remove(product.Id);
            productIds.Insert(0, product.Id);
            while (productIds.Count > maxCount)
                productIds.RemoveAt(maxCount);
        }
    }
}
