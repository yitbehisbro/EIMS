using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdrIMS.Models;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Controllers
{
    public class AccountController : Controller
    {
		private readonly EdrImsProjectContext _context;
		public AccountController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [Authorize]
		public async Task<IActionResult> Index()
		{
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Signout");
            }
            int _userId = (int)HttpContext.Session.GetInt32("UserId");
			if (_context.Users == null)
			{
				return NotFound();
			}

			var user = await _context.Users
				.FirstOrDefaultAsync(m => m.Id == _userId);
			if (user == null)
			{
				return NotFound();
			}

			return View(user);
		}

		public IActionResult AccessDenied()
        {
            //RedirectToAction("Create", "Members");
            return View();
        }
		public IActionResult NotActivated()
        {
            //RedirectToAction("Create", "Members");
            return View();
        }

        public async Task Signout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var prop = new AuthenticationProperties
            {
                RedirectUri = "/"
            };
            // after signout this will redirect to your provided target
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, prop);
        }

		
	}
}
