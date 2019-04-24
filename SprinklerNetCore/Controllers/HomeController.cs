using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SprinklerNetCore.Models;
using SprinklerNetCore.Resources;

namespace SprinklerNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly SiteInformation _site;

        public HomeController(ISiteInformation site)
        {
            _site = (SiteInformation)site;
        }

        public void PrepareViewData()
        {
            ViewData["Title"] = Text.HomeTitle;
            ViewData["SprinklerName"] = _site.Settings.Name;
            ViewData["IsClosed"] = Text.IsClosed;
            ViewData["IsOpen"] = Text.IsOpen;
        }

        public IActionResult Index()
        {
            PrepareViewData();            
            return View(new HomeModel()
            {
                Sprinklers = _site.Sprinklers,
                NeedToSrpinkle = _site.GetForecast(),
                DateTime = DateTime.Now.Add(TimeZoneInfo.FindSystemTimeZoneById(_site.Settings.TimeZoneInfoId).GetUtcOffset(DateTime.Now)).ToString("yyyy/MM/dd HH:mm")
            });
        }

        public IActionResult Privacy()
        {
            PrepareViewData();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            PrepareViewData();
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult SetCulture(string id = "en-us")
        {
            string culture = id;
            Response.Cookies.Append(
               CookieRequestCultureProvider.DefaultCookieName,
               CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
               new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);
            //ViewData["Message"] = "Culture set to " + culture;
            return Redirect(nameof(Index));

        }
    }
}
