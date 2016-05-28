using DemoGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DemoGraph.Controllers
{
    public class WorkbookController : Controller
    {
        public async Task<ActionResult> Index(string fileId, string sheetId, string range)
        {
            CellModel model = null;
            ViewBag.Files = await Helpers.GraphHelper.GetFilesAsync();

            if (!string.IsNullOrEmpty(fileId))
            {
                model = await Helpers.GraphHelper.GetExcelCellValue(fileId, sheetId, range);
            }
            return View(model);
        }
    }
}