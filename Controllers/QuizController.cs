using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Project_Quizz_Frontend.Controllers
{
    [Authorize]
    public class QuizController : Controller
	{
		private readonly QuizApiService _quizApiService;
		private readonly UserManager<IdentityUser> _userManager;

		public QuizController(QuizApiService quizApiService, UserManager<IdentityUser> userManager)
		{
			_quizApiService = quizApiService;
			_userManager = userManager;
		}

		[HttpGet]
		public async Task<IActionResult> CreateQuiz()
		{
			var categories = await _quizApiService.GetAllCategoriesAsync();

			// Pass categories to the view through ViewBag or ViewData
			ViewBag.Categories = categories ?? new List<CategorieIdDto>();

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

			//Set the first answer as the correct one
			if (model.Answers.Count > 0)
			{
				model.Answers[0].IsCorrectAnswer = true;
			}

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> CreateQuizOnDB(QuizQuestionViewModel model, int? correctAnswer)
		{
			// Set the correct answer based on the selected index
			if (correctAnswer.HasValue)
			{
				for (int i = 0; i < model.Answers.Count; i++)
				{
					model.Answers[i].IsCorrectAnswer = i == correctAnswer.Value;
				}
			}

			// Set the user ID from the current user
			model.UserId = _userManager.GetUserId(User);

			// Call the API service to create the question
			var response = await _quizApiService.CreateQuestionAsync(model);

			// Check if the request was successful
			if (response.IsSuccessStatusCode)
			{
				// Display a success message
				TempData["SuccessMessage"] = "Your question has been successfully submitted!";
				return RedirectToAction("CreateQuiz");
			}
			else
			{
				// Display an error message
				TempData["ErrorMessage"] = "There was an error submitting your question. Please try again!";
				return RedirectToAction("CreateQuiz");
			}
		}
	}
}