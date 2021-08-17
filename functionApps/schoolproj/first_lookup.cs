using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;


namespace schoolproj
{
    public static class first_lookup
    {
        [FunctionName("first_lookup")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "first_lookup/{id}")] HttpRequest req, string id,
            [Table("first")] CloudTable cloudTable,
            ILogger log)
        {
            string msg;
            TableQuery<TableEntity> Query = new TableQuery<TableEntity>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            TableQuerySegment<TableEntity> res = await cloudTable.ExecuteQuerySegmentedAsync(Query, null);
            TableEntity first = res.FirstOrDefault();
            if (first == null)
                msg = "ok";
            else
                msg = "image required";
            return new OkObjectResult(msg);
        }

        [FunctionName("first_delete")]
        public static async Task<IActionResult> first_delete(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "first_delete/{id}")] HttpRequest req, string id,
    [Table("first")] CloudTable cloudTable,
    ILogger log)
        {
            string msg;
            TableQuery<TableEntity> Query = new TableQuery<TableEntity>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            TableQuerySegment<TableEntity> res = await cloudTable.ExecuteQuerySegmentedAsync(Query, null);
            TableEntity first = res.FirstOrDefault();
            if (first == null)
                msg = "not found";
            else
            {
                TableOperation removeOperation = TableOperation.Delete(first);
                TableResult tableResult = await cloudTable.ExecuteAsync(removeOperation);
                msg = "deleted";
            }

            return new OkObjectResult(msg);
        }
    }
}
