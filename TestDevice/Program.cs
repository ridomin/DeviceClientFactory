using Microsoft.Azure.Devices.Client;
using Rido;
using System;
using System.Threading.Tasks;

namespace TestDevice
{
    class Program
    {
        static string CS = Environment.GetEnvironmentVariable("CS");
        static async Task Main(string[] args)
        {
            var dc = await DeviceClientFactory.CreateDeviceClientAsync(CS, "dtmi:com:example:Thermostat;1");
            await dc.OpenAsync();
            Console.WriteLine($"Device {DeviceClientFactory.Instance.DeviceId} Connected to {DeviceClientFactory.Instance.HostName}");
            await Task.Delay(500);
            await dc.CloseAsync();
        }
    }
}
