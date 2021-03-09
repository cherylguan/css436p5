using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Program5.Controllers
{
    internal class AzureTableHelper
    {
        const string myConnectionString = "DefaultEndpointsProtocol=https;AccountName=xuestorage1;AccountKey=Hhuc9fxOP8Z9EwOaXV7yQLahCs+gf5+ggmTyd9ACSSJSpGLF/TW1XSeevb/tvoWdG4gOYzBHU/O0Gmx/xSUzjQ==;EndpointSuffix=core.windows.net";
        const string myTableName = "traveltable";

        public class TravelLogEntity : TableEntity
        {
            public string BlobUrl { get; set; }

            public TravelLogEntity()
            {

            }

            public TravelLogEntity(string traveler, string blobUrl, long timestamp)
            {
                this.PartitionKey = traveler;
                this.RowKey = timestamp + "";
                this.BlobUrl = blobUrl;
            }
        }

        public static IEnumerable<TravelLogEntity> GetTravelLogEntities(string traveler)
        {
            try
            {
                if (string.IsNullOrEmpty(traveler))
                {
                    throw new Exception("Traveler name cannot be null or empty!");
                }

                // Get table reference.
                CloudStorageAccount storageAccount = storageAccount = CloudStorageAccount.Parse(myConnectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference(myTableName);

                // Query table.
                TableQuery<TravelLogEntity> query = new TableQuery<TravelLogEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, traveler));
                var result = table.ExecuteQuery(query);

                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to query Azure table. Error: " + e.Message);
            }
        }

        public static void SaveTravelLogEntity(TravelLogEntity entity)
        {
            try
            {
                // Create Azure Table if it doesn't already exist.
                CloudTable table = null;
                Helpers.Retry(() =>
                {
                    CloudStorageAccount storageAccount = storageAccount = CloudStorageAccount.Parse(myConnectionString);
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    table = tableClient.GetTableReference(myTableName);
                    if (!table.Exists())
                    {
                        table.CreateIfNotExists();
                    }
                }, 2);

                // Insert data to table.
                Helpers.Retry(() =>
                {
                    TableOperation insertOp = TableOperation.InsertOrReplace(entity);
                    table.Execute(insertOp);
                }, 2);

            }
            catch (Exception e)
            {
                throw new Exception("Failed to insert to Azure table. Error: " + e.Message);
            }
        }
    }
}