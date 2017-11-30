using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;

//{
//	"EventDateAndTime" : "2018-01-01 10:30 AM",
//	"Location" : "Here",
//	"Invitees" : [
//		{ "Name" : "Stepa",
//		  "Email" : "kapusta@gmair.org"	},
//		{ "Name" : "Vasily",
//		  "Email" : "vas@gmail.com" }
//	]
//}


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

    public static class CreateEvent
    {
        [FunctionName("CreateEvent")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req,
            [Queue("emails", Connection = "AzureWebJobsStorage")]
            IAsyncCollector<EmailDetails> emailsQueue,
            TraceWriter log)
        {
            log.Info("CreateEvent function  is processing a request.");

            var eventDetails = await req.Content.ReadAsAsync<EventDetails>();

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
                    ResponseUrl = $"https://sk-workshop.azurewebsites.net/index.html?code={accessCode}"
                };
                await emailsQueue.AddAsync(emailDetails);
            }

            log.Info("CreateEvent function processed a request.");

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
