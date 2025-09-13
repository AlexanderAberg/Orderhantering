using OTSystem.Models;
using System;
using System.Threading.Tasks;

namespace OTSystem.Services
{
    public class ProductionLineService
    {
        private ProductionStatusModel _currentStatus = new ProductionStatusModel
        {
            Status = "Idle",
            Timestamp = DateTime.Now
        };

        public async Task StartProductionAsync(OrderModel order)
        {
            Console.WriteLine($"[OT] Startar produktion av: {order.ProductName}, antal: {order.Quantity}");

            _currentStatus = new ProductionStatusModel
            {
                OrderId = order.Id,
                Status = "In Progress",
                Timestamp = DateTime.Now
            };

            await Task.Delay(2500); 

            _currentStatus.Status = "Completed";
            _currentStatus.Timestamp = DateTime.Now;

            Console.WriteLine("[OT] Produktion klar.");
        }

        public async Task<ProductionStatusModel> ProcessOrderAsync(OrderModel order)
        {
            Console.WriteLine($"\n== Produktionsstart för Order ID: {order.Id} ==");

            _currentStatus = new ProductionStatusModel
            {
                OrderId = order.Id,
                Status = "In Progress",
                Timestamp = DateTime.Now
            };

            await Task.Delay(2500); 

            _currentStatus.Status = "Completed";
            _currentStatus.Timestamp = DateTime.Now;

            Console.WriteLine($"Produktion klar för produkt: {order.ProductName}");

            return _currentStatus;
        }

        public ProductionStatusModel GetCurrentStatus()
        {
            return _currentStatus;
        }
    }
}
