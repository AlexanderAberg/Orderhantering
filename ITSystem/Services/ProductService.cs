using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly OrderDbContext _db;

        public ProductService(OrderDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _db.Products.ToList();
        }

        public Product? GetProductById(int id)
        {
            return _db.Products.FirstOrDefault(p => p.Id == id);
        }

        public void CreateProduct(Product product)
        {
            _db.Products.Add(product);
            _db.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            _db.Products.Update(product);
            _db.SaveChanges();
        }

        public void DeleteProduct(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _db.Products.Remove(product);
                _db.SaveChanges();
            }
        }

        public void ListProducts()
        {
            var products = _db.Products.ToList();

            Console.WriteLine("== Produkter ==");
            foreach (var product in products)
            {
                Console.WriteLine($"ID: {product.Id} | {product.Name} - {product.Description} ({product.Price:C})");
            }

            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }
    }
}
