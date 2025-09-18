using System;
using ITSystem.Data.Contexts;
using ITSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ITSystem.Integration;

namespace ITSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<OrderApp>();
            app.Init();
            app.RunMenu();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

                    services.AddDbContext<OrderDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    services.AddHttpClient<IntegrationClient>(client =>
                    {
                        var baseUrl = context.Configuration["Integration:BaseUrl"] ?? "http://localhost:5000";
                        client.BaseAddress = new Uri(baseUrl);
                    });

                    services.AddScoped<IOrderService, OrderService>();
                    services.AddScoped<IUserService, UserService>();
                    services.AddScoped<IProductService, ProductService>();
                    services.AddScoped<OrderApp>();
                });
    }
}
