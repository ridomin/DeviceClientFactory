using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Rido;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RealTests
{
    public class RealConnectionFixture
    {
        [Fact]
        public async Task DirectWithSas()
        {
            ConnectionStatus reportedStatus = ConnectionStatus.Disconnected;
            var dcf = new DeviceClientFactory("HostName=e2e-test-hub.azure-devices.net;DeviceId=test-01;SharedAccessKey=PfUZju2cm7huZKN1VtkIgL5pz2jMQzsPtGZqPG7Iv7s=");
            var dc = await dcf.CreateDeviceClient().ConfigureAwait(false);
            dc.SetConnectionStatusChangesHandler((ConnectionStatus status, ConnectionStatusChangeReason reason) => reportedStatus = status);
            await dc.OpenAsync();
            Assert.Equal(ConnectionStatus.Connected, reportedStatus);
        }
    }
}
