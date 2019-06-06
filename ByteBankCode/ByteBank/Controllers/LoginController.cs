using ByteBank.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ByteBank.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ByteBank.Controllers
{
	public class LoginController : Controller
	{
		public async Task<ActionResult> Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<ActionResult> Login(ContaLoginViewModel modelo)
		{
			if (ModelState.IsValid)
			{

			}
			return View(modelo);

		}

	}
}