using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OTSystem.Configuration;
using System.Threading.Tasks;

namespace OTSystem.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;
        private readonly ILogger<ApiKeyMiddleware> _logger;
        private const string APIKEYNAME = "X-API-KEY";

        public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiSettings> options, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _apiKey = options.Value.ApiKey;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                _logger.LogWarning("API Key saknas från IP: {IP}", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key saknas.");
                return;
            }

            if (!_apiKey.Equals(extractedApiKey))
            {
                _logger.LogWarning("Ogiltig API Key från IP: {IP}", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Ogiltig API Key.");
                return;
            }

            await _next(context);
        }
    }
}
