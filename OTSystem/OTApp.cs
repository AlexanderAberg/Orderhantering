using OTSystem.Models;
using OTSystem.Services;

namespace OTSystem
{
    public class OTApp
    {
        private readonly ITIntegrationService _integrationService;

        public OTApp(ITIntegrationService integrationService)
        {
            _integrationService = integrationService;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("== OTSystem är igång ==");
            Console.WriteLine("Väntar på order...");

            while (true)
            {
                Console.WriteLine("\nSkriv 'order' för att simulera ny order, eller 'exit' för att avsluta:");
                var input = Console.ReadLine();

                if (input?.ToLower() == "exit")
                    break;

                if (input?.ToLower() == "order")
                {
                    var dummyOrder = new OrderModel
                    {
                        Id = new Random().Next(1000, 9999),
                        ProductName = "Testprodukt",
                        Quantity = 5
                    };

                    await _integrationService.HandleOrderAsync(dummyOrder);
                }
            }
        }
    }
}
