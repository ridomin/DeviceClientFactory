using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rido
{
  class HubConnection
  {
    public static async Task<DeviceClient> CreateClientFromConnectionString(DeviceClientFactory dcf, ILogger logger)
    {
      var client = DeviceClient.CreateFromConnectionString(
        dcf.ConnectionString,
        TransportType.Mqtt,
        new ClientOptions { ModelId = dcf.ModelId });

      logger.LogWarning($"Device {dcf.DeviceId} connected to {dcf.HostName} via {dcf.connectionStringType}");
      return await Task.FromResult(client);
    }

    public static async Task<DeviceClient> CreateClientFromCert(DeviceClientFactory dcf, ILogger logger)
    {
      var client = DeviceClient.Create(dcf.HostName,
          new DeviceAuthenticationWithX509Certificate(
            dcf.DeviceId,
            X509Loader.GetCertFromConnectionString(dcf.X509, logger)),
          TransportType.Mqtt,
          new ClientOptions { ModelId = dcf.ModelId });
      logger.LogWarning($"Device {dcf.DeviceId} connected to {dcf.HostName} via {dcf.connectionStringType}");
      return await Task.FromResult(client);
    }

  }
}
