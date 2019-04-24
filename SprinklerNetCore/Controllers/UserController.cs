using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SprinklerNetCore.Data;
using SprinklerNetCore.Models;

namespace SprinklerNetCore.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly SiteInformation _site;

        public UserController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ISiteInformation site)
        {
            _context = context;
            _userManager = userManager;
            _site = (SiteInformation)site;
            // Add the first user to admin
            var users = _userManager.Users;
            if (users.Count() == 1)
            {
                var usr = users.First();
                _userManager.AddToRoleAsync(usr, "Admin").Wait();
            }
            else
            {
                //Check if anyone is admin. If no one, then make the first user Admin
                var usr = _userManager.GetUsersInRoleAsync("Admin").GetAwaiter().GetResult();
                if ((usr.Count == 0) && (users.Count() > 0))
                {
                    _userManager.AddToRoleAsync(users.First(), "Admin").Wait();
                }
            }

        }

        public void PrepareViewData()
        {
            ViewData["SprinklerName"] = _site.Settings.Name;
            ViewData["Title"] = Resources.Text.UserTitle;
            ViewData["Create"] = Resources.Text.Create;
            ViewData["Delete"] = Resources.Text.Delete;
            ViewData["Edit"] = Resources.Text.Edit;
            ViewData["Details"] = Resources.Text.Details;
            ViewData["Index"] = Resources.Text.Index;
            ViewData["IndexBackToList"] = Resources.Text.IndexBackToList;
            ViewData["Save"] = Resources.Text.Save;
        }


        [Authorize]
        // GET: User
        public ActionResult Index()
        {
            PrepareViewData();
            var user = _userManager.GetUserAsync(HttpContext.User).GetAwaiter().GetResult();
            if (!_userManager.GetRolesAsync(user).GetAwaiter().GetResult().Where(m => m == "Admin").Any())
                return Redirect("/Identity/Account/AccessDenied?ReturnUrl=%2Fuser");
            
            List<UserRoles> users = new List<UserRoles>();
            foreach (var usr in _userManager.Users)
            {
                UserRoles ur = new UserRoles();
                ur.Email = usr.Email;                
                ur.Roles = new List<string>(_userManager.GetRolesAsync(usr).GetAwaiter().GetResult());
                users.Add(ur);
            }
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        // GET: User/Edit/5
        public ActionResult Edit(string id)
        {
            PrepareViewData();
            UserRoles ur = new UserRoles();
            var theUsr = _userManager.Users.Where(m => m.Email == id).FirstOrDefault();
            ur.Email = theUsr.Email;
            ur.Roles = new List<string>(_userManager.GetRolesAsync(theUsr).GetAwaiter().GetResult());
            var roles = RolesInitialization.Roles.ToList();
            roles.Add("None");
            ViewBag.Roles = new SelectList(roles);
            return View(ur);
        }

        // POST: User/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, IFormCollection collection)
        {
            try
            {
                var theUsr = _userManager.Users.Where(m => m.Email == id).FirstOrDefault();
                // Get User roles
                var roles = _userManager.GetRolesAsync(theUsr).GetAwaiter().GetResult();
                // Check the new selected role
                var role = collection[nameof(UserRoles.Roles)]; //[0];
                if(role == "None")
                {
                    // Remove all roles
                    foreach (var rl in roles)
                        _userManager.RemoveFromRoleAsync(theUsr, rl).Wait();
                }
                else if (roles.Where(m => m == role).Count() != 1)
                {
                    _userManager.AddToRoleAsync(theUsr, role);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                PrepareViewData();
                return View();
            }
        }

        // GET: User/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            PrepareViewData();
            ViewData["DeleteConfirmation"] = Resources.Text.DeleteConfirmation;
            return View("Delete", _userManager.Users.Where(m => m.Email == id).FirstOrDefault().Email);
        }

        // POST: User/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, IFormCollection collection)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(id);
                
                var rolesForUser = await _userManager.GetRolesAsync(user);

                using (var transaction = _context.Database.BeginTransaction())
                {

                    if (rolesForUser.Count() > 0)
                    {
                        foreach (var item in rolesForUser.ToList())
                        {
                            // item should be the name of the role
                            var result = await _userManager.RemoveFromRoleAsync(user, item);
                        }
                    }
                    await _userManager.DeleteAsync(user);
                    transaction.Commit();
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                PrepareViewData();
                ViewData["DeleteConfirmation"] = Resources.Text.DeleteConfirmation;
                return View();
            }
        }
    }
}