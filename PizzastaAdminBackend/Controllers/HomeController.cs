using Microsoft.AspNetCore.Mvc;
using PizzastaAdminBackend.Models;
using System.Diagnostics;

namespace PizzastaAdminBackend.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet(Name = "home")]
        public IActionResult Index()
        {
            return Json("App working " + DateTime.Now.ToLocalTime());
        }

        [HttpGet(Name = "privacy")]
        public IActionResult Privacy()
        {
            return Json("App working" + DateTime.Now.ToFileTimeUtc());
        }

        [HttpGet(Name = "error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return Json(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
