using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Program5.Controllers
{
    public class TravelController : ApiController
    {
        // GET: api/Travel
        public IEnumerable<string> Get()
        {
            return new[] { "Traveler name cannot be empty!" };
        }

        // GET: api/Travel/5
        public string Get(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new Exception("Traveler name cannot be empty!");
                }

                var travelLogEntities = AzureTableHelper.GetTravelLogEntities(id);
                var result = new List<TravelLog>();
                foreach (var entity in travelLogEntities)
                {
                    var notes = "";
                    try
                    {
                        notes = AzureBlobHelper.GetBlob(entity.BlobUrl);
                    }
                    catch (Exception e)
                    {
                        notes = "Unable to retrieve notes from blob [" + entity.BlobUrl + "] Error: " + e.Message;
                    }

                    var travelLog = new TravelLog
                    {
                        TravelerName = entity.PartitionKey,
                        BlobUrl = entity.BlobUrl,
                        Notes = notes
                    };

                    result.Add(travelLog);
                }

                if (!result.Any())
                {
                    return "No results for traveler [" + id + "]. Forgot to save some logs?";
                }

                return string.Join("-------------------------------------------------------------------------------------------------------------------" + Environment.NewLine, result.Select(TravelLogToString));
            }
            catch (Exception e)
            {
                return "Failed to query travel logs for traveler: [" + id + "] Error: " + e.Message;
            }
        }

        private string TravelLogToString(TravelLog log)
        {
            var result = new StringBuilder();
            result.AppendLine("Traveler: " + log.TravelerName);
            result.AppendLine("Blob Url: " + log.BlobUrl);
            result.AppendLine(log.Notes);

            return result.ToString();
        }

        // POST: api/Travel
        public string Post([FromBody] TravelLog value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value.TravelerName)
                    || string.IsNullOrWhiteSpace(value.City))
                {
                    return "Traveler name or city cannot be empty!";
                }

                var date = DateTime.Now;
                var blobName = value.TravelerName + value.City + date.Ticks + ".txt";
                var notes = new StringBuilder();

                notes.AppendLine("City: " + value.City);
                notes.AppendLine("Date: " + date.ToString("G"));

                try
                {
                    var cityInfo = CityHelper.GetCityInfo(value.City);
                    notes.AppendLine(cityInfo);
                }
                catch (Exception e)
                {
                    notes.AppendLine("Unable to get travel information about city [" + value.City + "] Error: " + e.Message);
                }

                notes.AppendLine("Your personal notes:");
                notes.AppendLine(value.Notes);

                var notesStr = notes.ToString();

                var blobUrl = AzureBlobHelper.UploadBlob(blobName, notesStr);

                var travelLogEntity = new AzureTableHelper.TravelLogEntity(value.TravelerName, blobUrl, date.Ticks);
                AzureTableHelper.SaveTravelLogEntity(travelLogEntity);

                var travelLog = new TravelLog
                {
                    BlobUrl = blobUrl,
                    TravelerName = value.TravelerName,
                    Notes = notesStr
                };

                return TravelLogToString(travelLog);
            }
            catch (Exception e)
            {
                return "Failed to save travel log. Error: " + e.Message;
            }
        }
    }
}
