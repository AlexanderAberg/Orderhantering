using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;

namespace ITSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderDbContext _db;

        public OrderService(OrderDbContext db)
        {
            _db = db;
        }

        public Order? GetById(int id)
        {
            return _db.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == id);
        }

        public IEnumerable<Order> GetAll()
        {
            return _db.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .ToList();
        }

        public IEnumerable<Order> GetByUserId(int userId)
        {
            return _db.Orders
                .Include(o => o.Product)
                .Where(o => o.UserId == userId)
                .ToList();
        }

        public void CreateOrder(int productId, int userId)
        {
            var product = _db.Products.Find(productId);
            if (product == null)
                throw new Exception("Produkten finns inte.");

            var order = new Order
            {
                ProductId = productId,
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Pending"
            };

            _db.Orders.Add(order);
            _db.SaveChanges();
        }

        public void UpdateOrder(Order order)
        {
            _db.Orders.Update(order);
            _db.SaveChanges();
        }

        public void DeleteOrder(int orderId, int userId, bool isAdmin)
        {
            var order = _db.Orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                throw new Exception("Ordern finns inte.");

            if (!isAdmin && order.UserId != userId)
                throw new UnauthorizedAccessException("Du kan bara ta bort dina egna ordrar.");

            _db.Orders.Remove(order);
            _db.SaveChanges();
        }
    }

}