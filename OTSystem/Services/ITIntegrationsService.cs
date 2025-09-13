using Microsoft.Extensions.Options;
using OTSystem.Models;
using OTSystem.Services;

public class ITIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly ProductionLineService _productionLineService;
    private readonly string _apiKey;

    public ITIntegrationService(
        HttpClient httpClient,
        ProductionLineService productionLineService,
        IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _productionLineService = productionLineService;
        _apiKey = apiSettings.Value.ApiKey;
    }

    public async Task HandleOrderAsync(OrderModel order)
    {
        Console.WriteLine($"Använder API-nyckel: {_apiKey}");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);

        var status = _productionLineService.GetCurrentStatus();

        var response = await _httpClient.PostAsJsonAsync("http://localhost:5000/api/productionstatus", status);

        if (response.IsSuccessStatusCode)
            Console.WriteLine("Status skickad korrekt.");
        else
            Console.WriteLine($"Fel vid skickande av status: {response.StatusCode}");
    }
}
