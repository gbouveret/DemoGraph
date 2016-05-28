using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DemoGraph.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        public async Task<ActionResult> Index()
        {
            IList<DriveItem> myFiles = await Helpers.GraphHelper.GetFilesAsync();
            return View(myFiles);
        }
    }
}