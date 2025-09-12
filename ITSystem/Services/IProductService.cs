using ITSystem.Data.Entities;
using System.Collections.Generic;

namespace ITSystem.Services
{
    public interface IProductService
    {
        IEnumerable<Product> GetAllProducts();
        Product? GetProductById(int id);
        void CreateProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(int id);
        void ListProducts();
    }
}
