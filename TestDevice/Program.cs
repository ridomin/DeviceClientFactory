using Microsoft.Extensions.Logging;
using Rido;
using System;
using System.Threading.Tasks;

namespace TestDevice
{
    class Program
    {
        const string directSas = "HostName=e2e-test-hub.azure-devices.net;DeviceId=test-01;SharedAccessKey=PfUZju2cm7huZKN1VtkIgL5pz2jMQzsPtGZqPG7Iv7s=";
        const string directCert = "HostName=e2e-test-hub.azure-devices.net;DeviceId=test-cert-ca-01;X509Thumbprint=261AA9AE4024EA9AD23297C7C4C3C5579692445E";

        static async Task Main(string[] args)
        {
            var logger = LoggerFactory.Create(builder => { builder.AddConsole(); }).CreateLogger("Cat1");

            var dcf = new DeviceClientFactory(directCert, logger);
            var dc = await dcf.CreateDeviceClientAsync().ConfigureAwait(false);

            dc.SetConnectionStatusChangesHandler(
                (Microsoft.Azure.Devices.Client.ConnectionStatus status, 
                 Microsoft.Azure.Devices.Client.ConnectionStatusChangeReason reason) => Console.WriteLine(status));

            await dc.OpenAsync();
            await dc.CloseAsync();
            Console.ReadLine();
        }
    }
}
