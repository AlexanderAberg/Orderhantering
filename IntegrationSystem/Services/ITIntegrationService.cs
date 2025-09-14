using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IntegrationSystem.Models;
using Microsoft.Extensions.Logging;

namespace IntegrationSystem.Services
{
    public class ITIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ITIntegrationService> _logger;

        public ITIntegrationService(HttpClient httpClient, ILogger<ITIntegrationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendOrderAsync(OrderModel order)
        {
            _logger.LogInformation("Skickar order till IT-system...");
            var response = await _httpClient.PostAsJsonAsync("/orders", order);
            response.EnsureSuccessStatusCode();
        }

    }
}
