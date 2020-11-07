﻿using Microsoft.Extensions.Logging;
using Rido;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace dps_client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ILogger logger = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information)).CreateLogger<Program>();

            Stopwatch clock = Stopwatch.StartNew();
            Console.WriteLine($"Dps Client [{ThisAssemblyVersion}]. Connecting.");
            var dc = await DeviceClientFactory.CreateDeviceClientAsync(args[0], logger);
            Console.WriteLine($"Dps Client connected in {clock.Elapsed.TotalMilliseconds} ms.");

            dc.SetConnectionStatusChangesHandler(
                (Microsoft.Azure.Devices.Client.ConnectionStatus status,
                 Microsoft.Azure.Devices.Client.ConnectionStatusChangeReason reason) => Console.WriteLine(status));

            await dc.OpenAsync();
            var twin = await dc.GetTwinAsync();
            Console.WriteLine(twin.ToJson());
            await dc.CloseAsync();
            Console.WriteLine(DeviceClientFactory.Instance.ConnectionString);

        }
        static string ThisAssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}