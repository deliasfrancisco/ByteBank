using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Sua página de descrição do aplicativo.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Entre em contato conosco.";

			return View();
		}
	}
}