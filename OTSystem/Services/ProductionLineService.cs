using OTSystem.Models;
using System;
using System.Threading.Tasks;

namespace OTSystem.Services
{
    public class ProductionLineService
    {
        private readonly JiraService _jira;
        private ProductionStatusModel _currentStatus = new ProductionStatusModel
        {
            Status = "Idle",
            Timestamp = DateTime.Now
        };

        public ProductionLineService(JiraService jira)
        {
            _jira = jira;
        }

        public async Task StartProductionAsync(OrderModel order)
        {
            Console.WriteLine($"[OT] Startar produktion av: {order.ProductName}, antal: {order.Quantity}");

            try
            {
                var summary = $"OT Order {order.Id}: {order.ProductName} x{order.Quantity}";
                var description =
                    $@"Order created on OT side
                    Order Id: {order.Id}
                    Product: {order.ProductName}
                    Quantity: {order.Quantity}
                    Status: Pending production start";

                var issueKey = await _jira.CreateIssueAsync(summary, description, issueType: "Task", labels: new[] { "OT", "Production", "Order" });
                if (!string.IsNullOrWhiteSpace(issueKey))
                {
                    Console.WriteLine($"[OT] Jira issue created: {issueKey}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OT] Failed to create Jira issue: {ex.Message}");
            }

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
