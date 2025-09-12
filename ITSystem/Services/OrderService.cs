using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ITSystem.Services
{
    internal class OrderService : IOrderService
    {
        private readonly OrderDbContext _db;

        public OrderService(OrderDbContext db)
        {
            _db = db;
        }

        public List<Order> GetAllOrders()
        {
            return _db.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .ToList();
        }

        public Order? GetOrderById(int orderId)
        {
            return _db.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == orderId);
        }

        public void CreateOrder(Order order)
        {
            _db.Orders.Add(order);
            _db.SaveChanges();
        }

        public void UpdateOrder(Order order)
        {
            _db.Orders.Update(order);
            _db.SaveChanges();
        }

        public void DeleteOrder(int orderId)
        {
            var order = _db.Orders.Find(orderId);
            if (order != null)
            {
                _db.Orders.Remove(order);
                _db.SaveChanges();
            }
        }

        public void ListOrdersByUser(int userId)
        {
            var orders = _db.Orders
                .Include(o => o.Product)
                .Where(o => o.UserId == userId)
                .ToList();

            if (orders.Count == 0)
            {
                Console.WriteLine("Du har inga ordrar.");
                return;
            }

            foreach (var order in orders)
            {
                Console.WriteLine($"Order ID: {order.Id} | Produkt: {order.Product.Name} | Status: {order.Status} | Datum: {order.OrderDate}");
            }
        }

        public void CreateOrder(int userId)
        {
            var products = _db.Products.ToList();

            if (products.Count == 0)
            {
                Console.WriteLine("Inga produkter tillgängliga.");
                return;
            }

            Console.WriteLine("== Tillgängliga produkter ==");
            foreach (var p in products)
                Console.WriteLine($"ID: {p.Id} | {p.Name} - {p.Price:C}");

            Console.Write("Välj produkt-ID: ");
            if (!int.TryParse(Console.ReadLine(), out int productId))
            {
                Console.WriteLine("Ogiltigt ID.");
                return;
            }

            var product = _db.Products.Find(productId);
            if (product == null)
            {
                Console.WriteLine("Produkt hittades inte.");
                return;
            }

            var order = new Order
            {
                ProductId = product.Id,
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Pending"
            };

            _db.Orders.Add(order);
            _db.SaveChanges();

            Console.WriteLine("Order skapad.");
        }

        public void ModifyOrDeleteOwnOrder(int userId)
        {
            var orders = _db.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .Where(o => o.UserId == userId)
                .ToList();

            if (!orders.Any())
            {
                Console.WriteLine("Inga ordrar att visa.");
                return;
            }

            Console.WriteLine("== Dina ordrar ==");
            foreach (var o in orders)
                Console.WriteLine($"ID: {o.Id} | Produkt: {o.Product.Name} | Status: {o.Status}");

            Console.Write("Ange order-ID du vill ändra/ta bort: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ogiltigt ID.");
                return;
            }

            var order = _db.Orders
                .Include(o => o.Product)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                Console.WriteLine("Du har inte behörighet att hantera denna order eller den finns inte.");
                return;
            }

            Console.WriteLine("1. Ändra status");
            Console.WriteLine("2. Ta bort order");
            Console.Write("Val: ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Ny status (Pending, Approved, Canceled): ");
                var status = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(status))
                {
                    order.Status = status;

                    order.LastEditedByAdminId = null;

                    _db.SaveChanges();
                    Console.WriteLine("Order uppdaterad.");
                }
            }
            else if (choice == "2")
            {
                _db.Orders.Remove(order);
                _db.SaveChanges();
                Console.WriteLine("Order borttagen.");
            }
        }


        public void ManageAllOrders(int adminId)
        {
            var orders = _db.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .Include(o => o.LastEditedByAdmin)
                .ToList();

            if (!orders.Any())
            {
                Console.WriteLine("Inga ordrar att visa.");
                return;
            }

            Console.WriteLine("== Alla ordrar ==");
            foreach (var o in orders)
            {
                string editedBy = o.LastEditedByAdmin != null ? $" | Senast ändrad av: {o.LastEditedByAdmin.Username}" : "";
                Console.WriteLine($"ID: {o.Id} | Användare: {o.User.Username} | Produkt: {o.Product.Name} | Status: {o.Status}{editedBy}");
            }

            Console.Write("Ange order-ID att hantera: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ogiltigt ID.");
                return;
            }

            var order = orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                Console.WriteLine("Order hittades inte.");
                return;
            }

            Console.WriteLine("1. Ändra status");
            Console.WriteLine("2. Ta bort order");
            Console.Write("Val: ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Ny status (Pending, Approved, Canceled): ");
                var status = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(status))
                {
                    order.Status = status;
                    order.LastEditedByAdminId = adminId;
                    _db.SaveChanges();
                    Console.WriteLine("Order uppdaterad av admin.");
                }
            }
            else if (choice == "2")
            {
                _db.Orders.Remove(order);
                _db.SaveChanges();
                Console.WriteLine("Order borttagen av admin.");
            }
        }
    }
}
