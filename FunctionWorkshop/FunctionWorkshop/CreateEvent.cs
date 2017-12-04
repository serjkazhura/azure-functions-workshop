using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;

namespace FunctionWorkshop
{
    public class EventDetails
    {
        public DateTime EventDateAndTime { get; set; }
        public string Location { get; set; }
        public List<InviteeDetails> Invitees { get; set; }
    }

    public class InviteeDetails
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class EmailDetails
    {
        public DateTime EventDateAndTime { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ResponseUrl { get; set; }
    }

    public class EventTableEntity : TableEntity
    {
        public DateTime EventDateAndTime { get; set; }
        public string Location { get; set; }
        public string ResponseJson { get; set; }
    }

    public class Response
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string IsPlaying { get; set; }
        public string ResponseCode { get; set; }
    }

    public static class CreateEvent
    {
        [FunctionName("CreateEvent")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req,
            [Queue("emails", Connection = "AzureWebJobsStorage")]
            IAsyncCollector<EmailDetails> emailsQueue,
            [Table("eventsTable", Connection = "AzureWebJobsStorage")]
            IAsyncCollector<EventTableEntity> eventsTable,
            TraceWriter log)
        {
            log.Info("CreateEvent function  is processing a request.");

            var eventDetails = await req.Content.ReadAsAsync<EventDetails>();
            var responses = new List<Response>();
            var eventId = Guid.NewGuid().ToString("n");

            foreach (var invitee in eventDetails.Invitees)
            {
                log.Info($"Inviting {invitee.Name} ({invitee.Email})");
                var accessCode = Guid.NewGuid().ToString("n");
                var emailDetails = new EmailDetails
                {
                    EventDateAndTime = eventDetails.EventDateAndTime,
                    Location = eventDetails.Location,
                    Name = invitee.Name,
                    Email = invitee.Email,
                    ResponseUrl = $"https://sk-workshop.azurewebsites.net/index.html?eventId={eventId}&code={accessCode}"
                };
                await emailsQueue.AddAsync(emailDetails);
                responses.Add(new Response()
                {
                    Name = invitee.Name,
                    Email = invitee.Email,
                    IsPlaying = "unknown",
                    ResponseCode = accessCode
                });
            }

            await eventsTable.AddAsync(new EventTableEntity()
            {
                PartitionKey = "event",
                RowKey = eventId,
                EventDateAndTime = eventDetails.EventDateAndTime,
                Location = eventDetails.Location,
                ResponseJson = JsonConvert.SerializeObject(responses)
            });

            log.Info("CreateEvent function processed a request.");

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
