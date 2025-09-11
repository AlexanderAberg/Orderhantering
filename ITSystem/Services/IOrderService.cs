using ITSystem.Data.Entities;

namespace ITSystem.Services
{
    public interface IOrderService
    {
        Order? GetById(int id);
        IEnumerable<Order> GetAll();
        IEnumerable<Order> GetByUserId(int userId);
        void CreateOrder(int productId, int userId);
        void UpdateOrder(Order order);
        void DeleteOrder(int orderId, int userId, bool isAdmin);
    }

}