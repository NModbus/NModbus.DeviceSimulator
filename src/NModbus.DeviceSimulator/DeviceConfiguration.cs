namespace NModbus.DeviceSimulator
{
    public class DeviceConfiguration
    {
        /// <summary>
        /// The type of device to use.
        /// </summary>
        public DeviceType DeviceType { get; set; } = DeviceType.Tcp;

        /// <summary>
        /// The port on which to listen for incoming connections.
        /// </summary>
        public int Port { get; set; } = 502;

        /// <summary>
        /// The unit id (address) of the device.
        /// </summary>
        public string UnitIds { get; set; } = "1";

        /// <summary>
        /// The local IP Address to use. This normally doesn't need to be specified.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Set to true to enable additional logging.
        /// </summary>
        public bool Verbose { get; set; } = false;
    }
}
