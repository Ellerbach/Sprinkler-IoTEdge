using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SprinklerNetCore.Models;

namespace SprinklerNetCore.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class ForecastController : Controller
    {
        private SiteInformation _site;

        public ForecastController(ISiteInformation site)
        {
            _site = (SiteInformation)site;
        }

        public void PrepareViewData()
        {
            ViewData["SprinklerName"] = _site.Settings.Name;
            ViewData["Title"] = Resources.Text.ForecastTitle;
        }

        // GET: Forecast        
        public ActionResult Index()
        {
            PrepareViewData();
            return View(_site.GetForecast());
        }                
    }
}