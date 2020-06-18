using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NModbus.Data;
using NModbus.DeviceSimulator;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NModbus.DeviceSimulator
{
    public class DeviceWorker : IHostedService
    {
        private readonly ILogger _logger;
        private readonly DeviceConfiguration _configuration;
        private readonly IModbusLogger _modbusLogger;

        private Task _runTask;
        private CancellationTokenSource _runCts;
        private TcpListener _tcpListener;

        public DeviceWorker(
            ILogger<DeviceWorker> logger,
            DeviceConfiguration configuration,
            IModbusLogger modbusLogger)
        {
            _logger = logger;
            _configuration = configuration;
            _modbusLogger = modbusLogger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            IPAddress address;

            if (string.IsNullOrWhiteSpace(_configuration.IpAddress))
            {
                address = IPAddress.Any;
            }
            else
            {
                address = IPAddress.Parse(_configuration.IpAddress);
            }

            _logger.LogInformation("Starting Modbus TCP Device on {IPAddress}:{Port} with unit id [{UnitId}]", 
                address, 
                _configuration.Port, 
                _configuration.UnitId);

            IModbusLogger modbusLogger = null;

            if (_configuration.Verbose)
            {
                _logger.LogInformation("Enabling NModbus logging...");
                modbusLogger = _modbusLogger;
            }

            IModbusFactory factory = new ModbusFactory(logger: modbusLogger);

            _logger.LogInformation("Starting TCP Listener...");

            //Start up a listener
            _tcpListener = new TcpListener(address, _configuration.Port);
            _tcpListener.Start();

            var network = factory.CreateSlaveNetwork(_tcpListener);

            //Create a datastore
            var dataStore = new SlaveDataStore();

            //Log the operations for debugging purposes.
            dataStore.HoldingRegisters.BeforeRead += (sender, args) =>  LogReadAction("HoldingRegister(s)", args);
            dataStore.InputRegisters.BeforeRead += (sender, args) => LogReadAction("InputRegister(s)", args);
            dataStore.CoilDiscretes.BeforeRead += (sender, args) => LogReadAction("CoilDiscrete(s)", args);
            dataStore.CoilInputs.BeforeRead += (sender, args) => LogReadAction("CoilInput(s)", args);

            dataStore.HoldingRegisters.BeforeWrite += (sender, args) => LogWriteAction("HoldingRegister(s)", args);
            dataStore.CoilDiscretes.BeforeWrite += (sender, args) => LogWriteAction("CoilInput(s)", args);

            var device = factory.CreateSlave(_configuration.UnitId, dataStore);

            //Add the device to the network
            network.AddSlave(device);

            _runCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _runTask = network.ListenAsync(_runCts.Token);

            _logger.LogInformation("Startup complete.");

            return Task.CompletedTask;
        }

        private void LogReadAction(string type, Device.PointEventArgs args)
        {
            _logger.LogInformation("{Count} {Type} read starting at {Address}.", 
                args.NumberOfPoints, 
                type, 
                args.StartAddress);
        }

        private void LogWriteAction(string type, Device.PointEventArgs<ushort> args) 
        {
            var data = string.Join(", ", args.Points.Select(p => p.ToString()));

            _logger.LogInformation("{Count} {Type} written starting at {Address}: [{Data}]",
                args.NumberOfPoints,
                type,
                args.StartAddress,
                data);
        }

        private void LogWriteAction(string type, Device.PointEventArgs<bool> args)
        {
            var data = string.Join(", ", args.Points.Select(p => p ? "1" : "0"));

            _logger.LogInformation("{Count} {Type} written starting at {Address}: [{Data}]",
                args.NumberOfPoints,
                type,
                args.StartAddress,
                data);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping listener...");

            _runCts?.Cancel();

            _tcpListener.Stop();

            try
            {
                await Task.WhenAll(_runTask);
            }
            catch(Exception ex)
            {
                _logger.LogTrace(ex, "Error stopping device.");
            }
        
            _logger.LogInformation("Listener stopped.");
        }
    }
}
