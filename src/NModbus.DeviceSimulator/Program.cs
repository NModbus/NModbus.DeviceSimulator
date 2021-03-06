﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NModbus.DeviceSimulator.Devices;
using System.Threading.Tasks;

namespace NModbus.DeviceSimulator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<DeviceWorker>();
                    services.AddSingleton<IModbusLogger, ModbusLogger>();
                    services.AddSingleton<DeviceConfiguration>(serviceProvider =>
                    {
                        var config = serviceProvider.GetRequiredService<IConfiguration>();

                        var deviceConfiguration = new DeviceConfiguration();

                        config.GetSection("Device").Bind(deviceConfiguration);

                        return deviceConfiguration;
                    });

                    services.AddSingleton<IModbusFactory>(serviceProvider => {

                        var deviceConfiguration = serviceProvider.GetRequiredService<DeviceConfiguration>();

                        if (deviceConfiguration.Verbose)
                        {
                            var modbusLogger = serviceProvider.GetRequiredService<IModbusLogger>();

                            return new ModbusFactory(logger: modbusLogger);
                        }

                        return new ModbusFactory();
                    });

                    //Device Services
                    services.AddSingleton<DeviceService, TcpDeviceService>();
                    services.AddSingleton<DeviceService, UdpDeviceService>();
                });
        }
    }
}
