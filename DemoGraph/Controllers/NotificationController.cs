using DemoGraph.Models;
using Helpers;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DemoGraph.Controllers
{
    public class NotificationController : Controller
    {
        // The notificationUrl endpoint that's registered with the webhook subscription.
        [HttpPost]
        public async Task<ActionResult> Listen()
        {
            // Validate the new subscription by sending the token back to Microsoft Graph.
            // This response is required for each subscription.
            if (this.Request.QueryString["validationToken"] != null)
            {
                var token = Request.QueryString["validationToken"];
                return Content(token, "plain/text");
            }

            // Parse the received notifications.
            else
            {
                try
                {
                    var notifications = new Dictionary<string, Notification>();
                    using (var inputStream = new System.IO.StreamReader(Request.InputStream))
                    {
                        JObject jsonObject = JObject.Parse(inputStream.ReadToEnd());
                        if (jsonObject != null)
                        {
                            // Notifications are sent in a 'value' array.
                            JArray value = JArray.Parse(jsonObject["value"].ToString());
                            foreach (var notification in value)
                            {
                                Notification current = JsonConvert.DeserializeObject<Notification>(notification.ToString());

                                // Check client state to verify the message is from Microsoft Graph. 
                                var subscriptionParams = (Tuple<string, string>)HttpRuntime.Cache.Get("subscriptionId_" + current.SubscriptionId);
                                if (subscriptionParams != null)
                                {
                                    if (current.ClientState == subscriptionParams.Item1)
                                    {
                                        // Just keep the latest notification for each resource.
                                        // No point pulling data more than once.
                                        notifications[current.Resource] = current;
                                    }
                                }
                                if (notifications.Count > 0)
                                {
                                    // Query for the changed messages. 
                                    await HandleMessagesAsync(notifications.Values);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                    // TODO: Handle the exception.
                    // Still return a 202 so the service doesn't resend the notification.
                }
                return new HttpStatusCodeResult(202);
            }
        }

        // Get information about the changed messages and send to browser via SignalR.
        // A production application would typically queue a background job for reliability.
        public async Task HandleMessagesAsync(IEnumerable<Notification> notifications)
        {
            foreach (var notification in notifications)
            {
                try
                {
                    // Get the access token and add it to the request.
                    var subscriptionParams = (Tuple<string, string>)HttpRuntime.Cache.Get("subscriptionId_" + notification.SubscriptionId);
                    string refreshToken = subscriptionParams.Item2;

                    GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider(refreshToken));

                    var msg = await client.Me.Messages[notification.ResourceData.Id].Request().GetAsync();

                    if (msg.Subject.StartsWith("[SitesOnDemand]"))
                    {
                        var newMessage = new Message()
                        {
                            Subject = "Infos sur SitesOnDemand",
                            Body = new ItemBody() { Content = "Tout est disponible sur Nuget !", ContentType = BodyType.Html },
                            ToRecipients = new Recipient[] { new Recipient() { EmailAddress = new EmailAddress() { Address = msg.From.EmailAddress.Address, Name = msg.From.EmailAddress.Name } } }
                        };
                        await client.Me.SendMail(newMessage).Request().PostAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}