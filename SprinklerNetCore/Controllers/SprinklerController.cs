using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SprinklerNetCore.Models;

namespace SprinklerNetCore.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class SprinklerController : Controller
    {
        private SiteInformation _site;

        public SprinklerController(ISiteInformation site)
        {
            _site = (SiteInformation)site;
        }

        public void PrepareViewData()
        {
            ViewData["SprinklerName"] = _site.Settings.Name;
            ViewData["IsClosed"] = Resources.Text.IsClosed;
            ViewData["IsOpen"] = Resources.Text.IsOpen;
            ViewData["Title"] = Resources.Text.SprinklerTitle;            
        }

        // GET: Sprinkler
        public ActionResult Index()
        {
            PrepareViewData();            
            return View(_site.Sprinklers);
        }

        // GET: Sprinkler/Details/5
        public ActionResult Details(int id)
        {
            PrepareViewData();
            var spr = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault();
            return View(spr);
        }

        // GET: Sprinkler/Typical/5
        public ActionResult Typical(int id)
        {
            try
            {
                var spr = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault();
                var typic = spr.TypicalProgram;
                SprinklerProgram sprProg = new SprinklerProgram()
                {
                    Duration = typic.Duration,
                    DateTimeStart = (DateTimeOffset.Now.TimeOfDay > typic.StartTime) ? DateTimeOffset.Now.Date.AddDays(1).Add(typic.StartTime) : DateTimeOffset.Now.Date.Add(typic.StartTime),
                    Number = id
                };
                _site.SprinklerPrograms.Add(sprProg);
                _site.SaveConfiguration();
            }
            catch (Exception)
            { }

            return RedirectToAction(nameof(Index));
        }

        // GET: Sprinkler/Create
        public ActionResult Create()
        {
            // Get all the numbers            
            var lst = new SelectList(GetAvailableSprinklers());
            if (lst.Count() > 0)
            {
                ViewBag.Numbers = lst;
                PrepareViewData();
                return View();
            }
            else
                return RedirectToAction(nameof(Index));

        }
        
        private List<int> GetAvailableSprinklers()
        {
            List<int> SprAvailable = new List<int>();
            for (int i = 0; i < _site.MaxSprinklers; i++)
            {
                var isNum = _site.Sprinklers.Where(m => m.Number == i);
                if (isNum.FirstOrDefault() == null)
                {
                    SprAvailable.Add(i);
                }
            }
            return SprAvailable;
        }

        // POST: Sprinkler/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // Chceck if Number is valid
                var num = Convert.ToInt32(collection[nameof(Sprinkler.Number)]);
                if (num >= _site.MaxSprinklers)
                {
                    return View();
                }
                if (_site.Sprinklers.Where(m => m.Number == num).FirstOrDefault() == null)
                {
                    Sprinkler spr = new Sprinkler();
                    spr.Number = num;
                    spr.Name = collection[nameof(Sprinkler.Name)];
                    spr.IsInverted = Convert.ToBoolean(collection[nameof(Sprinkler.IsInverted)]);
                    _site.Sprinklers.Add(spr);
                    _site.SaveConfiguration();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {

            }
            PrepareViewData();
            return View();
        }

        // GET: Sprinkler/Open/5
        public ActionResult Open(int id, bool state, string page)
        {
            var spr = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault();
            spr.Open = state;
            if (page == null)
                return RedirectToAction(nameof(Index));
            else
                return RedirectToAction(page, new { id = spr.Number });
        }


        // GET: Sprinkler/Edit/5
        public ActionResult Edit(int id)
        {
            PrepareViewData();            
            var spr = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault();
            var avail = GetAvailableSprinklers();
            avail.Add(id);
            ViewBag.Numbers = new SelectList(avail);
            return View(spr);
        }

        // POST: Sprinkler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // Chceck if Number is valid
                var num = Convert.ToInt32(collection[nameof(Sprinkler.Number)]);
                var spr = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault();
                if (spr != null)
                {
                    spr.Number = Convert.ToInt32(collection[nameof(Sprinkler.Number)]);
                    spr.Name = collection[nameof(Sprinkler.Name)];
                    var invert = collection[nameof(Sprinkler.IsInverted)].FirstOrDefault();
                    spr.IsInverted = Convert.ToBoolean(invert);
                    _site.SaveConfiguration();
                    return RedirectToAction(nameof(Index));
                }

            }
            catch (Exception ex)
            {
                ViewData["msg"] = ex.Message;
            }
            PrepareViewData();
            return View();
        }

        // GET: Sprinkler/EditTypical/5
        public ActionResult EditTypical(int id)
        {
            PrepareViewData();
            var spr = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault().TypicalProgram;
            return View(spr);
        }

        // POST: Sprinkler/EditTypical/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTypical(int id, IFormCollection collection)
        {
            try
            {
                // Chceck if Number is valid
                var num = Convert.ToInt32(collection[nameof(Sprinkler.Number)]);
                var spr = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault();
                if (spr != null)
                {
                    spr.TypicalProgram.Duration = TimeSpan.Parse(collection[nameof(TypicalProgram.Duration)]);
                    spr.TypicalProgram.StartTime = TimeSpan.Parse(collection[nameof(TypicalProgram.StartTime)]);
                    _site.SaveConfiguration();
                    return RedirectToAction(nameof(Index));
                }

            }
            catch
            {

            }
            PrepareViewData();
            return View();
        }


        // GET: Sprinkler/Delete/5
        public ActionResult Delete(int id)
        {
            PrepareViewData();
            var spr = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault();
            return View(spr);
        }

        // POST: Sprinkler/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var spr = _site.Sprinklers.Where(m => m.Number == id).FirstOrDefault();
                _site.Sprinklers.Remove(spr);
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