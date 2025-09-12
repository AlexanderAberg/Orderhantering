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
                .Include(o => o.LastEditedByAdmin)
                .Where(o => o.UserId == userId)
                .ToList();

            Console.WriteLine("== Dina ordrar ==");
            foreach (var o in orders)
            {
                string editedInfo = o.LastEditedByAdmin != null
                    ? $"(Ändrad av admin: {o.LastEditedByAdmin.Username})"
                    : "";
                Console.WriteLine($"Order ID: {o.Id} | {o.Product.Name} | {o.OrderDate} | Status: {o.Status} {editedInfo}");
            }

            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }


        public void CreateOrder(int userId)
        {
            Console.Clear();
            Console.WriteLine("== Skapa order ==");

            var products = _db.Products.ToList();
            foreach (var product in products)
            {
                Console.WriteLine($"ID: {product.Id} | {product.Name} - {product.Description} ({product.Price:C})");
            }

            Console.Write("\nAnge produkt-ID: ");
            if (int.TryParse(Console.ReadLine(), out int productId))
            {
                var product = _db.Products.Find(productId);
                if (product != null)
                {
                    var order = new Order
                    {
                        ProductId = productId,
                        UserId = userId,
                        OrderDate = DateTime.Now,
                        Status = "Pending"
                    };
                    _db.Orders.Add(order);
                    _db.SaveChanges();
                    Console.WriteLine("Order skapad!");
                }
                else
                {
                    Console.WriteLine("Produkt hittades inte.");
                }
            }
            else
            {
                Console.WriteLine("Ogiltigt ID.");
            }

            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }


        public void ModifyOrDeleteOwnOrder(int userId)
        {
            Console.Clear();
            var orders = _db.Orders
                .Include(o => o.Product)
                .Where(o => o.UserId == userId)
                .ToList();

            if (!orders.Any())
            {
                Console.WriteLine("Inga ordrar hittades.");
                return;
            }

            foreach (var order in orders)
            {
                Console.WriteLine($"ID: {order.Id} | Produkt: {order.Product.Name} | Status: {order.Status}");
            }

            Console.Write("\nAnge order-ID du vill ändra eller ta bort: ");
            if (int.TryParse(Console.ReadLine(), out int orderId))
            {
                var order = _db.Orders.FirstOrDefault(o => o.Id == orderId && o.UserId == userId);
                if (order == null)
                {
                    Console.WriteLine("Order hittades inte.");
                }
                else
                {
                    Console.WriteLine("1. Ändra status");
                    Console.WriteLine("2. Ta bort order");
                    Console.Write("Val: ");
                    var choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        Console.Write("Ny status: ");
                        order.Status = Console.ReadLine();
                        _db.SaveChanges();
                        Console.WriteLine("Order uppdaterad.");
                    }
                    else if (choice == "2")
                    {
                        _db.Orders.Remove(order);
                        _db.SaveChanges();
                        Console.WriteLine("Order borttagen.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Ogiltigt ID.");
            }

            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }


        public void ManageAllOrders(int adminId)
        {
            Console.Clear();
            var orders = _db.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .ToList();

            foreach (var o in orders)
            {
                Console.WriteLine($"ID: {o.Id} | Användare: {o.User.Username} | Produkt: {o.Product.Name} | Status: {o.Status}");
            }

            Console.Write("\nAnge order-ID du vill ändra eller ta bort: ");
            if (int.TryParse(Console.ReadLine(), out int orderId))
            {
                var order = _db.Orders.Include(o => o.User).FirstOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    Console.WriteLine("Order hittades inte.");
                }
                else
                {
                    Console.WriteLine("1. Ändra status");
                    Console.WriteLine("2. Ta bort order");
                    Console.Write("Val: ");
                    var choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        Console.Write("Ny status: ");
                        order.Status = Console.ReadLine();
                        order.LastEditedByAdminId = adminId;
                        _db.SaveChanges();
                        Console.WriteLine("Order uppdaterad av admin.");
                    }
                    else if (choice == "2")
                    {
                        _db.Orders.Remove(order);
                        _db.SaveChanges();
                        Console.WriteLine("Order borttagen av admin.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Ogiltigt ID.");
            }

            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

    }
}
