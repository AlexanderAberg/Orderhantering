using System;

namespace ITSystem.Services
{
    public interface IOrderService
    {
        void ListOrdersByUser(int userId);
        void CreateOrder(int userId);
        void ModifyOrDeleteOwnOrder(int userId);
        void ManageAllOrders(int adminId);
    }
}
