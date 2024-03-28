using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Project_Quizz_Frontend.Controllers
{
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

			//Set the first answer as the correct one
			if (model.Answers.Count > 0)
			{
				model.Answers[0].IsCorrectAnswer = true;
			}

			return View(model);

		}

		[HttpPost]
		public async Task<IActionResult> CreateQuizOnDB(QuizQuestionViewModel model, int? CorrectAnswer)
		{
			if (CorrectAnswer.HasValue)
			{
				for (int i = 0; i < model.Answers.Count; i++)
				{
					model.Answers[i].IsCorrectAnswer = i == CorrectAnswer.Value;
				}
			}

			model.UserId = _userManager.GetUserId(User);

			var response = await _quizApiService.CreateQuestionAsync(model);

			
			if (response.IsSuccessStatusCode)
			{
				// Handle success (e.g., redirect to a confirmation page)
				return RedirectToAction("Index", "Home");
			}
			else
			{
				return RedirectToAction("CreateQuiz");
			}
		}
	}
}