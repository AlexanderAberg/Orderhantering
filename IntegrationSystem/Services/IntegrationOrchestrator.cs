using ITSystem.Data.Entities;
using IntegrationSystem.Models;
using Microsoft.Extensions.Logging;

namespace IntegrationSystem.Services
{
    public class IntegrationOrchestrator
    {
        private readonly ITIntegrationService _itService;
        private readonly OTIntegrationService _otService;
        private readonly MetricsService _metrics;
        private readonly ILogger<IntegrationOrchestrator> _logger;

        public IntegrationOrchestrator(
            ITIntegrationService itService,
            OTIntegrationService otService,
            MetricsService metrics,
            ILogger<IntegrationOrchestrator> logger)
        {
            _itService = itService;
            _otService = otService;
            _metrics = metrics;
            _logger = logger;
        }

        public async Task ProcessOrderAsync(Order itOrder)
        {
            var integrationOrder = MapFromItOrder(itOrder);
            await ProcessOrderAsync(integrationOrder);
        }

        public async Task ProcessOrderAsync(OrderModel order)
        {
            _logger.LogInformation("Startar integration för order {OrderId}", order.Id);
            _metrics.SetCurrentActiveOrders(1);

            try
            {
                await _metrics.TrackOrderSummaryAsync(async () =>
                {
                    await _otService.WriteOrderToModbusAsync(order.Id, order.ProductId, order.Quantity);

                    try { await _itService.SendOrderAsync(order); }
                    catch (Exception ex) { _logger.LogWarning(ex, "IT-notifiering misslyckades för order {OrderId} (ignoreras)", order.Id); }
                });

                _metrics.IncrementOrdersProcessed();
                _logger.LogInformation("Integration klar för order {OrderId}", order.Id);
            }
            finally
            {
                _metrics.SetCurrentActiveOrders(0);
            }
        }

        private OrderModel MapFromItOrder(Order order) => new()
        {
            Id = order.Id,
            ProductId = order.ProductId,
            ProductName = order.Product?.Name ?? "Unknown",
            Quantity = order.Quantity,
        };

        public object GetStatus() => new { Status = "OK" };
    }
}
