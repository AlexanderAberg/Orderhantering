using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OTSystem.Middleware;
using OTSystem.Models;
using OTSystem.Services;

namespace OTSystem.Configuration
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApiSettings>(Configuration.GetSection("ApiSettings"));
            services.AddHttpClient();

            services.AddScoped<ProductionLineService>();
            services.AddScoped<ITIntegrationService>();
            services.AddSingleton<IndustrialControlSystem>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            app.UseMiddleware<AuthMiddleware>();
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<ApiKeyMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapPost("/api/orders/start", async (OrderModel order, ProductionLineService productionService) =>
            {
                if (string.IsNullOrWhiteSpace(order.ProductName) || order.Quantity <= 0)
                {
                    return Results.BadRequest(new { message = "Ogiltigt orderdata." });
                }

                await productionService.StartProductionAsync(order);
                return Results.Ok(new { message = "Produktion startad", orderId = order.Id });
            });

            app.MapGet("/api/status", (ProductionLineService productionService) =>
            {
                var status = productionService.GetCurrentStatus();
                return Results.Ok(status);
            });
        }
    }
}
