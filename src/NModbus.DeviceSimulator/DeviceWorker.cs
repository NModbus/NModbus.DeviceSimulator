using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NModbus.DeviceSimulator.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NModbus.DeviceSimulator
{
    public class DeviceWorker : IHostedService
    {
        private readonly ILogger _logger;
        private readonly DeviceConfiguration _configuration;
        private readonly DeviceService[] _deviceServices;

        private Task _runTask;
        private CancellationTokenSource _runCts;
        private DeviceService _deviceService;

        public DeviceWorker(
            ILogger<DeviceWorker> logger,
            DeviceConfiguration configuration,
            IEnumerable<DeviceService> deviceServices)
        {
            _logger = logger;
            _configuration = configuration;
            _deviceServices = deviceServices.ToArray();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Device type is {DeviceType}.", _configuration.DeviceType);

            _deviceService = _deviceServices
                .FirstOrDefault(s => s.DeviceType == _configuration.DeviceType);

            if (_deviceService == null)
                throw new Exception($"Unable to find a service for device type '{_configuration.DeviceType}'.");
          
            _runCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _runTask = _deviceService.RunAsync(_runCts.Token);

            _logger.LogInformation("Startup complete.");

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping listener...");

            _runCts?.Cancel();

            await _deviceService.StopAsync();

            try
            {
                await Task.WhenAll(_runTask);
            }
            catch(Exception ex)
            {
                _logger.LogTrace(ex, "Error stopping device.");
            }
        
            _logger.LogInformation("Device stopped.");
        }
    }
}
