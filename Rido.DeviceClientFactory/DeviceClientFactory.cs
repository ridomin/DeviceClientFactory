using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        string invalidOptionsMessage = string.Empty;

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

        public async Task<DeviceClient> CreateDeviceClientAsync()
        {
            if (connectionStringType.Equals(ConnectionStringType.Invalid))
            {
                throw new ApplicationException("Invalid connection string: " + invalidOptionsMessage);
            }

            switch (connectionStringType)
            {
                case ConnectionStringType.DirectSas:
                    return await Task.FromResult(DeviceClient.CreateFromConnectionString(_connectionString, TransportType.Mqtt)).ConfigureAwait(false);
                case ConnectionStringType.DirectCert:
                    return await Task.FromResult(DeviceClient.Create(this.HostName, new DeviceAuthenticationWithX509Certificate(this.DeviceId, X509.FindCertFromLocalStore(this.X509Thumbprint)), TransportType.Mqtt)).ConfigureAwait(false);
                case ConnectionStringType.DPSSas:
                    return await DPS.ProvisionDeviceWithSasKeyAsync(this.ScopeId, this.DeviceId, this.SharedAccessKey, this.DcmId).ConfigureAwait(false);
                case ConnectionStringType.DPSCert:
                    return await DPS.ProvisionDeviceWithCertAsync(this.ScopeId, this.X509Thumbprint, this.DcmId).ConfigureAwait(false);
                default:
                    return null;
            }
        }

        void ParseConnectionString(string connectionString)
        {
            string GetConnectionStringValue(IDictionary<string, string> dict, string propertyName)
            {
                if (!dict.TryGetValue(propertyName, out string value))
                {
                    _logger.LogInformation($"The connection string is missing the property: {propertyName}");
                }
                else
                {
                    _logger.LogInformation($"Connection Property Found: {propertyName}={value}");
                }
                return value;
            }

            void ValidateParams()
            {
                if (!string.IsNullOrWhiteSpace(this.HostName)) //direct 
                {
                    if (!string.IsNullOrWhiteSpace(this.DeviceId) && !string.IsNullOrWhiteSpace(this.SharedAccessKey)) // direct sas
                    {
                        this.connectionStringType = ConnectionStringType.DirectSas;
                    }
                    else if (!string.IsNullOrWhiteSpace(this.X509Thumbprint) && !string.IsNullOrWhiteSpace(this.DeviceId)) // direct with cert
                    {
                        this.connectionStringType = ConnectionStringType.DirectCert;
                    }
                    else
                    {
                        this.invalidOptionsMessage = "Direct connection string require Sas or X509 credential";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(this.ScopeId)) // use DPS 
                {
                    if (!string.IsNullOrWhiteSpace(this.SharedAccessKey)) // use group enrollment key
                    {
                        this.connectionStringType = ConnectionStringType.DPSSas;
                    }
                    else if (!string.IsNullOrWhiteSpace(this.X509Thumbprint))
                    {
                        this.connectionStringType = ConnectionStringType.DPSCert;
                    }
                    else
                    {
                        this.invalidOptionsMessage = "DPS connection string require Sas or X509 credential";
                    }
                }
            }

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
            ValidateParams();
        }
    }
}
