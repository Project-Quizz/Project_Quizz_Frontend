using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Diagnostics;

namespace Project_Quizz_Frontend.Controllers
{
	/// <summary>
	/// Home Controller
	/// </summary>
    [Authorize]
    public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly QuizApiService _quizApiService;
        private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="quizApiService">API Injection</param>
		/// <param name="userManager">Identity injection for user management</param>
        public HomeController(ILogger<HomeController> logger, QuizApiService quizApiService, UserManager<IdentityUser> userManager)
		{
			_logger = logger;
			_quizApiService = quizApiService;
			_userManager = userManager;
		}

		/// <summary>
		/// Index
		/// </summary>
		/// <returns>Index view</returns>
		public IActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Privacy
		/// </summary>
		/// <returns>Return private view</returns>
		public IActionResult Privacy()
		{
			return View();
		}

		/// <summary>
		/// Highscore
		/// </summary>
		/// <returns>Return Highscore View</returns>
		public async Task<IActionResult> Highscore()
		{
			var highscoreInformation = await _quizApiService.GetHighscoreData();
			if (highscoreInformation == null)
			{
                TempData["ErrorMessage"] = "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return View(new List<HighscoreDataDto>());
            }

			/// Remove all users that are not in the database anymore
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

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}