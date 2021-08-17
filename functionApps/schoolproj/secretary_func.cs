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
    public static class secretary_func
    {
        [FunctionName("gate_permission")]
        public static async Task<IActionResult> gate_permission(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "gate_permission/{id}")] HttpRequest req,
            string id, [Table("onetimepermissions")] CloudTable permissions, [Table("Students")] CloudTable studentstable,
            ILogger log)
        {
            // find student
            TableQuery<students> studentsQuery = new TableQuery<students>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            TableQuerySegment<students> queryResult3 = await studentstable.ExecuteQuerySegmentedAsync(studentsQuery, null);
            students studentsres = queryResult3.FirstOrDefault();

            if (studentsres == null)
                return new OkObjectResult("id not found");

            TableEntity per = new TableEntity();
            per.RowKey = id;
            per.PartitionKey = "";
            TableOperation add = TableOperation.InsertOrReplace(per);
            await permissions.ExecuteAsync(add);
            return new OkObjectResult("updated");
        }


        [FunctionName("mod_student")]
        public static async Task<IActionResult> mod_student(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "add_student/{id}/{method}/{name}/{class_id}/{parent_id}/{pw}/{p_name}/{parent_pw}")] HttpRequest req,
    string id, string method, string name, string class_id, string parent_id, string pw, string p_name, string parent_pw, [Table("Students")] CloudTable studentstable,
    [Table("Parents")] CloudTable parentstable, [Table("PassingCode")] CloudTable pctable, [Table("first")] CloudTable ftable, ILogger log)
        {
            string prev_parent_id = parent_id;
            string msg = "success";
            // get student
            TableQuery<students> studentsQuery = new TableQuery<students>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            TableQuerySegment<students> queryResult3 = await studentstable.ExecuteQuerySegmentedAsync(studentsQuery, null);
            students studentsres = queryResult3.FirstOrDefault();

            if (method == "remove")
            {
                if (studentsres != null)
                {
                    parent_id = studentsres.parentID;
                    prev_parent_id = studentsres.parentID;
                    TableOperation removeOperation = TableOperation.Delete(studentsres);
                    TableResult tableResult = await studentstable.ExecuteAsync(removeOperation);
                }
                else
                    msg = "no id found";
            }
            else if (method == "add")
            {
                if (studentsres == null)
                {
                    studentsres = new students();
                    studentsres.PartitionKey = "";
                    studentsres.classID = class_id;
                    studentsres.RowKey = id;
                    studentsres.password = pw;
                    studentsres.parentID = parent_id;
                    studentsres.name = name;
                    TableOperation insertOperation = TableOperation.InsertOrReplace(studentsres);
                    TableResult tableResult = await studentstable.ExecuteAsync(insertOperation);
                    passcode pc = new passcode();
                    pc.PartitionKey = "";
                    pc.RowKey = id;
                    pc.code = "";
                    TableEntity f = new TableEntity();
                    f.RowKey = id;
                    f.PartitionKey = "";
                    TableOperation insertOperation1 = TableOperation.InsertOrReplace(pc);
                    TableResult tableResult1 = await pctable.ExecuteAsync(insertOperation1);
                    TableOperation insertOperation2 = TableOperation.InsertOrReplace(f);
                    TableResult tableResult2 = await ftable.ExecuteAsync(insertOperation);

                }
                else
                    msg = "id already exists";
            }
            else if (method == "modify")
            {
                if (studentsres != null)
                {
                    prev_parent_id = studentsres.parentID;

                    studentsres = new students();
                    studentsres.PartitionKey = "";
                    studentsres.classID = class_id;
                    studentsres.RowKey = id;
                    studentsres.password = pw;
                    studentsres.parentID = parent_id;
                    studentsres.name = name;
                    TableOperation insertOperation = TableOperation.InsertOrReplace(studentsres);
                    TableResult tableResult = await studentstable.ExecuteAsync(insertOperation);
                }
                else
                    msg = "no id found";
            }
            mod_parents(id, method, parent_id, prev_parent_id, p_name, parent_pw, studentstable, parentstable, log);
            return new OkObjectResult(msg);
        }


        public static async void mod_parents(
    string id, string method, string new_parent_id, string prev_parent_id, string p_name, string pw, [Table("Students")] CloudTable studentstable, [Table("Parents")] CloudTable parentstable,
    ILogger log)
        {
            // get prev_parent
            TableQuery<parents> parentsQuery = new TableQuery<parents>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, prev_parent_id));
            TableQuerySegment<parents> queryResult4 = await parentstable.ExecuteQuerySegmentedAsync(parentsQuery, null);
            parents parentsres = queryResult4.FirstOrDefault();


            if (method == "add")
            {
                parentsres = new parents();
                parentsres.PartitionKey = "";
                parentsres.RowKey = new_parent_id;
                parentsres.password = pw;
                parentsres.name = p_name;
                TableOperation insertOperation = TableOperation.InsertOrReplace(parentsres);
                await parentstable.ExecuteAsync(insertOperation);
            }

            if (method == "remove")
            {
                TableQuery<students> studentsQuery = new TableQuery<students>()
    .Where(TableQuery.GenerateFilterCondition("parentID", QueryComparisons.Equal, prev_parent_id));
                TableQuerySegment<students> queryResult3 = await studentstable.ExecuteQuerySegmentedAsync(studentsQuery, null);
                students studentsres = queryResult3.FirstOrDefault();

                if (studentsres == null)
                {
                    TableOperation removeOperation = TableOperation.Delete(parentsres);
                    await parentstable.ExecuteAsync(removeOperation);
                }
            }

            if (method == "modify")
            {
                // Remove prev ?
                TableQuery<students> studentsQuery = new TableQuery<students>()
    .Where(TableQuery.GenerateFilterCondition("parentID", QueryComparisons.Equal, prev_parent_id));
                TableQuerySegment<students> queryResult3 = await studentstable.ExecuteQuerySegmentedAsync(studentsQuery, null);
                students studentsres = queryResult3.FirstOrDefault();
                if (studentsres == null)
                {
                    // remove parent
                    TableOperation removeOperation = TableOperation.Delete(parentsres);
                    await parentstable.ExecuteAsync(removeOperation);
                }

                // add new / modify
                parentsres = new parents();
                parentsres.PartitionKey = "";
                parentsres.RowKey = new_parent_id;
                parentsres.password = pw;
                parentsres.name = p_name;
                TableOperation insertOperation = TableOperation.InsertOrReplace(parentsres);
                await parentstable.ExecuteAsync(insertOperation);

            }

        }
    }

}