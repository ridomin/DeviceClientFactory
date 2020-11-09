using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace dps_client_func
{
    public static class Provision
    {
        [FunctionName("Provision")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Device Client Factory Function");
            log.LogInformation(req.Query["DCF"]);

            string name = WebUtility.UrlDecode(req.Query["DCF"]);
            log.LogInformation(name);
            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            _ = await Rido.DeviceClientFactory.CreateDeviceClientAsync(name);
            string responseMessage = "https://mqtt.rido.dev?cs=" + WebUtility.UrlEncode(Rido.DeviceClientFactory.Instance.ConnectionString);
            return new OkObjectResult(responseMessage);
        }
    }
}
