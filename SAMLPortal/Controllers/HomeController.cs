using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SAMLPortal.Misc;
using SAMLPortal.Models;

namespace SAMLPortal.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		public HomeController()
		{
		}

		[AllowAnonymous]
		public IActionResult Index()
		{
			return View();
		}

		[Authorize(Roles = UserRoles.User)]
		public IActionResult Privacy()
		{
			ClaimsPrincipal currentUser = this.User;
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}