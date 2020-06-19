using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NModbus.DeviceSimulator.Devices
{
    public class UdpDeviceService : DeviceService
    {
        private UdpClient _udpClient;

        public UdpDeviceService(ILogger<UdpDeviceService> logger, IModbusFactory factory, DeviceConfiguration configuration) 
            : base(logger, factory, configuration)
        {
        }

        public override DeviceType DeviceType => DeviceType.Udp;

        protected override Task<IModbusSlaveNetwork> CreateNetworkAsync()
        {
            Logger.LogInformation("Starting Modbus UDP network on port {Port}",
                Configuration.Port);

            _udpClient = new UdpClient(Configuration.Port);

            //_udpClient.Connect(address, Configuration.Port);

            return Task.FromResult(Factory.CreateSlaveNetwork(_udpClient));
        }

        public override Task StopAsync()
        {
            _udpClient.Dispose();

            return Task.CompletedTask;
        }
    }
}
