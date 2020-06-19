using Microsoft.Extensions.Logging;
using NModbus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NModbus.DeviceSimulator.Devices
{
    public abstract class DeviceService
    {
        protected DeviceService(ILogger logger, IModbusFactory factory, DeviceConfiguration configuration)
        {
            Logger = logger;
            Factory = factory;
            Configuration = configuration;
        }

        public abstract DeviceType DeviceType { get; }

        protected ILogger Logger { get; }

        protected IModbusFactory Factory { get; }

        protected DeviceConfiguration Configuration { get; }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            //Create the network
            var network = await CreateNetworkAsync();

            //Create a datastore
            var dataStore = new SlaveDataStore();

            //Log the operations for debugging purposes.
            dataStore.HoldingRegisters.BeforeRead += (sender, args) => LogReadAction("HoldingRegister(s)", args);
            dataStore.InputRegisters.BeforeRead += (sender, args) => LogReadAction("InputRegister(s)", args);
            dataStore.CoilDiscretes.BeforeRead += (sender, args) => LogReadAction("CoilDiscrete(s)", args);
            dataStore.CoilInputs.BeforeRead += (sender, args) => LogReadAction("CoilInput(s)", args);

            dataStore.HoldingRegisters.BeforeWrite += (sender, args) => LogWriteAction("HoldingRegister(s)", args);
            dataStore.CoilDiscretes.BeforeWrite += (sender, args) => LogWriteAction("CoilInput(s)", args);

            var unitIds = GetUnitIds();

            foreach(var unitId in unitIds)
            {
                Logger.LogInformation("Adding device with unit id: {UnitId}", unitId);

                var device = Factory.CreateSlave(unitId, dataStore);

                //Add the device to the network
                network.AddSlave(device);
            }

            //Listen
            await network.ListenAsync(cancellationToken);
        }

        private byte[] GetUnitIds()
        {
            if (string.IsNullOrWhiteSpace(Configuration.UnitIds))
                throw new ApplicationException("No UnitIds were specified.");

            var split = Configuration.UnitIds.Split(',');

            var unitIds = new List<byte>(split.Length);

            foreach(var raw in split)
            {
                if (!byte.TryParse(raw, out var unitid))
                    throw new ApplicationException($"Unable to parse unit id '{raw}'.");

                unitIds.Add(unitid);
            }

            return unitIds.ToArray();
        }

        private void LogReadAction(string type, Device.PointEventArgs args)
        {
            Logger.LogInformation("{Count} {Type} read starting at {Address}.",
                args.NumberOfPoints,
                type,
                args.StartAddress);
        }

        private void LogWriteAction(string type, Device.PointEventArgs<ushort> args)
        {
            var data = string.Join(", ", args.Points.Select(p => p.ToString()));

            Logger.LogInformation("{Count} {Type} written starting at {Address}: [{Data}]",
                args.NumberOfPoints,
                type,
                args.StartAddress,
                data);
        }

        private void LogWriteAction(string type, Device.PointEventArgs<bool> args)
        {
            var data = string.Join(", ", args.Points.Select(p => p ? "1" : "0"));

            Logger.LogInformation("{Count} {Type} written starting at {Address}: [{Data}]",
                args.NumberOfPoints,
                type,
                args.StartAddress,
                data);
        }

        protected abstract Task<IModbusSlaveNetwork> CreateNetworkAsync();

        public abstract Task StopAsync();
    }

}
