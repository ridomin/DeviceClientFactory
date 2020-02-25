using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
[assembly: InternalsVisibleToAttribute("RealTests")]

namespace Rido
{

    enum ConnectionStringType
    {
        Invalid=0,
        DirectSas,
        DirectCert,
        DPSSas,
        DPSCert,
    }

    public class DeviceClientFactory
    {
        readonly string _connectionString;
        readonly ILogger _logger;

        internal ConnectionStringType connectionStringType = ConnectionStringType.Invalid;
        bool usePnP = false;

        internal string HostName { get; private set; }
        internal string ScopeId { get; private set; }
        internal string DeviceId { get; private set; }
        internal string SharedAccessKey { get; private set; }
        internal string X509Thumbprint { get; private set; }
        internal string DcmId { get; private set; }

        public DeviceClientFactory(string connectionString) : this(connectionString, new NullLogger<DeviceClientFactory>())
        {
        }

        public DeviceClientFactory(string connectionString, ILogger logger)
        {
            _logger = logger;
            _connectionString = connectionString;
            this.ParseConnectionString(connectionString);
        }

        public async Task<DeviceClient> CreateDeviceClient()
        {
            switch (connectionStringType)
            {
                case ConnectionStringType.DirectSas:
                    return await Task.FromResult<DeviceClient>(DeviceClient.CreateFromConnectionString(_connectionString)).ConfigureAwait(false);
                case ConnectionStringType.DirectCert:
                    return await Task.FromResult(DeviceClient.Create(this.HostName, new DeviceAuthenticationWithX509Certificate(this.DeviceId, FindCertFromLocalStore(this.X509Thumbprint)))).ConfigureAwait(false);
                case ConnectionStringType.DPSSas:
                    return await ProvisionDeviceWithSasKey(this.ScopeId, this.DeviceId, this.SharedAccessKey).ConfigureAwait(false);
                case ConnectionStringType.DPSCert:
                    return await ProvisionDeviceWithCert(this.ScopeId, this.X509Thumbprint).ConfigureAwait(false);
                default:
                    return null;
            }
        }

        async Task<DeviceClient> ProvisionDeviceWithSasKey(string scopeId, string deviceId, string deviceKey)
        {
            using (var transport = new ProvisioningTransportHandlerMqtt())
            {
                using (var security = new SecurityProviderSymmetricKey(deviceId, deviceKey, null))
                {
                    DeviceRegistrationResult provResult;
                    var provClient = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net", scopeId, security, transport);

                    if (this.usePnP)
                    {
                        provResult = await provClient.RegisterAsync(GetProvisionPayload(DcmId)).ConfigureAwait(false);
                    }
                    else
                    {
                        provResult = await provClient.RegisterAsync().ConfigureAwait(false);
                    }

                    _logger.LogWarning($"Device Provisoning with SAS result: {provResult.Status} {provResult.DeviceId} {provResult.AssignedHub} with DCM {DcmId}");

                    if (provResult.Status == ProvisioningRegistrationStatusType.Assigned)
                    {
                        var csBuilder = IotHubConnectionStringBuilder.Create(provResult.AssignedHub,
                            new DeviceAuthenticationWithRegistrySymmetricKey(provResult.DeviceId, security.GetPrimaryKey()));
                        string connectionString = csBuilder.ToString();
                        _logger.LogWarning($"DeviceId:{provResult.DeviceId}");
                        return DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
                    }
                    else
                    {
                        throw new IotHubException("Cant register device, reason:" + provResult.ErrorMessage);
                    }
                }
            }
        }

        async Task<DeviceClient> ProvisionDeviceWithCert(string scopeId, string X509Thumbprint)
        {
            using (var transport = new ProvisioningTransportHandlerMqtt())
            {
                var cert = FindCertFromLocalStore(X509Thumbprint);
                using (var security = new SecurityProviderX509Certificate(cert))
                {
                    DeviceRegistrationResult provResult;
                    var provClient = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net", scopeId, security, transport);

                    if (this.usePnP)
                    {
                        provResult = await provClient.RegisterAsync(GetProvisionPayload(DcmId)).ConfigureAwait(false);
                    }
                    else
                    {
                        provResult = await provClient.RegisterAsync().ConfigureAwait(false);
                    }

                    _logger.LogWarning($"Device Provisoning with X509 result: {provResult.Status} {provResult.DeviceId} {provResult.AssignedHub} with DCM {DcmId}");

                    if (provResult.Status == ProvisioningRegistrationStatusType.Assigned)
                    {
                        var csBuilder = IotHubConnectionStringBuilder.Create(provResult.AssignedHub,
                            new DeviceAuthenticationWithX509Certificate(provResult.DeviceId,
                            security.GetAuthenticationCertificate()));
                        string connectionString = csBuilder.ToString();

                        return DeviceClient.Create(provResult.AssignedHub,
                            new DeviceAuthenticationWithX509Certificate(provResult.DeviceId, security.GetAuthenticationCertificate()), TransportType.Mqtt);
                    }
                    else
                    {
                        throw new IotHubException("Cant register device, reason:" + provResult.ErrorMessage);
                    }
                }
            }
        }

        void ValidateParams()
        {
            if (!string.IsNullOrWhiteSpace(this.HostName)) //direct 
            {
                if (!string.IsNullOrWhiteSpace(this.DeviceId) && !string.IsNullOrWhiteSpace(this.SharedAccessKey)) // direct sas
                {
                    connectionStringType = ConnectionStringType.DirectSas;
                }
                if (!string.IsNullOrWhiteSpace(this.X509Thumbprint) && !string.IsNullOrWhiteSpace(this.DeviceId)) // direct with cert
                {
                    connectionStringType = ConnectionStringType.DirectCert;
                }
            }
            else if (!string.IsNullOrWhiteSpace(this.ScopeId)) // use DPS 
            {
                if (!string.IsNullOrWhiteSpace(this.SharedAccessKey)) // use group enrollment key
                {
                    connectionStringType = ConnectionStringType.DPSSas;
                }
                else if (!string.IsNullOrWhiteSpace(this.X509Thumbprint))
                {
                    connectionStringType = ConnectionStringType.DPSCert;
                }
            }
            if (!string.IsNullOrWhiteSpace(this.DcmId))
            {
                usePnP = true;
            }
        }

        void ParseConnectionString(string connectionString)
        {
            _logger.LogInformation("Parsing: " + connectionString);
            IDictionary<string, string> map = connectionString.ToDictionary(';', '=');
            if (map==null)
            {
                return;
            }    
            this.HostName = GetConnectionStringValue(map, nameof(this.HostName));
            this.ScopeId = GetConnectionStringValue(map, nameof(this.ScopeId));
            this.DeviceId = GetConnectionStringValue(map, nameof(this.DeviceId));
            this.SharedAccessKey = GetConnectionStringValue(map, nameof(this.SharedAccessKey));
            this.X509Thumbprint = GetConnectionStringValue(map, nameof(this.X509Thumbprint));
            this.DcmId = GetConnectionStringValue(map, nameof(DcmId));
            this.ValidateParams();
        }

        X509Certificate2 FindCertFromLocalStore(object findValue, X509FindType findType = X509FindType.FindByThumbprint)
        {
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                X509Certificate2 cert = null;
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(findType, findValue, false);
                if ((certs != null) && certs.Count > 0)
                {
                    cert = certs[0];
                }
                else
                {
                    _logger.LogError("cert not found");
                }
                store.Close();
                return cert;
            }
        }

        string GetConnectionStringValue(IDictionary<string, string> map, string propertyName)
        {
            if (!map.TryGetValue(propertyName, out string value))
            {
                _logger.LogInformation($"The connection string is missing the property: {propertyName}");
            }
            else
            {
                _logger.LogInformation($"Connection Property Found: {propertyName}={value}");
            }
            return value;
        }

        ProvisioningRegistrationAdditionalData GetProvisionPayload(string dcmId)
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


    public static class StringExtensions
    {
        public static IDictionary<string, string> ToDictionary(this string valuePairString, char kvpDelimiter, char kvpSeparator)
        {
            if (string.IsNullOrWhiteSpace(valuePairString))
            {
                return null;
            }

            IEnumerable<string[]> parts = new Regex($"(?:^|{kvpDelimiter})([^{kvpDelimiter}{kvpSeparator}]*){kvpSeparator}")
                .Matches(valuePairString)
                .Cast<Match>()
                .Select(m => new string[] {
                    m.Result("$1"),
                    valuePairString.Substring(
                        m.Index + m.Value.Length,
                        (m.NextMatch().Success ? m.NextMatch().Index : valuePairString.Length) - (m.Index + m.Value.Length))
                });

            if (!parts.Any() || parts.Any(p => p.Length != 2))
            {
                return null;
            }

            IDictionary<string, string> map = parts.ToDictionary(kvp => kvp[0], (kvp) => kvp[1], StringComparer.OrdinalIgnoreCase);

            return map;
        }
    }
}
