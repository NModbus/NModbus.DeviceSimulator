# NModbus.DeviceSimulator
Simple project that demonstrates a Modbus device.

# Limitations
Initially, the simulator only supports simulating a TCP device.

# Docker

To run in docker:

`docker run -it --rm -p 502:502 --name devicesim nmodbus/devicesim`

To specify the port:

`docker run -it --rm -p 1000:1000 -e "DEVICE__PORT=1000" --name devicesim nmodbus/devicesim`

To run UDP:

`docker run -it --rm -p 502:502/udp -e "DEVICE__DEVICETYPE=UDP"  --name devicesim devicesim`

# Environment Variables

|Option|Default Value|Description|
|---|---|---|
|DEVICE__DEVICETYPE | Tcp | Tcp, Udp |
|DEVICE__ADDRESS | | The address to use for Tcp. Usually not needed. |
|DEVICE__PORT | 502 | The port to listen on (Applies to Tcp / Udp). |
|DEVICE__UNITIDS| 1 | Comma delimited list of unit ids. |
|DEVICE__VERBOSE| false | Additional logging. |

