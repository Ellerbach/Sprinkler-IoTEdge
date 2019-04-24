using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SprinklerNetCore.Models;

namespace SprinklerNetCore.Controllers
{
    public class HistoryController : Controller
    {
        private readonly SiteInformation _site;

        public HistoryController(ISiteInformation site)
        {
            _site = (SiteInformation)site;
        }

        public void PrepareViewData()
        {
            ViewData["SprinklerName"] = _site.Settings.Name;
            ViewData["Title"] = Resources.Text.HistoryTitle;
        }

        // GET: History
        public ActionResult Index()
        {
            PrepareViewData();
            return View(_site.HistorySprinkling?.SprinklerPrograms);
        }

    }
}