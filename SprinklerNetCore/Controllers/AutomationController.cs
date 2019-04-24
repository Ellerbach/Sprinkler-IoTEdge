using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SprinklerNetCore.Models;
using SprinklerNetCore.Resources;

namespace SprinklerNetCore.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class AutomationController : Controller
    {
        private SiteInformation _site;

        public AutomationController(ISiteInformation site)
        {
            _site = (SiteInformation)site;
        }

        public void PrepareViewData()
        {
            ViewData["SprinklerName"] = _site.Settings.Name;
            ViewData["Title"] = Text.AutomationTitle;
        }

        // GET: Automation
        public ActionResult Index()
        {
            PrepareViewData();
            return View(_site.FuzzySprinklers);
        }

        // GET: Automation/Details/5
        public ActionResult Details(Guid id)
        {
            PrepareViewData();
            return View(_site.FuzzySprinklers.Where(m => m.id == id).FirstOrDefault());
        }

        // GET: Automation/Create
        public ActionResult Create()
        {
            PrepareViewData();
            return View();
        }

        // POST: Automation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                FuzzySprinkler fuzzy = new FuzzySprinkler();
                fuzzy.TempMin = float.Parse(collection[nameof(FuzzySprinkler.TempMin)], NumberStyles.Any);
                fuzzy.TempMax = float.Parse(collection[nameof(FuzzySprinkler.TempMax)], NumberStyles.Any);
                fuzzy.RainMax = float.Parse(collection[nameof(FuzzySprinkler.RainMax)], NumberStyles.Any);
                fuzzy.SprinklingMax = float.Parse(collection[nameof(FuzzySprinkler.SprinklingMax)], NumberStyles.Any);
                _site.FuzzySprinklers.Add(fuzzy);
                _site.SaveConfiguration();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                PrepareViewData();
                ViewData["msg"] = $"Error: {ex.Message}";
                return View();
            }
        }

        // GET: Automation/Edit/5
        public ActionResult Edit(Guid id)
        {
            PrepareViewData();
            return View(_site.FuzzySprinklers.Where(m => m.id == id).FirstOrDefault());
        }

        // POST: Automation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id, IFormCollection collection)
        {
            try
            {
                FuzzySprinkler fuzzy = _site.FuzzySprinklers.Where(m => m.id == id).FirstOrDefault();
                if (fuzzy != null)
                {
                    fuzzy.TempMin = float.Parse(collection[nameof(FuzzySprinkler.TempMin)],NumberStyles.Any);
                    fuzzy.TempMax = float.Parse(collection[nameof(FuzzySprinkler.TempMax)], NumberStyles.Any);
                    fuzzy.RainMax = float.Parse(collection[nameof(FuzzySprinkler.RainMax)], NumberStyles.Any);
                    fuzzy.SprinklingMax = float.Parse(collection[nameof(FuzzySprinkler.SprinklingMax)], NumberStyles.Any);
                    _site.SaveConfiguration();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                PrepareViewData();
                ViewData["msg"] = $"Error: {ex.Message}";
                return View(_site.FuzzySprinklers.Where(m => m.id == id).FirstOrDefault());
            }
        }

        // GET: Automation/Delete/5
        public ActionResult Delete(Guid id)
        {
            PrepareViewData();
            return View(_site.FuzzySprinklers.Where(m => m.id == id).FirstOrDefault());
        }

        // POST: Automation/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id, IFormCollection collection)
        {
            try
            {
                var toDel = _site.FuzzySprinklers.Where(m => m.id == id).FirstOrDefault();
                if (toDel != null)
                {
                    _site.FuzzySprinklers.Remove(toDel);
                    _site.SaveConfiguration();
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}