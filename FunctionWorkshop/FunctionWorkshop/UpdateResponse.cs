using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace FunctionWorkshop
{
    public static class UpdateResponse
    {
        [FunctionName("UpdateResponse")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "event/{id}/response/{responseCode}")]
            HttpRequestMessage req,
            [Table("eventsTable", Connection = "AzureWebJobsStorage")]
            CloudTable @events,
            string id,
            string responseCode,
            TraceWriter log)
        {
            log.Info($"Update Response {responseCode} for event {id}.");

            TableOperation operation = TableOperation.Retrieve<EventTableEntity>("event", id);
            var @event = @events.Execute(operation).Result as EventTableEntity;

            if (@event == null)
            {
                log.Warning($"failed to find an event with id {id}");
                req.CreateResponse(HttpStatusCode.NotFound, "invalid event");
            }

            log.Info("deserealizing");

            var responses = JsonConvert.DeserializeObject<Response[]>(@event.ResponseJson);
            var responseToUpdate = responses.FirstOrDefault(r => string.Equals(r.ResponseCode, responseCode));

            if (responseToUpdate == null)
            {

                log.Warning($"failed to find a response with code {responseCode}");
                req.CreateResponse(HttpStatusCode.NotFound, "invalid response");
            }
            
            log.Info("getting body.");

            dynamic response = await req.Content.ReadAsAsync<object>();
            responseToUpdate.IsPlaying = response.IsPlaying;
            @event.ResponseJson = JsonConvert.SerializeObject(responses);

            operation = TableOperation.Replace(@event);
            @events.Execute(operation);

            return req.CreateResponse(HttpStatusCode.OK, "Updated successfully");
        }
    }
}
