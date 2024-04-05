using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace Project_Quizz_Frontend.Controllers
{
    [Authorize]
    public class CreateQuestionController : Controller
	{
		private readonly QuizApiService _quizApiService;
		private readonly UserManager<IdentityUser> _userManager;

		public CreateQuestionController(QuizApiService quizApiService, UserManager<IdentityUser> userManager)
		{
			_quizApiService = quizApiService;
			_userManager = userManager;
		}

		[HttpGet]
		public async Task<IActionResult> CreateQuestion()
		{
			var model = new CreateQuizQuestionDto();
			var categories = await _quizApiService.GetAllCategoriesAsync();

			// Pass categories to the view through ViewBag or ViewData
			ViewBag.Categories = categories ?? new List<CategorieIdDto>();

			model = new CreateQuizQuestionDto
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

		public async Task<IActionResult> MyQuestions()
		{
            var userId = _userManager.GetUserId(User);

			var (questions, statusCode) = await _quizApiService.GetAllQuestionsFromUser(userId);

			if(statusCode != HttpStatusCode.OK)
			{
				TempData["ErrorMessage"] = "Es konnten keine Fragen geladen werden. Bitte versuche es später nochmal.";
				return View();
			}

            return View(questions);
		}

		public async Task<IActionResult> EditQuestion(int questionId)
		{
			GetQuestionForEditingDto question = await GetSelectedQuestion(questionId);
			if (question == null)
			{
                return View("MyQuestions");
            }

            var userId = _userManager.GetUserId(User);
			if(question.UserId != userId)
			{
				return View("MyQuestions");
			}

			var categories = await _quizApiService.GetAllCategoriesAsync();
			ViewBag.Categories = categories ?? new List<CategorieIdDto>();

			return View(question);
		}

		public async Task<GetQuestionForEditingDto> GetSelectedQuestion(int questionId)
		{
			var (question, statusCode) = await _quizApiService.GetQuestionForEditing(questionId);

            if (statusCode != HttpStatusCode.OK)
            {
				return null;
            }

			return question;
		}

		[HttpPost]
		public async Task<IActionResult> CreateQuestionOnDB(CreateQuizQuestionDto model, int? correctAnswer)
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
				TempData["SuccessMessage"] = "Die Frage wurde erfoglreich erstellt!";
				return RedirectToAction("CreateQuestion");
			}
			else
			{
				// Display an error message
				TempData["ErrorMessage"] = "Leider gab es ein Problem beim erstellen deiner Frage!";
				return RedirectToAction("CreateQuestion");
			}
		}
	}
}