using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Diagnostics;

namespace Project_Quizz_Frontend.Controllers
{
    [Authorize]
    public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly QuizApiService _quizApiService;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, QuizApiService quizApiService, UserManager<IdentityUser> userManager)
		{
			_logger = logger;
			_quizApiService = quizApiService;
			_userManager = userManager;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		public async Task<IActionResult> Highscore()
		{
			var highscoreInformation = await _quizApiService.GetHighscoreData();
			if (highscoreInformation == null)
			{
                TempData["ErrorMessage"] = "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return View(new List<HighscoreDataDto>());
            }

			try
			{
                foreach (var user in highscoreInformation)
                {
                    var userObj = _userManager.Users.FirstOrDefault(x => x.Id == user.UserId);
                    user.UserId = await _userManager.GetUserNameAsync(userObj);
                }
            }
			catch (Exception ex)
			{
                TempData["ErrorMessage"] = "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return View(new List<HighscoreDataDto>());
            }

			return View(highscoreInformation);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}