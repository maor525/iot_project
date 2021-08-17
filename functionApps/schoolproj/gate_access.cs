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
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace schoolproj
{
    public static class gate_access
    {
        [FunctionName("gate_access")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "gate_access/{id}/{pw}")] HttpRequest req,
            string id, string pw,
            [Table("Schedule")] CloudTable scheduletable,
            [Table("PassingCode")] CloudTable passcodetable,
            [Table("Students")] CloudTable studentstable,
            [Table("Presence")] CloudTable presencetable,
            [SignalR(HubName = "SchoolHub")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            string msg = "";
            Dictionary<string, string> value;
            value = new Dictionary<string, string> { };
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

            TableQuery<passcode> codeQuery = new TableQuery<passcode>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            TableQuerySegment<passcode> queryResult1 = await passcodetable.ExecuteQuerySegmentedAsync(codeQuery, null);
            passcode passcoderes = queryResult1.FirstOrDefault();
            TableQuery<presence> presenceQuery = new TableQuery<presence>()
    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id),"and", TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DateTime.Today.ToString().Replace("/", "."))));
            TableQuerySegment<presence> queryResult2 = await presencetable.ExecuteQuerySegmentedAsync(presenceQuery, null);
            presence presenceres = queryResult2.FirstOrDefault();
            TableQuery<students> studentsQuery = new TableQuery<students>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            TableQuerySegment<students> queryResult3 = await studentstable.ExecuteQuerySegmentedAsync(studentsQuery, null);
            students studentsres = queryResult3.FirstOrDefault();
            TableQuery<schedule> scheduleQuery = new TableQuery<schedule>()
    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, studentsres.classID), "and", TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, DateTime.Today.DayOfWeek.ToString())));
            TableQuerySegment<schedule> queryResult4 = await scheduletable.ExecuteQuerySegmentedAsync(scheduleQuery, null);
            schedule scheduleres = queryResult4.FirstOrDefault();
            if(studentsres.password != pw)
            {
                value.Add("Access", "rejected");
                msg = "user_err";
            }
            else if (passcoderes.code != DateTime.Today.ToString())
            {
                value.Add("Access", "rejected");
                msg = "expired";
            }
            else if (presenceres != null)
            {
                string[] time = scheduleres.finishTime.Split(":");
                if (easternTime < DateTime.Today.AddHours(Double.Parse(time[0])).AddMinutes(Double.Parse(time[1])))
                {
                    value.Add("Access", "rejected");
                    msg = "schedule_err";
                }
                else if (presenceres.left != "null")
                {
                    value.Add("Access", "rejected");
                    msg = "out_err";
                }
                else
                {
                    presenceres.left = easternTime.ToShortTimeString();
                    TableOperation updateOperation = TableOperation.Replace(presenceres);
                    await presencetable.ExecuteAsync(updateOperation);
                    value.Add("Access", "granted");
                    msg = "updated";
                }
            }
            else
            {
                    presenceres = new presence();
                    presenceres.left = "null";
                    presenceres.arrived = easternTime.ToShortTimeString();
                    log.LogInformation($"{DateTime.Now.ToShortTimeString()}");
                    presenceres.PartitionKey = DateTime.Today.ToString().Replace("/",".");
                    presenceres.RowKey = id;
                    TableOperation insertOperation = TableOperation.InsertOrReplace(presenceres);
                    TableResult tableResult = await presencetable.ExecuteAsync(insertOperation);
                    value.Add("Access", "granted");
                    msg = "updated";
            }
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "GateAccess",
                    Arguments = new object[] { value }
                });
            return new OkObjectResult(msg);
        }
    }
}
