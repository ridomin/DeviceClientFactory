using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Rido.DeviceClientFactoryFunction
{
    public static class Provision
    {
        [FunctionName("Provision")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string dcf = req.Query["DCF"];

            _ = await Rido.DeviceClientFactory.CreateDeviceClientAsync(dcf);


            string url = $"https://mqtt.rido.dev?cs=";
            return new OkObjectResult(url);
        }
    }
}
