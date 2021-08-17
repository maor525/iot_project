using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using QRCoder;
using static QRCoder.PayloadGenerator;
/*
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.EventHubs;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Devices; 
*/


namespace schoolproj
{
    public static class shirfunc
    {
        /* APP FUNCTIONS */
        [FunctionName("login")]
        public static async Task<string> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "login/{id}&{password}")] HttpRequestMessage request,
            [Table("Students")] CloudTable studentTable,
            [Table("Parents")] CloudTable parentTable,
            string id, string password,
            ILogger log)
        {
            log.LogInformation("Logging in: id = " + id);

            // If student:
            TableQuery<students> idQuery = new TableQuery<students>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));

            TableQuerySegment<students> queryResult = await studentTable.ExecuteQuerySegmentedAsync(idQuery, null);
            students user = queryResult.FirstOrDefault();

            if (user != null)
            {
                if (user.password.Equals(password))
                    return "student " + user.name;
                else
                    return "wrong password";
            }
            // If parent
            TableQuery<parents> idQuery1 = new TableQuery<parents>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));

            TableQuerySegment<parents> queryResult1 = await parentTable.ExecuteQuerySegmentedAsync(idQuery1, null);
            parents user1 = queryResult1.FirstOrDefault();

            if (user1 != null)
            {
                if (user1.password.Equals(password))
                    return "parent " + user1.name;
                else
                    return "wrong password";
            }
            return "-1";
        }

        [FunctionName("getChildPresence")]
        public static async Task<string> GetChildPresence(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getChildPresence/{id}")] HttpRequestMessage request,
            [Table("Students")] CloudTable studentTable, [Table("Presence")] CloudTable presenceTable,
            string id,
            ILogger log)
        {
            log.LogInformation("Getting Child Presence: ParentID = " + id);

            // Find children
            TableQuery<students> idQuery = new TableQuery<students>()
                .Where(TableQuery.GenerateFilterCondition("parentID", QueryComparisons.Equal, id));

            TableQuerySegment<students> queryResult = await studentTable.ExecuteQuerySegmentedAsync(idQuery, null);
            students[] children = queryResult.ToArray<students>();

            if (children == null)
                return "no such parent";

            // Get today's presence information
            string output = "";
            TableQuery<presence> idQuery1;
            TableQuerySegment<presence> queryResult1;
            presence presence;
            string today = DateTime.Today.AddHours(12).ToString("MM/dd/yyyy hh:mm:ss").Replace("/", ".") + " AM";

            foreach (students child in children)
            {
                output += child.RowKey + "|" + child.name + ": ";
                idQuery1 = new TableQuery<presence>()
                    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, today.Substring(1)), "and", TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, child.RowKey)));

                queryResult1 = await presenceTable.ExecuteQuerySegmentedAsync(idQuery1, null);
                presence = queryResult1.FirstOrDefault();

                if (presence == null)
                {
                    output += "Not arrived yet.\n";
                    continue;
                }
                if (presence.left == "null")
                    presence.left = "-";
                output += "Arrival: " + presence.arrived + ", Exit: " + presence.left + "\n";
            }
            return output;
        }

        [FunctionName("getAttendance")]
        public static async Task<string> getAttendance(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getAttendance/{dates}/{id}")] HttpRequestMessage request,
    [Table("Presence")] CloudTable presenceTable,
    string dates, string id,
    ILogger log)
        {
            string output = "";
            string[] dates_arr = dates.Split('|');
            for(int i=0; i < dates_arr.Length; i++)
            {
                log.LogInformation(dates_arr[i]);
                TableQuery<presence> idQuery1 = new TableQuery<presence>()
                    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, dates_arr[i]), "and", TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)));
                TableQuerySegment<presence> queryResult1 = await presenceTable.ExecuteQuerySegmentedAsync(idQuery1, null);
                presence res = queryResult1.FirstOrDefault();
                if(res == null)
                {
                    output += dates_arr[i].Split(' ')[0] + "?-?-|";
                }
                else
                {
                    output += dates_arr[i].Split(' ')[0] + "?" + res.arrived;
                    if (res.left == "null")
                        output += "?-|";
                    else
                        output += "?" + res.left + "|";
                }
                log.LogInformation(output);
            }
            return output;
        }


        [FunctionName("get_student")]
        public static async Task<IActionResult> get_student(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "get_student/{id}")] HttpRequest req,
    string id, [Table("Students")] CloudTable studentstable, [Table("Parents")] CloudTable parentstable,
    ILogger log)
        {
            string msg;
            TableQuery<students> studentsQuery = new TableQuery<students>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            TableQuerySegment<students> queryResult3 = await studentstable.ExecuteQuerySegmentedAsync(studentsQuery, null);
            students studentsres = queryResult3.FirstOrDefault();
            if (studentsres == null)
                return new OkObjectResult("id not found");

            TableQuery<parents> parentsQuery = new TableQuery<parents>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, studentsres.parentID));
            TableQuerySegment<parents> queryResult = await parentstable.ExecuteQuerySegmentedAsync(parentsQuery, null);
            parents parentsres = queryResult.FirstOrDefault();
            msg = id + " " + studentsres.name + " " + studentsres.parentID + " " + studentsres.classID + " " + studentsres.password + " " + parentsres.name + " " + parentsres.password;
            return new OkObjectResult(msg);
        }


    /* DEVICE FUNCTIONS */

    [FunctionName("QRGenerator")]
        public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
    ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string url = req.Query["url"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            url = url ?? data?.url;
            if (string.IsNullOrEmpty(url))
            {
                return new BadRequestResult();
            }
            var isAbsoluteUrl = Uri.TryCreate(url, UriKind.Absolute, out Uri resultUrl);
            if (!isAbsoluteUrl)
            {
                return new BadRequestResult();
            }

            var generator = new Url(resultUrl.AbsoluteUri);
            var payload = generator.ToString();

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeAsPng = qrCode.GetGraphic(20);
                return new FileContentResult(qrCodeAsPng, "image/png");
            }
        }


        /* Utils */
        private static async Task<T> ExtractContent<T>(HttpRequestMessage request)
        {
            string connectionRequestJson = await request.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(connectionRequestJson);
        }
    }
}
