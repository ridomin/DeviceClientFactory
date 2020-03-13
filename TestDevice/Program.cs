using Microsoft.Extensions.Logging;
using Rido;
using System;
using System.Threading.Tasks;

namespace TestDevice
{
    class Program
    {
        const string directSas = "HostName=e2e-test-hub.azure-devices.net;DeviceId=test-sas-01;SharedAccessKey=pRNwmc8UU0fH6vnTZ50PmfqGffii5fWWLNfdQOaBsu8=";
        const string directSSCert = "HostName=e2e-test-hub.azure-devices.net;DeviceId=test-ss-cert-01;X509=mycert.pfx|1234";
        const string directCACert = "HostName=e2e-test-hub.azure-devices.net;DeviceId=test-cert-ca-01;X509=01632B87BED25142DD2F1337AA50CCF1B0F79831";
        //const string directInvalidCACert = "HostName=e2e-test-hub.azure-devices.net;DeviceId=test-cert-ca-01;X509=INVALID01632B87BED25142DD2F1337AA50CCF1B0F79831";
        const string unauthorizedCert = "HostName=e2e-test-hub.azure-devices.net;DeviceId=test-cert-ca-01;X509=2afebca112d69de4b22f349a46f70991555ed31e";

        static ILogger logger = LoggerFactory.Create(builder => { builder.AddConsole(); }).CreateLogger("Cat1");
        static async Task Main(string[] args)
        {
            //await ConnectDevice(unauthorizedCert, nameof(unauthorizedCert)).ConfigureAwait(false);
            //await ConnectDevice(directCACert, nameof(directCACert)).ConfigureAwait(false);
            await ConnectDevice(directSSCert, nameof(directSSCert)).ConfigureAwait(false);
            await ConnectDevice(directSas, nameof(directSas)).ConfigureAwait(false);
        }

        private static async Task ConnectDevice(string cs, string name)
        {
            Console.WriteLine("Connecting: " + name);
            var dcf = new DeviceClientFactory(cs, logger);
            var dc = await dcf.CreateDeviceClientAsync().ConfigureAwait(false);

            dc.SetConnectionStatusChangesHandler(
                (Microsoft.Azure.Devices.Client.ConnectionStatus status,
                 Microsoft.Azure.Devices.Client.ConnectionStatusChangeReason reason) => Console.WriteLine(status));

            await dc.OpenAsync();
            await dc.CloseAsync();
            Console.WriteLine("Finished: " + name);
        }
    }
}
