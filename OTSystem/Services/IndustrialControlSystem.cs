using EasyModbus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OTSystem.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OTSystem.Services
{
    public class IndustrialControlSystem
    {
        private readonly ILogger<IndustrialControlSystem> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ModbusServer _modbusServer;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _modbusTask;
        private int _processing = 0;

        public IndustrialControlSystem(ILogger<IndustrialControlSystem> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;

            _modbusServer = new ModbusServer { Port = 502 };

            _modbusServer.CoilsChanged += (startAddress, numberOfCoils) =>
            {
                var commit = _modbusServer.coils.localArray[2];
                _logger.LogInformation("🌀 CoilsChanged at {Time}. Start={Start} Count={Count}, commit@2={Commit}",
                    DateTime.Now, startAddress, numberOfCoils, commit);

                if (!commit) return;

                if (Interlocked.Exchange(ref _processing, 1) == 1)
                {
                    _logger.LogInformation("Commit received but processing already in progress; ignoring.");
                    return;
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(100);

                        int orderId   = _modbusServer.holdingRegisters.localArray[2];
                        int productId = _modbusServer.holdingRegisters.localArray[3];
                        int quantity  = _modbusServer.holdingRegisters.localArray[4];

                        _logger.LogInformation("🔎 Snapshot read: orderId={OrderId}, productId={ProductId}, quantity={Quantity}", orderId, productId, quantity);

                        _modbusServer.coils.localArray[2] = false;
                        _logger.LogInformation("🔁 Commit coil[2] reset to false by OT.");

                        using var scope = _scopeFactory.CreateScope();
                        var production = scope.ServiceProvider.GetRequiredService<ProductionLineService>();

                        var order = new OrderModel
                        {
                            Id = orderId,
                            ProductName = $"Produkt {productId}",
                            Quantity = quantity
                        };

                        _logger.LogInformation("▶️ Starting production: orderId={OrderId}, productId={ProductId}, qty={Quantity}", orderId, productId, quantity);
                        await production.StartProductionAsync(order);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to trigger production from commit coil");
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _processing, 0);
                    }
                });
            };

            _modbusServer.HoldingRegistersChanged += (startAddress, numberOfRegisters) =>
            {
                _logger.LogInformation("📦 HoldingRegistersChanged at {Time}. Start={Start} Count={Count}", DateTime.Now, startAddress, numberOfRegisters);
                for (int i = 0; i < numberOfRegisters; i++)
                {
                    int address = startAddress + i;
                    if (address >= 0 && address < _modbusServer.holdingRegisters.localArray.Length)
                        _logger.LogInformation("   HoldingRegister[{Address}] = {Value}", address, _modbusServer.holdingRegisters.localArray[address]);
                }
            };
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
                        Thread.Sleep(1000);

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
            if (_cancellationTokenSource == null) return;

            _logger.LogInformation("🚦 Stopping EasyModbus TCP Slave...");
            try
            {
                _cancellationTokenSource.Cancel();
                _modbusServer.StopListening();
                if (_modbusTask != null) await _modbusTask;
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
