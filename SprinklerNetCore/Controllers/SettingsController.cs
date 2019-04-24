using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DarkSkyApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SprinklerNetCore.Models;
using SprinklerNetCore.Resources;

namespace SprinklerNetCore.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private SiteInformation _site;

        public SettingsController(ISiteInformation site)
        {
            _site = (SiteInformation)site;
        }

        public void PrepareViewData()
        {
            ViewData["SprinklerName"] = _site.Settings.Name;
            ViewData["Title"] = Text.SettingsTitle;
        }

        // GET: Settings
        [Authorize(Roles = "Admin,User")]
        public ActionResult Index()
        {
            PrepareViewData();
            return View(_site.Settings);
        }

        // GET: Settings/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit()
        {
            PrepareViewData();
            var timeZone = new SelectList(TimeZoneInfo.GetSystemTimeZones(), nameof(TimeZoneInfo.Id), nameof(TimeZoneInfo.Id));
            ViewBag.timeZone = timeZone;
            var langauges = new SelectList(Enum.GetValues(typeof(Language)));
            ViewBag.langauges = langauges;
            return View(_site.Settings);
        }

        // POST: Settings/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IFormCollection collection)
        {
            try
            {
                var forc = _site.Settings;
                forc.Name = collection[nameof(Settings.Name)];
                forc.TimeZoneInfoId = collection[nameof(Settings.TimeZoneInfoId)];
                forc.ApiKey = collection[nameof(Settings.ApiKey)];
                forc.City = collection[nameof(Settings.City)];
                var b = collection[nameof(Settings.FullAutomationMode)].ToArray()[0];
                forc.FullAutomationMode = bool.Parse(b);
                forc.Language = (Language)Enum.Parse(typeof(Language), collection[nameof(Settings.Language)]);
                forc.Latitude = float.Parse(collection[nameof(Settings.Latitude)]);
                forc.Longitude = float.Parse(collection[nameof(Settings.Longitude)]);
                forc.TimeToCheck = TimeSpan.Parse(collection[nameof(Settings.TimeToCheck)]);
                forc.Unit = (Unit)Enum.Parse(typeof(Unit), collection[nameof(Settings.Unit)]);
                forc.PrecipitationPercentForecast = int.Parse(collection[nameof(Settings.PrecipitationPercentForecast)]);
                forc.PrecipitationThresholdActuals = float.Parse(collection[nameof(Settings.PrecipitationThresholdActuals)]);
                forc.PrecipitationThresholdForecast = float.Parse(collection[nameof(Settings.PrecipitationThresholdForecast)]);
                _site.SaveConfiguration();
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(Culture.FromLanguageToCulture(_site.Settings.Language));
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(Culture.FromLanguageToCulture(_site.Settings.Language));
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                PrepareViewData();
                var timeZone = new SelectList(TimeZoneInfo.GetSystemTimeZones(), nameof(TimeZoneInfo.Id), nameof(TimeZoneInfo.Id));
                ViewBag.timeZone = timeZone;
                return View(_site.Settings);
            }
        }
    }
}