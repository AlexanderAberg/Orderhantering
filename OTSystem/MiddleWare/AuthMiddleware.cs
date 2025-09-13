using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using OTSystem.Models;

namespace OTSystem.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _expectedApiKey;

        public AuthMiddleware(RequestDelegate next, IOptions<ApiSettings> options)
        {
            _next = next;
            _expectedApiKey = options.Value.ApiKey;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var providedKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API-nyckel saknas.");
                return;
            }

            if (providedKey != _expectedApiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Felaktig API-nyckel.");
                return;
            }

            await _next(context);
        }
    }
}
