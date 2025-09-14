using IntegrationSystem.Middleware;
using IntegrationSystem.Models;
using IntegrationSystem.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;

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
            services.AddSingleton<MetricsService>();

            services.AddControllers();

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

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapMetrics();
        }
    }
}
