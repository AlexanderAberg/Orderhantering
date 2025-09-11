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
            throw new NotImplementedException();
        }

        public void CreateOrder(int userId)
        {
            throw new NotImplementedException();
        }

        public void ModifyOrDeleteOwnOrder(int userId)
        {
            throw new NotImplementedException();
        }

        public void ManageAllOrders(int adminId)
        {
            throw new NotImplementedException();
        }
    }
}
