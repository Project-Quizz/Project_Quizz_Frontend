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
    public class MyAreaController : Controller
	{
		private readonly QuizApiService _quizApiService;
		private readonly UserManager<IdentityUser> _userManager;

		public MyAreaController(QuizApiService quizApiService, UserManager<IdentityUser> userManager)
		{
			_quizApiService = quizApiService;
			_userManager = userManager;
		}

		[HttpGet]
		public async Task<IActionResult> CreateQuestion()
		{
			var model = new CreateQuizQuestionDto();
			
			// Get the categories from the cache
			var categories = CategorieCache.Categories;

			// If the cache is empty, get the categories from the API
			if (categories == null)
			{
				categories = await _quizApiService.GetAllCategoriesAsync();
				CategorieCache.Categories = categories;
			}

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

        public IActionResult MyAreaIndex()
        {
            return View();
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
            (GetQuestionForEditingDto question, List<GetQuizQuestionFeedbackDto> feedback) = await GetSelectedQuestion(questionId);
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

			var categories = CategorieCache.Categories;

			if (categories == null)
			{
				categories = await _quizApiService.GetAllCategoriesAsync();
				CategorieCache.Categories = categories;
			}

			ViewBag.Categories = categories ?? new List<CategorieIdDto>();
			ViewBag.Feedbacks = feedback;

			return View(question);
		}

        private async Task<(GetQuestionForEditingDto question, List<GetQuizQuestionFeedbackDto> feedbacks)> GetSelectedQuestion(int questionId)
        {
            var (question, statusCode) = await _quizApiService.GetQuestionForEditing(questionId);
            if (statusCode != HttpStatusCode.OK)
            {
                return (null, null);
            }

            var (feedbacks, statusCodeFeedback) = await _quizApiService.GetQuizQuestionFeedback(questionId);
            if (statusCodeFeedback != HttpStatusCode.OK)
            {
                return (question, null);
            }

			foreach (var feedback in feedbacks)
			{
                var user = await _userManager.FindByIdAsync(feedback.UserId);
                if (user != null)
                {
                    feedback.UserId = user.UserName;
                }
                else
                {
                    feedback.UserId = "Unbekannter Benutzer";
                }
            }

            return (question, feedbacks);
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

            // Get the categories from the cache
			var categories = CategorieCache.Categories;

			// If the cache is empty, get the categories from the API
			if (categories == null)
			{
				categories = await _quizApiService.GetAllCategoriesAsync();
				CategorieCache.Categories = categories;
			}

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

            var category = categories.Find(x => x.CategorieId == modifiedQuestion.Categorie.CategorieId);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return RedirectToAction("EditQuestion", new { questionId = questionIdNullable.Value });
			}

			modifiedQuestion.Categorie.Name = category.Name;

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
			if (correctAnswer.HasValue)
			{
				for (int i = 0; i < model.Answers.Count; i++)
				{
					model.Answers[i].IsCorrectAnswer = i == correctAnswer.Value;
				}
			}

			model.UserId = _userManager.GetUserId(User);

			var response = await _quizApiService.CreateQuestionAsync(model);

			if (response.IsSuccessStatusCode)
			{
				TempData["SuccessMessage"] = "Die Frage wurde erfoglreich erstellt!";
				return RedirectToAction("CreateQuestion");
			}
			else
			{
				TempData["ErrorMessage"] = "Leider gab es ein Problem beim erstellen deiner Frage!";
				return RedirectToAction("CreateQuestion");
			}
		}
	}
}