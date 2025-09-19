using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITSystem.Integration
{
    internal class IntegrationClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly ILogger<IntegrationClient> _logger;

        public IntegrationClient(HttpClient http, IConfiguration config, ILogger<IntegrationClient> logger)
        {
            _http = http;
            _logger = logger;
            _baseUrl = (config["Integration:BaseUrl"] ?? "http://localhost:5000").TrimEnd('/');

            var apiKey = config["Integration:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey) && !_http.DefaultRequestHeaders.Contains("X-API-KEY"))
                _http.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        }

        public async Task SendOrderAsync(int orderId, int productId, string productName, int quantity)
        {
            var url = $"{_baseUrl}/api/integration/orders";
            var payload = new { Id = orderId, ProductId = productId, ProductName = productName, Quantity = quantity };

            _logger.LogInformation("Sänder order till Integration på {Url}: OrderId={OrderId} ProductId={ProductId} {Product} x{Qty}", url, orderId, productId, productName, quantity);
            var resp = await _http.PostAsJsonAsync(url, payload);
            _logger.LogInformation("Integrationssvar: {Status}", resp.StatusCode);
            resp.EnsureSuccessStatusCode();
        }
    }
}