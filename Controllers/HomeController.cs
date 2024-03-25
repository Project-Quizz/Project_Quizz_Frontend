using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using System.Diagnostics;
using Project_Quizz_Frontend.Areas.Identity.Pages.Account;

namespace Project_Quizz_Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
