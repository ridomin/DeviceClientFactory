using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Rido;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class RealConnectionFixture
    {
        [Fact]
        public async Task DirectWithSas()
        {
            ConnectionStatus reportedStatus = ConnectionStatus.Disconnected;
            var dcf = new DeviceClientFactory("HostName=e2e-test-hub.azure-devices.net;DeviceId=test-sas-01;SharedAccessKey=pRNwmc8UU0fH6vnTZ50PmfqGffii5fWWLNfdQOaBsu8=");
            var dc = await dcf.CreateDeviceClientAsync().ConfigureAwait(false);
            dc.SetConnectionStatusChangesHandler((ConnectionStatus status, ConnectionStatusChangeReason reason) => reportedStatus = status);
            await dc.OpenAsync();
            Assert.Equal(ConnectionStatus.Connected, reportedStatus);
        }
    }
}
