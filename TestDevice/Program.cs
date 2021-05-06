using Microsoft.Azure.Devices.Client;
using Rido;
using System;
using System.Threading.Tasks;

namespace TestDevice
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = Environment.GetEnvironmentVariable("CS");
            string modelId = "dtmi:com:example:TemperatureController;1";
            DeviceClient dc = await DeviceClientFactory.CreateDeviceClientAsync(connectionString, modelId);
            await dc.OpenAsync();
            Console.WriteLine("Connected");
            await Task.Delay(500);
            await dc.CloseAsync();
        }
    }
}
