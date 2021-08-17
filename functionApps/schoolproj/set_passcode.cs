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
    public static class set_passcode
    {
        [FunctionName("set_passcode")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "set_passcode/{id}")] HttpRequest req,
            [Table("PassingCode")] CloudTable cloudTable, [Table("onetimepermissions")] CloudTable permissions, [Table("Presence")] CloudTable presencetable, string id,
            ILogger log)
        {
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            string msg;
            TableQuery<TableEntity> permissionQuery = new TableQuery<TableEntity>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            TableQuerySegment<TableEntity> res = await permissions.ExecuteQuerySegmentedAsync(permissionQuery, null);
            TableEntity permit = res.FirstOrDefault();
            if(permit != null)
            {
                TableOperation removeOperation = TableOperation.Delete(permit);
                TableResult tableResult = await permissions.ExecuteAsync(removeOperation);
                TableQuery<presence> presenceQuery = new TableQuery<presence>()
    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id), "and", TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DateTime.Today.ToString().Replace("/", "."))));
                TableQuerySegment<presence> queryResult2 = await presencetable.ExecuteQuerySegmentedAsync(presenceQuery, null);
                presence presenceres = queryResult2.FirstOrDefault();
                if (presenceres != null)
                {
                    presenceres.left = easternTime.ToShortTimeString();
                    TableOperation updateOperation = TableOperation.Replace(presenceres);
                    await presencetable.ExecuteAsync(updateOperation);
                }
                else
                {
                    presenceres = new presence();
                    presenceres.left = "null";
                    presenceres.arrived = easternTime.ToShortTimeString();
                    log.LogInformation($"{DateTime.Now.ToShortTimeString()}");
                    presenceres.PartitionKey = DateTime.Today.ToString().Replace("/", ".");
                    presenceres.RowKey = id;
                    TableOperation insertOperation = TableOperation.InsertOrReplace(presenceres);
                    await presencetable.ExecuteAsync(insertOperation);
                }
                msg = "permit";
            }
            else
            {
                TableQuery<passcode> idQuery = new TableQuery<passcode>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));

                TableQuerySegment<passcode> queryResult = await cloudTable.ExecuteQuerySegmentedAsync(idQuery, null);
                passcode pass = queryResult.FirstOrDefault();
                if (pass == null)
                {
                    return new OkObjectResult("Invalid Id");
                }
                pass.code = DateTime.Today.ToString();
                TableOperation updateOperation = TableOperation.Replace(pass);
                await cloudTable.ExecuteAsync(updateOperation);
                msg = "code_set"; 
            }
            return new OkObjectResult(msg);
        }
    }
}

