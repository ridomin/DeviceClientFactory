using System.Security.Cryptography.X509Certificates;

namespace Rido
{
    class X509
    {
        internal static X509Certificate2 FindCertFromLocalStore(object findValue, X509FindType findType = X509FindType.FindByThumbprint)
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
