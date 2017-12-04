using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using Newtonsoft.Json;

namespace FunctionWorkshop
{
    public static class GetEvent
    {
        [FunctionName("GetEvent")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/{id}/responsecode/{responseCode}")]
            HttpRequestMessage req, 
            [Table("eventsTable", "event", "{id}", Connection = "AzureWebJobsStorage")]
            EventTableEntity @event,
            string id,
            string responseCode,
            TraceWriter log)
        {
            log.Info("Start GetEvent");

            if (@event== null)
            {
                log.Warning($"failed to load an event with id {id}");
                return req.CreateResponse(HttpStatusCode.NotFound, "Invalid event");
            }

            var responses = JsonConvert.DeserializeObject<Response[]>(@event.ResponseJson);
            var myResponse = responses.FirstOrDefault(r => string.Equals(r.ResponseCode, responseCode));

            if (myResponse != null)
            {
                return req.CreateResponse(HttpStatusCode.OK, new {
                    EventDateAndTime = @event.EventDateAndTime,
                    Location = @event.Location,
                    MyResponse = myResponse,
                    Responses = responses.Select(r => new   
                    {
                        Name = r.Name, IsPlaying = r.IsPlaying
                    }).ToArray()
                });
            }

            return req.CreateResponse(HttpStatusCode.NotFound, "Invalid Response Code");
        }
    }
}
