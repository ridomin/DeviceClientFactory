using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace dcf_client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            ILogger logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();

            await Rido.DeviceClientFactory.CreateDeviceClientAsync(config.GetValue<string>("DeviceConnectionString"), logger);
            
        }
    }
}
