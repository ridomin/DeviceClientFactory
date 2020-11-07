using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Rido
{
    class HubConnection
    {
        public static async Task<DeviceClient> CreateClientFromConnectionString(string connectionString, ILogger logger, string modelId = "")
        {
            DeviceClient client;
            if (String.IsNullOrEmpty(modelId))
            {
                client = DeviceClient.CreateFromConnectionString(
                  connectionString,
                  TransportType.Mqtt);
                DeviceClientFactory.Instance.ConnectionString = connectionString;
            }
            else
            {
                client = DeviceClient.CreateFromConnectionString(
                  connectionString,
                  TransportType.Mqtt,
                  new ClientOptions { ModelId = modelId });
                DeviceClientFactory.Instance.ConnectionString = $"{connectionString};ModelId={modelId}";

            }
            logger.LogWarning($"Device connected: " + connectionString);
            return await Task.FromResult(client);
        }

        public static async Task<DeviceClient> CreateClientFromCert(string hostName, string deviceId, string x509, ILogger logger, string modelId = "")
        {
            DeviceClient client;
            if (String.IsNullOrWhiteSpace(modelId))
            {
                client = DeviceClient.Create(hostName,
                 new DeviceAuthenticationWithX509Certificate(
                   deviceId,
                   X509Loader.GetCertFromConnectionString(x509, logger)),
                 TransportType.Mqtt);
            }
            else
            {
                client = DeviceClient.Create(hostName,
                  new DeviceAuthenticationWithX509Certificate(
                    deviceId,
                    X509Loader.GetCertFromConnectionString(x509, logger)),
                  TransportType.Mqtt,
                  new ClientOptions { ModelId = modelId });
            }
            logger.LogWarning($"Device {deviceId} connected to {hostName}");
            return await Task.FromResult(client);
        }

    }
}
