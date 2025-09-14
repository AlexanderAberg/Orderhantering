using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OTSystem.Middleware
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
            _apiKey = configuration.GetValue<string>("ApiSettings:ApiKey") ?? "";
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Request path: " + context.Request.Path);

            if (context.Request.Path.StartsWithSegments("/swagger", System.StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/metrics", System.StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Swagger or metrics endpoint accessed, skipping API key check.");
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                _logger.LogWarning("API key missing.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API-nyckel saknas.");
                return;
            }

            if (!_apiKey.Equals(extractedApiKey))
            {
                _logger.LogWarning("Invalid API key.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Ogiltig API-nyckel.");
                return;
            }

            await _next(context);
        }
    }
}
