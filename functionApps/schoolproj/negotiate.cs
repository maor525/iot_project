using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace schoolproj
{
    public static class negotiate
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
                [SignalRConnectionInfo(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "SchoolHub")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }
    }
}
