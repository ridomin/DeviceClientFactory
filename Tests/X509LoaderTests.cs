using Rido;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Security.Cryptography.X509Certificates;
namespace Tests
{
    public class X509LoaderTests
    {
       [Fact]
       public void ParseCertConfigFromFileWithPasswd()
        {
            string certConfigWithPassswd = "mycert.pfx|1234";
            var cert = X509Loader.GetCertFromConnectionString(certConfigWithPassswd);
            Assert.NotNull(cert);
        }

        [Fact]
        public void ParseCertConfigFromFileNoPasswd()
        {
            string certConfigWithPassswd = "mycertnopasswd.pfx";
            var cert = X509Loader.GetCertFromConnectionString(certConfigWithPassswd);
            Assert.NotNull(cert);
        }

        [Fact]
        public void CertFromStore()
        {
            string certConfigWithPassswd = "mycertnopasswd.pfx";
            var cert1 = X509Loader.GetCertFromConnectionString(certConfigWithPassswd);
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert1);
            System.Console.WriteLine(cert1.Thumbprint);
            store.Close();


            string certThumbprint = "29AD021F61B31D505CF64445B6269606E79063B4";
            var cert = X509Loader.GetCertFromConnectionString(certThumbprint);
            Assert.NotNull(cert);
        }
    }
}
