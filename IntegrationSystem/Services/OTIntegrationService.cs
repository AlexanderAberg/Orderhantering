using EasyModbus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IntegrationSystem.Services
{
    public class OTIntegrationService
    {
        private readonly ILogger<OTIntegrationService> _logger;
        private readonly string _modbusIp;
        private readonly int _modbusPort;

        public OTIntegrationService(ILogger<OTIntegrationService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _modbusIp = configuration.GetValue<string>("ApiSettings:OTConnectionString") ?? "127.0.0.1";
            _modbusPort = 502;
        }

        public async Task WriteOrderToModbusAsync(int orderId, int productId, int quantity)
        {
            ModbusClient? client = null;
            try
            {
                client = new ModbusClient(_modbusIp, _modbusPort);
                client.Connect();

                _logger.LogInformation("Ansluten till Modbus @ {ip}:{port}", _modbusIp, _modbusPort);

                client.WriteSingleRegister(1, orderId);
                client.WriteSingleRegister(2, productId);
                client.WriteSingleRegister(3, quantity);

                client.WriteSingleCoil(1, true);

                _logger.LogInformation("Order skickad till Modbus: OrderID={orderId}, ProduktID={productId}, Antal={quantity}", orderId, productId, quantity);
                client.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid kommunikation med Modbus-klient");
            }
            finally
            {
                client = null;
            }

            await Task.CompletedTask;
        }
    }
}
