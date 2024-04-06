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
				return View(new List<GetAllQuestionsFromUserDto>());
			}

            return View(questions);
		}

		public async Task<IActionResult> EditQuestion(int questionId)
		{
			GetQuestionForEditingDto question = await GetSelectedQuestion(questionId);
			if (question == null)
			{
                TempData["ErrorMessage"] = "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return RedirectToAction("MyQuestions");
            }

            var userId = _userManager.GetUserId(User);
			if(question.UserId != userId)
			{
				return RedirectToAction("MyQuestions");
			}

			HttpContext.Session.SetInt32("QuestionId", questionId);

            var categories = await _quizApiService.GetAllCategoriesAsync();
			ViewBag.Categories = categories ?? new List<CategorieIdDto>();

			return View(question);
		}

		private async Task<GetQuestionForEditingDto> GetSelectedQuestion(int questionId)
		{
			var (question, statusCode) = await _quizApiService.GetQuestionForEditing(questionId);

            if (statusCode != HttpStatusCode.OK)
            {
				return null;
            }

			return question;
		}

		public async Task<IActionResult> UpdateModifiedQuestion(GetQuestionForEditingDto modifiedQuestion, int isCorrectAnswerRadio)
		{
            int? questionIdNullable = HttpContext.Session.GetInt32("QuestionId");
            if (questionIdNullable.HasValue)
            {
                modifiedQuestion.Id = questionIdNullable.Value;
            }
            else
            {
                TempData["ErrorMessage"] = "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return RedirectToAction("MyQuestions");
            }

            var categories = await _quizApiService.GetAllCategoriesAsync();
			if(!categories.Any(x => x.CategorieId == modifiedQuestion.Categorie.CategorieId))
			{
                TempData["ErrorMessage"] = "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return RedirectToAction("EditQuestion", new { questionId = questionIdNullable.Value });
			}

            if (isCorrectAnswerRadio < 0 || isCorrectAnswerRadio >= modifiedQuestion.Answers.Count)
            {
                TempData["ErrorMessage"] = "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return RedirectToAction("EditQuestion", new { questionId = questionIdNullable.Value });
            }

			modifiedQuestion.Answers[isCorrectAnswerRadio].IsCorrectAnswer = true;

			modifiedQuestion.Categorie.Name = categories.FirstOrDefault(x => x.CategorieId == modifiedQuestion.Categorie.CategorieId).Name;

            var userId = _userManager.GetUserId(User);
			modifiedQuestion.UserId = userId;

            var response = await _quizApiService.UpdateQuestion(modifiedQuestion);

			if (response.StatusCode != HttpStatusCode.OK)
			{
				HttpContext.Session.Remove("QuestionId");
                TempData["ErrorMessage"] = "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return RedirectToAction("EditQuestion", new { questionId = questionIdNullable.Value });
			}

            TempData["UpdateComplete"] = "Vorgang erfolgreich abgeschlossen";
            return RedirectToAction("MyQuestions");
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