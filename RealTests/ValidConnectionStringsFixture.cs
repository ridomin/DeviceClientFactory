using System;
using Xunit;
using Rido;
using Microsoft.Extensions.Logging.Abstractions;

namespace RealTests
{
    public class ValidConnectionStringFixture
    {
        NullLogger<DeviceClientFactory> _logger = new NullLogger<DeviceClientFactory>();

        [Fact]
        public void DirectConnectionString()
        {
            string cs = "HostName=myhub.azure-devices.net;DeviceId=myDevice;SharedAccessKey=asd8f789fa9s8u9suf9s8udf9as8uf8d";
            ValidConnectingString(cs);
        }

        [Fact]
        public void DPSonnectionString()
        {
            string cs = "ScopeId=0ne123123;DeviceId=myDevice;SharedAccessKey=s0f98as0d9f8as0d89fsa0d89f0asd89fsadf";
            ValidConnectingString(cs);
        }

        [Fact]
        public void DPSonnectionStringWithCert()
        {
            string cs = "ScopeId=0ne12312;X509Thumbprint=1231231423459243859328";
            ValidConnectingString(cs);
        }
        
        [Fact]
        public void DPSonnectionStringWithCertAndDcm()
        {
            string cs = "ScopeId=0ne12312;X509Thumbprint=1231231423459243859328;DcmId=urn:company:interface:1";
            ValidConnectingString(cs);
        }

        //Therory]
        public void ValidConnectingString(string connectionString)
        {
            var deviceFactory = new DeviceClientFactory(connectionString, _logger);
            Assert.NotNull(deviceFactory);   
            Assert.True(deviceFactory.parsedOk);
        }
    }
}
