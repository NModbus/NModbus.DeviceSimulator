# NModbus.DeviceSimulator
Simple project that demonstrates a Modbus device.

# Limitations
Initially, the simulator only supports simulating a TCP device.

# Docker

To run in docker:

`docker run -it --rm -p 502:502 --name devicesim nmodbus/devicesim`

To specify the port:

`docker run -it --rm -p 1000:1000 -e "DEVICE__PORT=1000" --name devicesim nmodbus/devicesim`

# Environment Variables

|Option|Default Value|
|---|---|
|DEVICE__ADDRESS | |
|DEVICE__PORT | 502 |
|DEVICE__UNITID| 1 |
|DEVICE__VERBOSE| false |

