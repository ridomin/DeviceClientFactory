using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Rido
{
  class DPS
  {
    internal static async Task<DeviceClient> ProvisionDeviceWithSasKeyAsync(string scopeId, string deviceId, string deviceKey, string modelId, ILogger log)
    {
      using (var transport = new ProvisioningTransportHandlerMqtt())
      {
        using (var security = new SecurityProviderSymmetricKey(deviceId, deviceKey, null))
        {
          DeviceRegistrationResult provResult;
          var provClient = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net", scopeId, security, transport);

          if (!string.IsNullOrEmpty(modelId))
          {
            provResult = await provClient.RegisterAsync(GetProvisionPayload(modelId)).ConfigureAwait(false);
          }
          else
          {
            provResult = await provClient.RegisterAsync().ConfigureAwait(false);
          }

          log.LogInformation($"Provioning Result. Status [{provResult.Status}] SubStatus [{provResult.Substatus}]");

          if (provResult.Status == ProvisioningRegistrationStatusType.Assigned)
          {
            log.LogWarning($"Device {provResult.DeviceId} in Hub {provResult.AssignedHub}");
            log.LogInformation($"LastRefresh {provResult.LastUpdatedDateTimeUtc} RegistrationId {provResult.RegistrationId}");
            var csBuilder = IotHubConnectionStringBuilder.Create(provResult.AssignedHub, new DeviceAuthenticationWithRegistrySymmetricKey(provResult.DeviceId, security.GetPrimaryKey()));
            string connectionString = csBuilder.ToString();
            return await HubConnection.CreateClientFromConnectionString(connectionString, log, modelId);
          }
          else
          {
            string errorMessage = $"Device not provisioned. Message: {provResult.ErrorMessage}";
            log.LogError(errorMessage);
            throw new IotHubException(errorMessage);
          }
        }
      }
    }

    internal static async Task<DeviceClient> ProvisionDeviceWithCertAsync(string scopeId, string X509LocatorString, string modelId, ILogger log)
    {
      using (var transport = new ProvisioningTransportHandlerMqtt())
      {
        var cert = X509Loader.GetCertFromConnectionString(X509LocatorString, log);
        using (var security = new SecurityProviderX509Certificate(cert))
        {
          DeviceRegistrationResult provResult;
          var provClient = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net", scopeId, security, transport);
          if (!String.IsNullOrEmpty(modelId))
          {
            provResult = await provClient.RegisterAsync(GetProvisionPayload(modelId)).ConfigureAwait(false);
          }
          else
          {
            provResult = await provClient.RegisterAsync().ConfigureAwait(false);
          }

          log.LogInformation($"Provioning Result. Status [{provResult.Status}] SubStatus [{provResult.Substatus}]");

          if (provResult.Status == ProvisioningRegistrationStatusType.Assigned)
          {
            log.LogWarning($"Device {provResult.DeviceId} in Hub {provResult.AssignedHub}");
            log.LogInformation($"LastRefresh {provResult.LastUpdatedDateTimeUtc} RegistrationId {provResult.RegistrationId}");

            var csBuilder = IotHubConnectionStringBuilder.Create(provResult.AssignedHub, new DeviceAuthenticationWithX509Certificate(provResult.DeviceId, security.GetAuthenticationCertificate()));
            string connectionString = csBuilder.ToString();

            DeviceClient client;
            if (string.IsNullOrEmpty(modelId))
            {
              client = DeviceClient.Create(provResult.AssignedHub, new DeviceAuthenticationWithX509Certificate(provResult.DeviceId, security.GetAuthenticationCertificate()), TransportType.Mqtt);
            }
            else
            {
              client = DeviceClient.Create(provResult.AssignedHub, new DeviceAuthenticationWithX509Certificate(provResult.DeviceId, security.GetAuthenticationCertificate()), TransportType.Mqtt, new ClientOptions { ModelId = modelId });
            }
            return client;
          }
          else
          {
            string errorMessage = $"Device not provisioned. Message: {provResult.ErrorMessage}";
            log.LogError(errorMessage);
            throw new IotHubException(errorMessage);
          }
        }
      }
    }

    static ProvisioningRegistrationAdditionalData GetProvisionPayload(string modelId)
    {
      return new ProvisioningRegistrationAdditionalData
      {
        JsonData = "{ modelId: '" + modelId + "'}"
      };
    }
  }
}
