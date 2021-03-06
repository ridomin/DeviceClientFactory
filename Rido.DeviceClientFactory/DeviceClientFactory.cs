using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
[assembly: InternalsVisibleToAttribute("Tests")]

namespace Rido
{
    enum ConnectionStringType
    {
        Invalid = 0,
        DirectSas,
        DirectCert,
        DPSSas,
        DPSCert,
        DPSSasMaster
    }

    enum ModelAnnoucement
    {
        OnConnectionOnly = 0,
        OnProvisioningOnly,
        All
    }

    public class DeviceClientFactory
    {
        readonly ILogger logger;

        internal ConnectionStringType connectionStringType = ConnectionStringType.Invalid;
        readonly string invalidOptionsMessage = string.Empty;
        public string ConnectionString;
        internal string _ConnectionString { get; private set; }
        public string HostName { get; set; }
        internal string ScopeId { get; private set; }
        public string DeviceId { get; internal set; }
        public string SharedAccessKey { get;  set; }
        internal string MasterAccessKey { get; private set; }
        internal string X509 { get; private set; }
        public string ModelId { get; private set; }

        static public DeviceClientFactory Instance { get; private set; }

        private DeviceClientFactory(string connectionString, ILogger logger, string modelId)
        {
            this.logger = logger;
            this._ConnectionString = connectionString;
            this.ModelId = modelId;
            this.ParseConnectionString(connectionString);
        }

        /// <summary>
        /// Creates a DeviceClient using an augmented connection string, supporting DPS, SasKeys, X509 and including PnP registration with the Model Id.
        /// See https://github.com/ridomin/deviceclientfactory/ for more connection string samples.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<DeviceClient> CreateDeviceClientAsync(string connectionString, string modelId = "")
        {
            return await CreateDeviceClientAsync(connectionString, new NullLogger<DeviceClientFactory>(), modelId);
        }

        /// <summary>
        /// Creates a DeviceClient using an augmented connection string, supporting DPS, SasKeys, X509 and including PnP registration with the Model Id.
        /// See https://github.com/ridomin/deviceclientfactory/ for more connection string samples.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static async Task<DeviceClient> CreateDeviceClientAsync(string connectionString, ILogger logger, string modelId = "")
        {
            var dcf = new DeviceClientFactory(connectionString, logger, modelId);
            if (dcf.connectionStringType.Equals(ConnectionStringType.Invalid))
            {
                throw new ApplicationException("Invalid connection string: " + dcf.invalidOptionsMessage);
            }
            Instance = dcf;
            logger.LogTrace("Creating Client with options: " + dcf.connectionStringType.ToString());
            switch (dcf.connectionStringType)
            {
                case ConnectionStringType.DirectSas:
                    return await HubConnection.CreateClientFromConnectionString(dcf._ConnectionString, logger, dcf.ModelId);
                case ConnectionStringType.DirectCert:
                    return await HubConnection.CreateClientFromCert(dcf.HostName, dcf.DeviceId, dcf.X509, logger, dcf.ModelId);
                case ConnectionStringType.DPSCert:
                    return await DPS.ProvisionDeviceWithCertAsync(dcf.ScopeId, dcf.X509, dcf.ModelId, logger);
                case ConnectionStringType.DPSSas:
                    return await DPS.ProvisionDeviceWithSasKeyAsync(dcf.ScopeId, dcf.DeviceId, dcf.SharedAccessKey, dcf.ModelId, logger);
                case ConnectionStringType.DPSSasMaster:
                    return await DPS.CreateDeviceWithMasterKey(dcf.ScopeId, dcf.DeviceId, dcf.MasterAccessKey, dcf.ModelId, logger);
                default:
                    return null;
            }
        }

        void ParseConnectionString(string connectionString)
        {
            logger.LogInformation("Parsing: " + connectionString);

            string GetConnectionStringValue(IDictionary<string, string> dict, string propertyName)
            {
                if (!dict.TryGetValue(propertyName, out string value))
                {
                    logger.LogTrace($"The connection string is missing the property: {propertyName}");
                }
                else
                {
                    logger.LogTrace($"Connection Property Found: {propertyName}={value}");
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
                        this.logger.LogWarning("Connection string require Sas or X509 credential");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(this.ScopeId)) // use DPS 
                {
                    if (!string.IsNullOrWhiteSpace(this.SharedAccessKey)) // use group enrollment key
                    {
                        result = ConnectionStringType.DPSSas;
                    }
                    else if (!string.IsNullOrWhiteSpace(this.MasterAccessKey))
                    {
                        result = ConnectionStringType.DPSSasMaster;
                    }
                    else if (!string.IsNullOrWhiteSpace(this.X509))
                    {
                        result = ConnectionStringType.DPSCert;
                    }
                    else
                    {
                        this.logger.LogWarning("Connection string require Sas or X509 credential");
                    }
                }
                return result;
            }

            IDictionary<string, string> map = connectionString.ToDictionary(';', '=');
            if (map == null)
            {
                logger.LogError("Cannot parse connection string");
                return;
            }
            this.HostName = GetConnectionStringValue(map, nameof(this.HostName));
            this.ScopeId = GetConnectionStringValue(map, nameof(this.ScopeId));
            this.DeviceId = GetConnectionStringValue(map, nameof(this.DeviceId));
            this.SharedAccessKey = GetConnectionStringValue(map, nameof(this.SharedAccessKey));
            this.X509 = GetConnectionStringValue(map, nameof(this.X509));
            this.MasterAccessKey = GetConnectionStringValue(map, nameof(this.MasterAccessKey));

            if (string.IsNullOrEmpty(this.ModelId))
            {
                this.ModelId = GetConnectionStringValue(map, nameof(ModelId));
                logger.LogTrace($"Using ModelId from connection string: {this.ModelId}");
            }
            else
            {
                logger.LogTrace($"ModelId set {this.ModelId}, ignoring from connection string.");
            }

            this.connectionStringType = ValidateParams();

            logger.LogInformation($"Connection Type: {this.connectionStringType}");
        }
    }
}
