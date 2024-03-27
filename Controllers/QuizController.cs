using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Threading.Tasks;

namespace Project_Quizz_Frontend.Controllers
{
	public class QuizController : Controller
	{
		private readonly QuizApiService _quizApiService;

		public QuizController(QuizApiService quizApiService)
		{
			_quizApiService = quizApiService;
		}

		[HttpGet]
		public IActionResult CreateQuiz()
		{
			var model = new QuizQuestionViewModel
			{
				Answers = new List<AnswerViewModel>
				{
					new AnswerViewModel(), // Initialize with empty answers
					new AnswerViewModel(),
					new AnswerViewModel(),
					new AnswerViewModel()
				}
			};

			return View(model); }


		[HttpPost]
		public async Task<IActionResult> CreateQuiz(QuizQuestionViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var response = await _quizApiService.CreateQuestionAsync(model);

			if (response.IsSuccessStatusCode)
			{
				// Handle success (e.g., redirect to a confirmation page)
				return RedirectToAction("Index", "Home");
			}
			else
			{
				// Parse the response to display error details from the API
				ModelState.AddModelError(string.Empty, "API call failed.");
				return View(model);
			}
		}
	}
}