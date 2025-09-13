using EasyModbus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OTSystem.Services
{
    public class IndustrialControlSystem
    {
        private readonly ILogger<IndustrialControlSystem> _logger;
        private readonly ModbusServer _modbusServer;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _modbusTask;

        private static double currentTemperature = 20.0;
        private const double TargetTemperature = 25.0;
        private static bool heaterOn = false;
        private static bool messageReceived = false;

        public IndustrialControlSystem(ILogger<IndustrialControlSystem> logger)
        {
            _logger = logger;
            _modbusServer = new ModbusServer
            {
                Port = 502
            };

            _modbusServer.CoilsChanged += (startAddress, numberOfCoils) =>
            {
                _logger.LogInformation("🌀 CoilsChanged event fired at {Time}", DateTime.Now);
                _logger.LogInformation("   Start Address: {Start}, Count: {Count}", startAddress, numberOfCoils);

                for (int i = 0; i < numberOfCoils; i++)
                {
                    int address = startAddress + i;
                    if (address >= 0 && address < _modbusServer.coils.localArray.Length)
                    {
                        _logger.LogInformation("   Coil[{Address}] = {Value}", address, _modbusServer.coils.localArray[address]);
                    }
                }

                messageReceived = true;
            };

            _modbusServer.HoldingRegistersChanged += (startAddress, numberOfRegisters) =>
            {
                _logger.LogInformation("📦 HoldingRegistersChanged event fired at {Time}", DateTime.Now);
                _logger.LogInformation("   Start Address: {Start}, Count: {Count}", startAddress, numberOfRegisters);

                for (int i = 0; i < numberOfRegisters; i++)
                {
                    int address = startAddress + i;
                    if (address >= 0 && address < _modbusServer.holdingRegisters.localArray.Length)
                    {
                        _logger.LogInformation("   HoldingRegister[{Address}] = {Value}", address, _modbusServer.holdingRegisters.localArray[address]);
                    }
                }
            };

            _modbusServer.holdingRegisters.localArray[0] = 123;
            _modbusServer.holdingRegisters.localArray[1] = 456;
            _modbusServer.coils.localArray[0] = true;
            _modbusServer.holdingRegisters.localArray[10] = 789;
            _modbusServer.inputRegisters[0] = 999;
            _modbusServer.discreteInputs[0] = true;
        }

        public void Run()
        {
            if (_modbusTask != null && !_modbusTask.IsCompleted)
            {
                _logger.LogWarning("Modbus server is already running.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _modbusTask = Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("🔌 Starting EasyModbus TCP Slave...");
                    _modbusServer.Listen();
                    _logger.LogInformation("✅ EasyModbus TCP Slave started.");

                    while (!token.IsCancellationRequested)
                    {
                        if (messageReceived)
                        {
                            _logger.LogInformation("📩 Order received via Modbus!");
                            messageReceived = false;
                        }

                        Thread.Sleep(1000);
                    }

                    _logger.LogInformation("🛑 Modbus server cancellation requested.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ An error occurred in Modbus server.");
                }
            }, token);
        }

        public async Task StopAsync()
        {
            if (_cancellationTokenSource == null)
                return;

            _logger.LogInformation("🚦 Stopping EasyModbus TCP Slave...");

            try
            {
                _cancellationTokenSource.Cancel();
                _modbusServer.StopListening();

                if (_modbusTask != null)
                {
                    await _modbusTask;
                }

                _logger.LogInformation("🟢 EasyModbus TCP Slave stopped cleanly.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while stopping Modbus server.");
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                _modbusTask = null;
            }
        }
    }
}
