using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IntegrationSystem.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "X-API-KEY";
        private readonly string _apiKey;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _apiKey = configuration.GetValue<string>("ApiSettings:ApiKey") ?? "";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation($"Inkommande request path: {context.Request.Path}");

            var path = context.Request.Path;

            if (path.StartsWithSegments("/swagger") || path.StartsWithSegments("/metrics"))
            {
                _logger.LogInformation($"Tillåter åtkomst utan API-nyckel till: {path}");
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                _logger.LogWarning("API-nyckel saknas.");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API-nyckel saknas.");
                return;
            }

            var received = extractedApiKey.ToString().Trim();
            var expected = (_apiKey ?? string.Empty).Trim();

            if (!string.Equals(expected, received, StringComparison.Ordinal))
            {
                _logger.LogWarning("Ogiltig API-nyckel. Expected len={ExpectedLen} last4={ExpectedLast4}, got len={GotLen} last4={GotLast4}",
                    expected.Length, expected[^Math.Min(4, expected.Length)..],
                    received.Length, received[^Math.Min(4, received.Length)..]);

                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Ogiltig API-nyckel.");
                return;
            }

            await _next(context);
        }
 
    }
}
