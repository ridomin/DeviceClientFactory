using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rido
{
    static class Extensions
    {
        static int times = 1;
        static ProvisioningRegistrationAdditionalData GetProvisionPayload(string modelId)
        {
            return new ProvisioningRegistrationAdditionalData
            {
                JsonData = "{ modelId: '" + modelId + "'}"
            };
        }

        public static async Task<DeviceRegistrationResult> RegisterWithModelAsync(this ProvisioningDeviceClient dpsClient, string modelId, ILogger log)
        {
            var res = await dpsClient.RegisterAsync(GetProvisionPayload(modelId));
            log.LogInformation("First DPS call with Model ID, result: " + res.Status);
            while (res.Status != ProvisioningRegistrationStatusType.Assigned && times++ < 3)
            {
                res = await dpsClient.RegisterAsync(GetProvisionPayload(modelId));
                log.LogInformation($"Next DPS call: {times} with Model ID, result: " + res.Status);
                await Task.Delay((2 ^ times) * 1000);
            }
            return res;
        }
    }

    class DPS
    {
        public static async Task<DeviceClient> CreateDeviceWithMasterKey(string scopeId, string deviceId, string masterKey, string modelId, ILogger log)
        {
            byte[] key = Convert.FromBase64String(masterKey);
            using (var hmac = new HMACSHA256(key))
            {
                string deviceKey = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(deviceId)));
                return await ProvisionDeviceWithSasKeyAsync(scopeId, deviceId, deviceKey, modelId, log);
            }
        }

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
                        provResult = await provClient.RegisterWithModelAsync(modelId, log);
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
                        provResult = await provClient.RegisterWithModelAsync(modelId, log);
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
    }
}
