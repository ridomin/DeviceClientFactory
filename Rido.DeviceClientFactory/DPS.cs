﻿using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Threading.Tasks;

namespace Rido
{
    class DPS
    {
        internal static async Task<DeviceClient> ProvisionDeviceWithSasKeyAsync(string scopeId, string deviceId, string deviceKey, string dcmId)
        {
            using (var transport = new ProvisioningTransportHandlerMqtt())
            {
                using (var security = new SecurityProviderSymmetricKey(deviceId, deviceKey, null))
                {
                    DeviceRegistrationResult provResult;
                    var provClient = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net", scopeId, security, transport);

                    if (!string.IsNullOrEmpty(dcmId))
                    {
                        provResult = await provClient.RegisterAsync(GetProvisionPayload(dcmId)).ConfigureAwait(false);
                    }
                    else
                    {
                        provResult = await provClient.RegisterAsync().ConfigureAwait(false);
                    }

                    if (provResult.Status == ProvisioningRegistrationStatusType.Assigned)
                    {
                        var csBuilder = IotHubConnectionStringBuilder.Create(provResult.AssignedHub,new DeviceAuthenticationWithRegistrySymmetricKey(provResult.DeviceId, security.GetPrimaryKey()));
                        string connectionString = csBuilder.ToString();
                        return DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
                    }
                    else
                    {
                        throw new IotHubException($"Cant register device: {provResult.ErrorMessage}");
                    }
                }
            }
        }

        internal static async Task<DeviceClient> ProvisionDeviceWithCertAsync(string scopeId, string X509LocatorString, string dcmId)
        {
            using (var transport = new ProvisioningTransportHandlerMqtt())
            {
                var cert = X509Loader.GetCertFromConnectionString(X509LocatorString);
                using (var security = new SecurityProviderX509Certificate(cert))
                {
                    DeviceRegistrationResult provResult;
                    var provClient = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net", scopeId, security, transport);
                    if (!String.IsNullOrEmpty(dcmId))
                    {
                        provResult = await provClient.RegisterAsync(GetProvisionPayload(dcmId)).ConfigureAwait(false);
                    }
                    else
                    {
                        provResult = await provClient.RegisterAsync().ConfigureAwait(false);
                    }
                    if (provResult.Status == ProvisioningRegistrationStatusType.Assigned)
                    {
                        var csBuilder = IotHubConnectionStringBuilder.Create(provResult.AssignedHub,new DeviceAuthenticationWithX509Certificate(provResult.DeviceId,security.GetAuthenticationCertificate()));
                        string connectionString = csBuilder.ToString();
                        return DeviceClient.Create(provResult.AssignedHub,new DeviceAuthenticationWithX509Certificate(provResult.DeviceId, security.GetAuthenticationCertificate()), TransportType.Mqtt);
                    }
                    else
                    {
                        throw new IotHubException($"Cant register device: {provResult.ErrorMessage}");
                    }
                }
            }
        }

        static ProvisioningRegistrationAdditionalData GetProvisionPayload(string dcmId)
        {
            return new ProvisioningRegistrationAdditionalData
            {
                JsonData = $@"{{
                    ""__iot:interfaces"":
                    {{
                        ""CapabilityModelId"": ""{dcmId}""
                    }}
                }}",
            };
        }
    }
}