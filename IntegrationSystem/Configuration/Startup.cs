using IntegrationSystem.Middleware;
using IntegrationSystem.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntegrationSystem.Configuration
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
            services.AddSingleton<OTIntegrationService>();  
            services.AddScoped<ITIntegrationService>();     
            services.AddScoped<IntegrationOrchestrator>();  

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

            app.UseMiddleware<ApiKeyMiddleware>();

            app.UseHttpsRedirection();

            app.MapPost("/api/integrate/order", async (OrderModel order, IntegrationOrchestrator orchestrator) =>
            {
                if (string.IsNullOrWhiteSpace(order.ProductName) || order.Quantity <= 0)
                    return Results.BadRequest(new { message = "Ogiltigt orderdata." });

                await orchestrator.ProcessOrderAsync(order);
                return Results.Ok(new { message = "Order integrerad och processad." });
            });

            app.MapGet("/api/integrate/status", (IntegrationOrchestrator orchestrator) =>
            {
                var status = orchestrator.GetStatus();
                return Results.Ok(status);
            });
        }
    }
}
