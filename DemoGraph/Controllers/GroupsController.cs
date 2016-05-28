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
    public class GroupsController : Controller
    {
        public async Task<ActionResult> Index(bool? onlyMyGroups)
        {
            ViewBag.OnlyMyGroups = onlyMyGroups.GetValueOrDefault(false);
            IList<Group> groups = null;
            if (ViewBag.OnlyMyGroups)
            {
                groups = await Helpers.GraphHelper.GetMyGroupsAsync();
            }
            else
            {
                groups = await Helpers.GraphHelper.GetGroupsAsync();
            }
            return View(groups);
        }

        public async Task<ActionResult> AddGroup(string title, string alias, string description)
        {
            Group group = new Group();
            group.DisplayName = title;
            group.MailEnabled = true;
            group.MailNickname = alias;
            group.SecurityEnabled = true;
            group.Description = description;
            var grpTypes = new List<string>() { "Unified" };
            group.GroupTypes = grpTypes;
            await Helpers.GraphHelper.AddGroupAsync(group);

            Thread.Sleep(1000);
            return RedirectToAction("Index");
        }

    }
}