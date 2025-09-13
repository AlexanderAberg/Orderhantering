using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OTSystem.Middleware;
using OTSystem.Models;
using OTSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddHttpClient();

builder.Services.AddScoped<ProductionLineService>();
builder.Services.AddScoped<ITIntegrationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();


if (app.Environment.IsDevelopment())
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

app.Run();
