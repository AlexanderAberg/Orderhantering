using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITSystem.Integration
{
    internal class IntegrationClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<IntegrationClient> _logger;

        public IntegrationClient(HttpClient http, IConfiguration config, ILogger<IntegrationClient> logger)
        {
            _http = http;
            _logger = logger;

            if (_http.BaseAddress is null)
            {
                var baseUrl = (config["Integration:BaseUrl"] ?? "http://localhost:5000").TrimEnd('/');
                _http.BaseAddress = new Uri(baseUrl);
            }

            var apiKey = config["Integration:ApiKey"]?.Trim();
            if (!string.IsNullOrWhiteSpace(apiKey) && !_http.DefaultRequestHeaders.Contains("X-API-KEY"))
                _http.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            _logger.LogInformation("Integration HttpClient BaseAddress: {Base}. API key present: {HasKey}",
                _http.BaseAddress, !string.IsNullOrWhiteSpace(apiKey));
        }

        public async Task SendOrderAsync(int orderId, int productId, string productName, int quantity, CancellationToken cancellationToken = default)
        {
            var relativePath = "api/integration/orders";
            var url = new Uri(_http.BaseAddress!, relativePath).ToString();
            var payload = new { Id = orderId, ProductId = productId, ProductName = productName, Quantity = quantity };

            _logger.LogInformation("Sänder order till Integration på {Url}: OrderId={OrderId} ProductId={ProductId} {Product} x{Qty}",
                url, orderId, productId, productName, quantity);

            var resp = await _http.PostAsJsonAsync(relativePath, payload, cancellationToken);
            _logger.LogInformation("Integrationssvar: {Status}", resp.StatusCode);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Integration call failed: {StatusCode} {ReasonPhrase}. Response body: {Body}",
                    (int)resp.StatusCode, resp.ReasonPhrase, body);
                resp.EnsureSuccessStatusCode();
            }
        }
    }
}