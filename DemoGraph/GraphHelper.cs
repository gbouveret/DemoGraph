using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using DemoGraph.Models;
using Newtonsoft.Json.Linq;

namespace Helpers
{
    public static class GraphHelper
    {
        public static async Task<IList<Contact>> GetContactsAsync()
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());
                        
            var myContacts = await client.Me.Contacts.Request().GetAsync();
            IList<Contact> contacts = myContacts.CurrentPage;

            return contacts;
        }

        public static async Task<IList<Message>> GetMessagesAsync(string folderId, string filter)
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());

            IMailFolderRequestBuilder mailBox = client.Me.MailFolders.Inbox;
            if (!string.IsNullOrEmpty(folderId)) {
                switch (folderId)
                {
                    case "DeletedItems":
                        mailBox = client.Me.MailFolders.DeletedItems;
                        break;
                    case "Drafts":
                        mailBox = client.Me.MailFolders.Drafts;
                        break;
                    case "SentItems":
                        mailBox = client.Me.MailFolders.SentItems;
                        break;
                    default:
                        mailBox = client.Me.MailFolders[folderId];
                        break;
                }
            }
            var myMessages = await mailBox.Messages.Request().Filter(filter).GetAsync();

            IList <Message> messages = myMessages.CurrentPage;

            return messages;
        }

        public static async Task<IList<Group>> GetGroupsAsync()
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());

            var myGroups = await client.Groups.Request().GetAsync();

            IList<Group> groups = myGroups.CurrentPage;

            return groups;
        }
        public static async Task<IList<Group>> GetMyGroupsAsync()
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());
            
            HttpRequestMessage reqMsg = client.Me.MemberOf.References.Request().GetHttpRequestMessage();
            reqMsg.RequestUri = new Uri("https://graph.microsoft.com/v1.0/me/memberOf/$/microsoft.graph.group?$filter=groupTypes/any(a:a%20eq%20'unified')");
            await client.AuthenticationProvider.AuthenticateRequestAsync(reqMsg);

            var response = await client.HttpProvider.SendAsync(reqMsg);

            var json = await response.Content.ReadAsStringAsync();

            MyGroups grps = JsonConvert.DeserializeObject(json, typeof(MyGroups)) as MyGroups;

            return grps.value.ToList();
        }

        public class MyGroups
        {
            public Group[] value;
        }
        public static async Task<Group> GetGroupAsync(string groupId)
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());

            var myGroup = await client.Groups[groupId].Request().GetAsync();
           
            return myGroup;
        }

        public static async Task<DriveItem> GetFileAsync(string fileId)
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());

            var myFile = await client.Drive.Items[fileId].Request().GetAsync();

            return myFile;
        }

        public static async Task<IList<DriveItem>> GetFilesAsync()
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());

            List<Option> options = new List<Option>();
            //options.Add(new Option(""))
            
            var myRecentFiles = await client.Me.Drive.Root.Children.Request().OrderBy("Name").GetAsync();

            IList<DriveItem> files = myRecentFiles.CurrentPage;

            return files;
        } 

        public static async Task AddGroupAsync(Group group)
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());

            await client.Groups.Request().AddAsync(group);            
        }

        public static async Task<CellModel> GetExcelCellValue(string fileId, string sheetId, string range)
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());
            
            // Then get values
            string getValues = string.Format("https://graph.microsoft.com/beta/me/drive/items/{0}/workbook/worksheets/{1}/range(address='{2}')/text", fileId, sheetId, range);
            HttpRequestMessage reqMsgValues = new HttpRequestMessage(HttpMethod.Get, getValues);
            await client.AuthenticationProvider.AuthenticateRequestAsync(reqMsgValues);
            var respValues = await client.HttpProvider.SendAsync(reqMsgValues);

            string json = await respValues.Content.ReadAsStringAsync();
            dynamic dynObj = JsonConvert.DeserializeObject(json);

            JArray jvals = dynObj.value.value as JArray;
            //vals.ToArray<string>();

            string[][] vals = new string[jvals.Count()][];
            for (var i = 0; i < jvals.Count(); i++)
            {
                vals[i] = new string[jvals[i].Count()];
                for (var j = 0; j < jvals[i].Count(); j++)
                {
                    vals[i][j] = (string)jvals[i][j];
                }
            }
            return new CellModel() { Range = range, Values = vals };
        }

        public static async Task DeleteMessageAsync(string messageId)
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());
            await client.Me.Messages[messageId].Request().DeleteAsync();
        }

        public static async Task<IList<Subscription>> GetSubscriptionsAsync()
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());
            var subsc = await client.Subscriptions.Request().GetAsync();

            return subsc.CurrentPage;
        }

        public static async Task<Subscription> AddSubscriptionAsync(Subscription sub)
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());
            Subscription result = await client.Subscriptions.Request().AddAsync(sub);
            return result;
        }

        public static async Task<Subscription> GetSubscriptionAsync(string subscriptionId)
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());
            var sub = await client.Subscriptions[subscriptionId].Request().GetAsync();
            return sub;
        }
        public static async Task DeleteSubscriptionAsync(string subscriptionId)
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());
            await client.Subscriptions[subscriptionId].Request().DeleteAsync();
        }
        public static async Task SendMessagesAsync(string to, string subject, string body)
        {
            GraphServiceClient client = new GraphServiceClient(new GraphAuthenticationProvider());

            var newMessage = new Message()
            {
                Subject = subject,
                Body = new ItemBody() { Content = body, ContentType = BodyType.Html },                
                ToRecipients = new Recipient[] { new Recipient() {  EmailAddress = new EmailAddress() { Address = to} } }
            };
            await client.Me.SendMail(newMessage).Request().PostAsync();            
        }
    }
}