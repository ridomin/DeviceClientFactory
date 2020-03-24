using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Rido
{
    class X509Loader
    {
        internal static X509Certificate2 GetCertFromConnectionString(string certParam, ILogger log)
        {
            X509Certificate2 result;
            if (certParam.Contains(".pfx")) //is pfx file
            {
                if (certParam.Contains("|")) //has password
                {
                    var parts = certParam.Split('|');
                    log.LogInformation($"Loading cert from {parts[0]} pfx with pwd");
                    result =  LoadCertFromFile(parts[0], parts[1]);
                }
                else // no password
                {
                    log.LogInformation($"Loading cert from {certParam} pfx with no pwd");
                    result = LoadCertFromFile(certParam, string.Empty);
                }
            }
            else // should be a thumbprint
            {
                log.LogInformation($"Loading cert from store by thumprint: {certParam} ");
                result = FindCertFromLocalStore(certParam);
            }
            if (result==null)
            {
                log.LogError("Certificate not found !!");
                throw new ArgumentException($"Certificate '{certParam}' not found.");
            }
            return result;
        }

        static X509Certificate2 LoadCertFromFile(string pathToPfx, string password)
        {
            return new X509Certificate2(pathToPfx, password);
        }

        static X509Certificate2 FindCertFromLocalStore(object findValue, X509FindType findType = X509FindType.FindByThumbprint)
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
                store.Close();
                return cert;
            }
        }
    }
}
