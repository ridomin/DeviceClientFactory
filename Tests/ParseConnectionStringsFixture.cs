using System;
using Xunit;
using Rido;
using Microsoft.Extensions.Logging.Abstractions;

namespace RealTests
{
    public class ParseConnectionStringFixture
    {
        [Theory]
        [InlineData("HostName=myhub.azure-devices.net;DeviceId=myDevice;SharedAccessKey=asd8f789fa9s8u9suf9s8udf9as8uf8d")]
        [InlineData("ScopeId=0ne123123;DeviceId=myDevice;SharedAccessKey=s0f98as0d9f8as0d89fsa0d89f0asd89fsadf")]
        [InlineData("ScopeId=0ne123123;X509Thumbprint=1231231423459243859328")]
        [InlineData("ScopeId=0ne12312;X509Thumbprint=1231231423459243859328;DcmId=urn:company:interface:1")]
        public void ValidConnectingString(string connectionString)
        {
            var deviceFactory = new DeviceClientFactory(connectionString);
            Assert.NotNull(deviceFactory);   
            Assert.NotEqual(ConnectionStringType.Invalid, deviceFactory.connectionStringType);
        }

        [Theory]
        [InlineData("")]
        [InlineData("empty")]
        [InlineData("nohostname=")]
        [InlineData("hostname=aa;noscret=")]
        public void InvalidConnectingString(string connectionString)
        {
            var deviceFactory = new DeviceClientFactory(connectionString);
            Assert.NotNull(deviceFactory);
            Assert.Equal(ConnectionStringType.Invalid,  deviceFactory.connectionStringType);
        }
    }
}
