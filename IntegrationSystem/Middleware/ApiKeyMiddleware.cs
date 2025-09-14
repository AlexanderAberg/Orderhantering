using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IntegrationSystem.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        private const string APIKEYNAME = "X-API-KEY";

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _apiKey = configuration.GetValue<string>("ApiSettings:ApiKey") ?? "";
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Request path: " + context.Request.Path);

            if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Swagger accessed. Skipping API key check.");
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API-nyckel saknas.");
                return;
            }

            if (!_apiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Ogiltig API-nyckel.");
                return;
            }

            await _next(context);
        }
    }
}
