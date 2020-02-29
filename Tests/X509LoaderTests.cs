using Rido;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

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
        public void ParseCertConfigFromThumbprint()
        {
            string certConfigWithPassswd = "261aa9ae4024ea9ad23297c7c4c3c5579692445e";
            var cert = X509Loader.GetCertFromConnectionString(certConfigWithPassswd);
            Assert.NotNull(cert);
        }
    }
}
