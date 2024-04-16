using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Diagnostics;

namespace Project_Quizz_Frontend.Controllers
{
	/// <summary>
	/// Is responsible for the Home view.
	/// </summary>
    public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly QuizApiService _quizApiService;
        private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="HomeController"/> class.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="quizApiService"></param>
		/// <param name="userManager"></param>
        public HomeController(ILogger<HomeController> logger, QuizApiService quizApiService, UserManager<IdentityUser> userManager)
		{
			_logger = logger;
			_quizApiService = quizApiService;
			_userManager = userManager;
		}

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        [Authorize]
        public IActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Impressums this instance.
		/// </summary>
		/// <returns>Return Impressum view</returns>
		public IActionResult Impressum()
		{
            return View();
        }
		/// <summary>
		/// Privacies this instance.
		/// </summary>
		/// <returns>Return Privacy view</returns>
		public IActionResult Privacy()
		{
			return View();
		}

        /// <summary>
        /// Highscores this instance.
        /// </summary>
        /// <returns>Return Highscore view</returns>
        [Authorize]
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
                for (int i = highscoreInformation.Count - 1; i >= 0; i--)
                {
                    var user = highscoreInformation[i];
                    var userObj = _userManager.Users.FirstOrDefault(x => x.Id == user.UserId);
                    if (userObj == null)
                    {
                        highscoreInformation.RemoveAt(i);
                    }
                    else
                    {
                        user.UserId = await _userManager.GetUserNameAsync(userObj);
                    }
                }
            }
            catch (Exception ex)
			{
                TempData["ErrorMessage"] = "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return View(new List<HighscoreDataDto>());
            }

			var actualUser = _userManager.GetUserName(User);
			ViewBag.UserName = actualUser;

			return View(highscoreInformation);
		}

		/// <summary>
		/// Errors this instance.
		/// </summary>
		/// <returns>Return Error view</returns>
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}