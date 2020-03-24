using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
[assembly: InternalsVisibleToAttribute("Tests")]

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
        readonly ILogger _logger;

        internal ConnectionStringType connectionStringType = ConnectionStringType.Invalid;
        readonly string invalidOptionsMessage = string.Empty;

        internal string HostName { get; private set; }
        internal string ScopeId { get; private set; }
        internal string DeviceId { get; private set; }
        internal string SharedAccessKey { get; private set; }
        internal string X509 { get; private set; }
        internal string DcmId { get; private set; }

        static public DeviceClientFactory Instance { get; private set; }

        [Obsolete("From v02 use the static method CreateDeviceClientAsync.")]
        public DeviceClientFactory(string connectionString) : this(connectionString, new NullLogger<DeviceClientFactory>())
        {
        }

        [Obsolete("From v02 use the static method CreateDeviceClientAsync.")]
        public DeviceClientFactory(string connectionString, ILogger logger)
        {
            _logger = logger;
            this.ParseConnectionString(connectionString);
        }

        /// <summary>
        /// Creates a DeviceClient using an augmented connection string, supporting DPS, SasKeys, X509 and including PnP registration with the DCM Id.
        /// See https://github.com/ridomin/deviceclientfactory/ for more connection string samples.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<DeviceClient> CreateDeviceClientAsync(string connectionString)
        {
            return await CreateDeviceClientAsync(connectionString, new NullLogger<DeviceClientFactory>());
        }

        /// <summary>
        /// Creates a DeviceClient using an augmented connection string, supporting DPS, SasKeys, X509 and including PnP registration with the DCM Id.
        /// See https://github.com/ridomin/deviceclientfactory/ for more connection string samples.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static async Task<DeviceClient> CreateDeviceClientAsync(string connectionString, ILogger logger)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var dcf = new DeviceClientFactory(connectionString, logger);
#pragma warning restore CS0618 // Type or member is obsolete
            if (dcf.connectionStringType.Equals(ConnectionStringType.Invalid))
            {
                throw new ApplicationException("Invalid connection string: " + dcf.invalidOptionsMessage);
            }
            Instance = dcf;
            switch (dcf.connectionStringType)
            {
                case ConnectionStringType.DirectSas:
                    return await Task.FromResult(DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt)).ConfigureAwait(false);
                case ConnectionStringType.DirectCert:
                    return await Task.FromResult(DeviceClient.Create(dcf.HostName, new DeviceAuthenticationWithX509Certificate(dcf.DeviceId, X509Loader.GetCertFromConnectionString(dcf.X509, logger)), TransportType.Mqtt)).ConfigureAwait(false);
                case ConnectionStringType.DPSSas:
                    return await DPS.ProvisionDeviceWithSasKeyAsync(dcf.ScopeId, dcf.DeviceId, dcf.SharedAccessKey, dcf.DcmId, logger).ConfigureAwait(false);
                case ConnectionStringType.DPSCert:
                    return await DPS.ProvisionDeviceWithCertAsync(dcf.ScopeId, dcf.X509, dcf.DcmId, logger).ConfigureAwait(false);
                default:
                    return null;
            }
        }

        void ParseConnectionString(string connectionString)
        {
            _logger.LogInformation("Parsing: " + connectionString);
            
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

            ConnectionStringType ValidateParams()
            {
                ConnectionStringType result = ConnectionStringType.Invalid;
                if (!string.IsNullOrWhiteSpace(this.HostName)) //direct 
                {
                    if (!string.IsNullOrWhiteSpace(this.DeviceId) && !string.IsNullOrWhiteSpace(this.SharedAccessKey)) // direct sas
                    {
                        result = ConnectionStringType.DirectSas;
                    }
                    else if (!string.IsNullOrWhiteSpace(this.X509) && !string.IsNullOrWhiteSpace(this.DeviceId)) // direct with cert
                    {
                        result = ConnectionStringType.DirectCert;
                    }
                    else
                    {
                        this._logger.LogWarning("Connection string require Sas or X509 credential");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(this.ScopeId)) // use DPS 
                {
                    if (!string.IsNullOrWhiteSpace(this.SharedAccessKey)) // use group enrollment key
                    {
                        result = ConnectionStringType.DPSSas;
                    }
                    else if (!string.IsNullOrWhiteSpace(this.X509))
                    {
                        result = ConnectionStringType.DPSCert;
                    }
                    else
                    {
                        this._logger.LogWarning("Connection string require Sas or X509 credential");
                    }
                }

                return result;
            }

            IDictionary<string, string> map = connectionString.ToDictionary(';', '=');
            if (map==null)
            {
                _logger.LogError("Cannot parse connection string");
                return;
            }    
            this.HostName = GetConnectionStringValue(map, nameof(this.HostName));
            this.ScopeId = GetConnectionStringValue(map, nameof(this.ScopeId));
            this.DeviceId = GetConnectionStringValue(map, nameof(this.DeviceId));
            this.SharedAccessKey = GetConnectionStringValue(map, nameof(this.SharedAccessKey));
            this.X509 = GetConnectionStringValue(map, nameof(this.X509));
            this.DcmId = GetConnectionStringValue(map, nameof(DcmId));
            this.connectionStringType = ValidateParams();
            
            _logger.LogInformation($"Connection Tyoe: {this.connectionStringType}");

        }
    }
}
