using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Project_Quizz_Frontend.Controllers
{
	/// <summary>
	/// MyAreaController is responsible for the user's area.
	/// </summary>
	[Authorize]
	public class MyAreaController : Controller
	{
		private readonly QuizApiService _quizApiService;
		private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Constructor of the MyAreaController.
		/// </summary>
		/// <param name="quizApiService"></param>
		/// <param name="userManager"></param>
		public MyAreaController(QuizApiService quizApiService, UserManager<IdentityUser> userManager)
		{
			_quizApiService = quizApiService;
			_userManager = userManager;
		}

        /// <summary>
        /// Returns the view for creating a question.
        /// </summary>
        /// <returns>Returns the view for creating a question.</returns>
        [HttpGet]
		public async Task<IActionResult> CreateQuestion()
		{
			var model = new CreateQuizQuestionDto();

			var categories = CategorieCache.Categories;

			if (categories == null)
			{
				categories = await _quizApiService.GetAllCategoriesAsync();
				CategorieCache.Categories = categories;
			}

			ViewBag.Categories = categories ?? new List<CategorieIdDto>();

			model = new CreateQuizQuestionDto
			{
				Answers = new List<AnswerViewModel>
				{
					new AnswerViewModel(), 
					new AnswerViewModel(),
					new AnswerViewModel(),
					new AnswerViewModel()
				}
			};

			if (model.Answers.Count > 0)
			{
				model.Answers[0].IsCorrectAnswer = true;
			}

			return View(model);
		}

        /// <summary>
        /// Returns the view for the user's area.
        /// </summary>
        /// <returns>Returns MyAreaIndex view</returns>
        public IActionResult MyAreaIndex()
		{
			return View();
		}

        /// <summary>
        /// Index for the user's questions.
        /// </summary>
        /// <returns>Return the view MyQuestions with all created questions</returns>
        public async Task<IActionResult> MyQuestions()
		{
			var userId = _userManager.GetUserId(User);

			var (questions, statusCode) = await _quizApiService.GetAllQuestionsFromUser(userId);

			if (statusCode != HttpStatusCode.OK)
			{
				TempData["ErrorMessage"] = "Es konnten keine Fragen geladen werden. Bitte versuche es später nochmal.";
				return View(new List<GetAllQuestionsFromUserDto>());
			}

			return View(questions);
		}

        /// <summary>
        /// Edit a question.
        /// </summary>
        /// <param name="questionId">The id of the question to edit</param>
        /// <returns>Return the EditQuestion view</returns>
        public async Task<IActionResult> EditQuestion(int questionId)
        {
            (GetQuestionForEditingDto question, List<GetQuizQuestionFeedbackDto> feedback) = await GetSelectedQuestion(questionId);

            if (question == null)
            {
                TempData["ErrorMessage"] =
                    "Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
                return RedirectToAction("MyQuestions");
            }

            var userId = _userManager.GetUserId(User);
            if (question.UserId != userId)
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

        /// <summary>
        /// Load the view for the user's progress.
        /// </summary>
        /// <returns>Return MyProgress view</returns>
        public async Task<IActionResult> MyProgress()
		{
			var userId = _userManager.GetUserId(User);
			var userInformation = await _quizApiService.GetQuizMatchOverviewFromUser(userId);
			if (userInformation == null)
			{
                return View(new QuizMatchOverviewUserDto
                {
                    UserId = userId
                });
            }

			if (userId != userInformation.UserId)
			{
				TempData["ErrorMessage"] = "Zugriff wurde verweigert";
				return View(new QuizMatchOverviewUserDto
				{
					UserId = userId
				});
			}

			return View(userInformation);
		}

        /// <summary>
        /// Get the selected Question.
        /// </summary>
        /// <param name="questionId">Question id</param>
        /// <returns>Return the question as GetQuestionForEditingDto and the feedback GetQuizQuestionFeedbackDto as list</returns>
        private async Task<(GetQuestionForEditingDto question, List<GetQuizQuestionFeedbackDto> feedbacks)>
			GetSelectedQuestion(int questionId)
		{
			var (question, statusCode) = await _quizApiService.GetQuestionForEditing(questionId);

            // Setzen Sie hier den IsMultipleChoice-Wert
            question.IsMultipleChoice = question.Answers.Count(a => a.IsCorrectAnswer) > 1;


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

        /// <summary>
        /// Update a question that was modified.
        /// </summary>
        /// <param name="modifiedQuestion">GetQuestionForEditingDto</param>
        /// <param name="correctAnswer">The correct answers</param>
        /// <param name="isMultipleChoice">Bool</param>
        /// <returns>Return RedirectToAction</returns>
        public async Task<IActionResult> UpdateModifiedQuestion(GetQuestionForEditingDto modifiedQuestion, 
            List<int> correctAnswer, bool isMultipleChoice)
		{
			int? questionIdNullable = HttpContext.Session.GetInt32("QuestionId");
			if (questionIdNullable.HasValue)
			{
				modifiedQuestion.Id = questionIdNullable.Value;
			}
			else
			{
				TempData["ErrorMessage"] =
					"Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
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

			if (!categories.Any(x => x.CategorieId == modifiedQuestion.Categorie.CategorieId))
			{
				TempData["ErrorMessage"] =
					"Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
				return RedirectToAction("EditQuestion", new { questionId = questionIdNullable.Value });
			}

            foreach (var index in correctAnswer)
            {
                if (index >= 0 && index < modifiedQuestion.Answers.Count)
                {
                    modifiedQuestion.Answers[index].IsCorrectAnswer = true;
                }
            }

            var category = categories.Find(x => x.CategorieId == modifiedQuestion.Categorie.CategorieId);
			if (category == null)
			{
				TempData["ErrorMessage"] =
					"Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
				return RedirectToAction("EditQuestion", new { questionId = questionIdNullable.Value });
			}

			modifiedQuestion.Categorie.Name = category.Name;
            modifiedQuestion.IsMultipleChoice = isMultipleChoice;

            var userId = _userManager.GetUserId(User);
			modifiedQuestion.UserId = userId;

			var response = await _quizApiService.UpdateQuestion(modifiedQuestion);

			if (response.StatusCode != HttpStatusCode.OK)
			{
				HttpContext.Session.Remove("QuestionId");
				TempData["ErrorMessage"] =
					"Leider ist ein Fehler aufgetreten. Bitte versuchen Sie es nochmal oder kontaktieren den Support!";
				return RedirectToAction("EditQuestion", new { questionId = questionIdNullable.Value });
			}

			TempData["UpdateComplete"] = "Vorgang erfolgreich abgeschlossen";
			return RedirectToAction("MyQuestions");
		}

        /// <summary>
        /// Transfer created question to db
        /// </summary>
        /// <param name="model">CreateQuizQuestionDto</param>
        /// <param name="correctAnswer">List of correct answers</param>
        /// <returns>Return RedirectToAction</returns>
        [HttpPost]
		public async Task<IActionResult> CreateQuestionOnDB(CreateQuizQuestionDto model, List<int> correctAnswer)
		{
			if (model.IsMultipleChoice)
			{
				for (int i = 0; i < model.Answers.Count; i++)
				{
					model.Answers[i].IsCorrectAnswer = correctAnswer.Contains(i);
				}
			}
			else
			{
				if (correctAnswer.Any())
				{
					for (int i = 0; i < model.Answers.Count; i++)
					{
						model.Answers[i].IsCorrectAnswer = i == correctAnswer.First();
					}
				}
				else
				{
					TempData["ErrorMessage"] = "Bitte geben Sie mindestens eine korrekte Antwort an!";
					return RedirectToAction("CreateQuestion");
				}
			}

			model.UserId = _userManager.GetUserId(User);

			var response = await _quizApiService.CreateQuestionAsync(model);

			if (response.IsSuccessStatusCode)
			{
				TempData["SuccessMessage"] = "Die Frage wurde erfolgreich erstellt!";
				return RedirectToAction("CreateQuestion");
			}
			else
			{
				TempData["ErrorMessage"] = "Leider gab es ein Problem beim Erstellen Ihrer Frage!";
				return RedirectToAction("CreateQuestion");
			}
		}

	}
}