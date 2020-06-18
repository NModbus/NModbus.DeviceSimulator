namespace NModbus.DeviceSimulator
{
    public class DeviceConfiguration
    {
        public int Port { get; set; } = 502;

        public byte UnitId { get; set; } = 1;

        public string IpAddress { get; set; }

        public bool Verbose { get; set; } = false;
    }
}
