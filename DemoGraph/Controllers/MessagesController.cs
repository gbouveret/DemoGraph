using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DemoGraph.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        public async Task<ActionResult> Index(string folderId, string filter)
        {
            ViewBag.FolderId = folderId;
            ViewBag.Filter = filter;
            Dictionary<string, string> foldersOptions = new Dictionary<string, string>();
            foldersOptions.Add("Inbox", "Boîte de réception");
            foldersOptions.Add("SentItems", "Eléments envoyés");
            foldersOptions.Add("Drafts", "Brouillons");
            foldersOptions.Add("DeletedItems", "Eléments supprimés");
            ViewBag.FoldersOptions = foldersOptions;

            IList<Message> msgs = await Helpers.GraphHelper.GetMessagesAsync(folderId, filter);

            return View(msgs);
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(string to, string subject, string body)
        {
            await Helpers.GraphHelper.SendMessagesAsync(to, subject, body);
            Thread.Sleep(1000);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteMessage(string mid)
        {
            await Helpers.GraphHelper.DeleteMessageAsync(mid);

            return RedirectToAction("Index");
        }
    }
}