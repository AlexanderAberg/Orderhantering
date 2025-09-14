using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IntegrationSystem.Services
{
    public class OTIntegrationService
    {
        private readonly ILogger<OTIntegrationService> _logger;

        public OTIntegrationService(ILogger<OTIntegrationService> logger)
        {
            _logger = logger;
        }

        public Task WriteOrderToModbusAsync()
        {
            _logger.LogInformation("Skriver order till OT-system (Modbus)...");
            return Task.CompletedTask;
        }

    }
}
