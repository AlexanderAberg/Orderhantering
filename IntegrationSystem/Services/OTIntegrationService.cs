using EasyModbus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
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

        public async Task WriteOrderToModbusAsync(int productId, int quantity)
        {
            ModbusClient client = null;
            try
            {
                client = new ModbusClient(_modbusIp, _modbusPort);
                client.Connect();

                _logger.LogInformation("Ansluten till Modbus @ {ip}:{port}", _modbusIp, _modbusPort);

                client.WriteSingleRegister(0, productId);
                client.WriteSingleRegister(1, quantity);

                _logger.LogInformation("Order skickad till Modbus: ProduktID={productId}, Antal={quantity}", productId, quantity);

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

        public async Task<(int productId, int quantity)> ReadLastOrderAsync()
        {
            ModbusClient client = null;
            try
            {
                client = new ModbusClient(_modbusIp, _modbusPort);
                client.Connect();

                var values = client.ReadHoldingRegisters(0, 2);
                int productId = values[0];
                int quantity = values[1];

                client.Disconnect();

                return (productId, quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kunde inte läsa order från Modbus");
                return (0, 0);
            }
            finally
            {
                client = null;
            }
        }
    }
}
