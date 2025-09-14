using System.Threading.Tasks;
using IntegrationSystem.Models;
using Microsoft.Extensions.Logging;

namespace IntegrationSystem.Services
{
    public class IntegrationOrchestrator
    {
        private readonly ITIntegrationService _itService;
        private readonly OTIntegrationService _otService;
        private readonly ILogger<IntegrationOrchestrator> _logger;

        public IntegrationOrchestrator(ITIntegrationService itService, OTIntegrationService otService, ILogger<IntegrationOrchestrator> logger)
        {
            _itService = itService;
            _otService = otService;
            _logger = logger;
        }

        public async Task ProcessOrderAsync(OrderModel order)
        {
            _logger.LogInformation("Startar integration för order...");

            await _itService.SendOrderAsync(order);

            await _otService.WriteOrderToModbusAsync();

            _logger.LogInformation("Order integrerad mellan IT och OT.");
        }

        public object GetStatus()
        {
            return new { Status = "Allt OK" };
        }
    }
}
