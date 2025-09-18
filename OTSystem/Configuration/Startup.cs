using Prometheus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OTSystem.Middleware;
using OTSystem.Models;
using OTSystem.Services;
using OTSystem.Configuration;
using System.Net.Http.Headers;

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
            services.Configure<JiraSettings>(Configuration.GetSection("Jira"));

            services.AddHttpClient();
            services.AddHttpClient("Jira", (sp, c) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var baseUrl = cfg["Jira:CloudUrl"]?.TrimEnd('/');
                if (!string.IsNullOrWhiteSpace(baseUrl))
                    c.BaseAddress = new Uri($"{baseUrl}/");
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddScoped<ProductionLineService>();
            services.AddScoped<ITIntegrationService>();
            services.AddSingleton<IndustrialControlSystem>();
            services.AddSingleton<JiraService>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMetricServer();
            app.UseHttpMetrics();

            app.UseMiddleware<ApiKeyMiddleware>();
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseHttpsRedirection();

            var modbusServer = app.Services.GetRequiredService<IndustrialControlSystem>();
            modbusServer.Run();

            app.Lifetime.ApplicationStopping.Register(() =>
            {
                modbusServer.StopAsync().GetAwaiter().GetResult();
            });

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
