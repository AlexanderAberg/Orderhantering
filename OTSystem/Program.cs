using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OTSystem.Configuration;
using OTSystem.Services;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

startup.Configure(app, app.Environment);

var controlSystem = app.Services.GetRequiredService<IndustrialControlSystem>();
controlSystem.Run();

var lifetime = app.Lifetime;
lifetime.ApplicationStopping.Register(() =>
{
    controlSystem.StopAsync().GetAwaiter().GetResult();
});

await app.RunAsync();
