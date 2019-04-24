using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SprinklerNetCore;
using SprinklerNetCore.Models;

namespace SprinklerNetCore.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class ProgramController : Controller
    {
        private SiteInformation _site;

        public ProgramController(ISiteInformation site)
        {
            _site = (SiteInformation)site;
        }

        public void PrepareViewData()
        {
            ViewData["SprinklerName"] = _site.Settings.Name;
            ViewData["Title"] = Resources.Text.ProgramTitle;
        }

        // GET: Program
        public ActionResult Index(int? sprinkler)
        {
            PrepareViewData();
            //ViewBag.Sprinklers = new SelectList(_site.Sprinklers);
            var sprProg = _site.SprinklerPrograms;
            if (sprinkler.HasValue)
                sprProg = sprProg.Where(m => m.Number == sprinkler.Value).ToList();
            ViewBag.Sprinklers = _site.Sprinklers;
            return View(sprProg);
        }

        // GET: Program/Details/5
        public ActionResult Details(Guid id)
        {
            PrepareViewData();
            return View(_site.SprinklerPrograms.Where(m => m.id == id).FirstOrDefault());
        }

        // GET: Program/Create
        public ActionResult Create()
        {
            PrepareViewData();
            ViewData["DateTimeMin"] = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm"); //"2019-02-28T16:00"
            var sprinklers = new SelectList(_site.Sprinklers, nameof(Sprinkler.Number), nameof(Sprinkler.Name));
            ViewBag.sprinklers = sprinklers;
            return View();
        }

        // POST: Program/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                SprinklerProgram sprProg = new SprinklerProgram();
                sprProg.DateTimeStart = DateTimeOffset.Parse(collection[nameof(SprinklerProgram.DateTimeStart)]);
                sprProg.Duration = TimeSpan.Parse(collection[nameof(SprinklerProgram.Duration)]);
                sprProg.Number = Convert.ToInt32(collection[nameof(SprinklerProgram.Number)]);
                _site.SprinklerPrograms.Add(sprProg);
                _site.SaveConfiguration();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewData["DateTimeMin"] = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm"); //"2019-02-28T16:00"
                var sprinklers = new SelectList(_site.Sprinklers, nameof(Sprinkler.Number), nameof(Sprinkler.Name));
                ViewBag.sprinklers = sprinklers;
                PrepareViewData();
                return View();
            }
        }

        // GET: Program/Edit/5
        public ActionResult EditTypical(int id)
        {
            PrepareViewData();
            ViewData["DateTimeMin"] = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm"); //"2019-02-28T16:00"
            var sprProg = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault();
            if (sprProg.TypicalProgram == null)
            {
                sprProg.TypicalProgram = new TypicalProgram()
                {
                    Duration = new TimeSpan(0, 10, 0),
                    StartTime = new TimeSpan(1, 0, 0)
                };
                _site.SaveConfiguration();
            }
            return View(sprProg.TypicalProgram);
        }

        // POST: Program/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTypical(int id, IFormCollection collection)
        {
            try
            {
                var sprProg = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault().TypicalProgram;
                sprProg.Duration = TimeSpan.Parse(collection[nameof(TypicalProgram.Duration)]);
                sprProg.StartTime = TimeSpan.Parse(collection[nameof(TypicalProgram.StartTime)]);
                _site.SaveConfiguration();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                PrepareViewData();
                return View();
            }
        }

        // GET: Program/Edit/5
        public ActionResult Edit(Guid id)
        {
            ViewData["DateTimeMin"] = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm"); //"2019-02-28T16:00"
            var sprinklers = new SelectList(_site.Sprinklers, nameof(Sprinkler.Number), nameof(Sprinkler.Name));
            ViewBag.sprinklers = sprinklers;
            PrepareViewData();
            return View(_site.SprinklerPrograms.Where(m => m.id == id).FirstOrDefault());
        }

        // POST: Program/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id, IFormCollection collection)
        {
            try
            {
                var sprProg = _site.SprinklerPrograms.Where(m => m.id == id).FirstOrDefault();
                sprProg.DateTimeStart = DateTimeOffset.Parse(collection[nameof(SprinklerProgram.DateTimeStart)]);
                sprProg.Duration = TimeSpan.Parse(collection[nameof(SprinklerProgram.Duration)]);
                sprProg.Number = Convert.ToInt32(collection[nameof(SprinklerProgram.Number)]);
                _site.SaveConfiguration();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewData["DateTimeMin"] = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm"); //"2019-02-28T16:00"
                var sprinklers = new SelectList(_site.Sprinklers, nameof(Sprinkler.Number), nameof(Sprinkler.Name));
                ViewBag.sprinklers = sprinklers;
                PrepareViewData();
                return View();
            }
        }

        // GET: Program/Delete/5
        public ActionResult Delete(Guid id)
        {
            PrepareViewData();
            return View(_site.SprinklerPrograms.Where(m => m.id == id).FirstOrDefault());
        }

        // POST: Program/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id, IFormCollection collection)
        {
            try
            {
                var sprProg = _site.SprinklerPrograms.Where(m => m.id == id).FirstOrDefault();
                _site.SprinklerPrograms.Remove(sprProg);
                _site.SaveConfiguration();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                PrepareViewData();
                return View();
            }
        }
    }
}