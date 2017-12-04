using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SendGrid.Helpers.Mail;
using System.Text;

namespace FunctionWorkshop
{
    public static class SendInvitationEmails
    {
        [FunctionName("SendInvitationEmails")]
        public static void Run(
            [QueueTrigger("emails", Connection = "AzureWebJobsStorage")]EmailDetails myQueueItem, 
            TraceWriter log,
            [SendGrid(ApiKey = "SendGridApiKey")]
            out Mail mail)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            mail = new Mail();
            var personalization = new Personalization();
            personalization.AddTo(new Email(myQueueItem.Email));
            mail.AddPersonalization(personalization);

            var sb = new StringBuilder();
            sb.Append($"Hi {myQueueItem.Name},");
            sb.Append($"<p>You're invited to join us on {myQueueItem.EventDateAndTime} at {myQueueItem.Location}.</p>");
            sb.Append($"<p>Let us know if you can make it by clicking <a href=\"{myQueueItem.ResponseUrl}\">here</a></p>");
            log.Info(sb.ToString());

            var messageContent = new Content("text/html", sb.ToString());
            mail.AddContent(messageContent);
            mail.Subject = "Your invitation...";
            mail.From = new Email("gray.serj@gmail.com");
        }
    }
}
