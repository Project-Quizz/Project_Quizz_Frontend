using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Net;


namespace Project_Quizz_Frontend.Controllers
{
    /// <summary>
    /// SingleplayerQuizController is responsible for managing singleplayer quiz sessions and settings.
    /// </summary>
    [Authorize]
	public class SingleplayerQuizController : Controller
	{
		private readonly QuizApiService _quizApiService;
        private readonly SingleplayerApiService _singleplayerApiService;
        private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Constructor for SingleplayerQuizController
		/// </summary>
		/// <param name="quizApiService"></param>
		/// <param name="singleplayerApiService"></param>
		/// <param name="userManager"></param>
		public SingleplayerQuizController(QuizApiService quizApiService,SingleplayerApiService singleplayerApiService, UserManager<IdentityUser> userManager)
		{
			_quizApiService = quizApiService;
			_singleplayerApiService = singleplayerApiService;
			_userManager = userManager;
		}

        /// <summary>
        /// Index method for SingleplayerQuizController
        /// </summary>
        /// <returns>Return SingleplayerIndex view</returns>
        public async  Task<IActionResult> SingleplayerIndex()
		{
            var notificationCount = await LoadSingleplayerNotification();
            ViewBag.NotificationCount = notificationCount.ToString();
            return View();
		}

        /// <summary>
        /// For displaying the single quiz session
        /// </summary>
        /// <param name="newQuizQuestion">The new quiz question as GetQuizQuestionDto</param>
        /// <returns>Return the SingleQuizSession view</returns>
        public IActionResult SingleQuizSession(GetQuizQuestionDto newQuizQuestion)
		{
			return View(newQuizQuestion);
		}

        /// <summary>
        /// For displaying the single quiz answer result
        /// </summary>
        /// <param name="answerResult">The answer reuslt as SoloQuizAnswerResultViewModel</param>
        /// <returns>Return the SingleQuizAnswerResult view</returns>
        public IActionResult SingleQuizAnswerResult(SoloQuizAnswerResultViewModel answerResult)
		{
			return View(answerResult);
		}

        /// <summary>
        /// For displaying the single quiz complete result
        /// </summary>
        /// <param name="result">The result of a singleplayer as GetResultFromSingleQuizDto</param>
        /// <returns>Return the SingleQuizCompleteResult view</returns>
		public IActionResult SingleQuizCompleteResult(GetResultFromSingleQuizDto result)
		{
			return View(result);
		}

        /// <summary>
        /// For displaying the singleplayer settings
        /// </summary>
        /// <returns>Return the SingleplayerSettings view</returns>
		public async Task<IActionResult>  SingleplayerSettings()
		{
			var categories = await LoadCategories();
            ViewBag.Categories = categories;
            return View();
		}

        /// <summary>
        /// For displaying the overview of open single quizzes
        /// </summary>
        /// <returns>Return the OverviewOfOpenSingleQuizzes view</returns>
		public async Task<IActionResult> OverviewOfOpenSingleQuizzes()
		{
			var quizList = await LoadSingleQuizzesFromUser();
			if (quizList == null)
			{
				quizList = new List<GetSingleQuizzesFromUserDto>();
			}
			return View(quizList);
		}

        /// <summary>
        /// TO create a new single quiz session
        /// </summary>
        /// <param name="categorieId">The categorie id for the new quiz</param>
        /// <returns>Return RedirectToAction</returns>
        public async Task<IActionResult> CreateNewSingleQuizSession(int categorieId)
		{
			var userId = _userManager.GetUserId(User);
			var quizSession = await _singleplayerApiService.CreateSingleQuizSession(userId, categorieId);

			var newQuizSessionId = quizSession.CreatedQuizSessionId;

			if (!quizSession.HttpResponse.IsSuccessStatusCode)
			{
				return RedirectToAction("Error", "Home");
			}

			return RedirectToAction("GetQuestion", new { quizId = newQuizSessionId});
		}

        /// <summary>
        /// Get the question for a single quiz
        /// </summary>
        /// <param name="quizId">Quiz id from where the question comes</param>
        /// <returns>Return RedirectToAction</returns>
		public async Task<IActionResult> GetQuestion(int quizId)
		{
			var userId = _userManager.GetUserId(User);

			var (quizQuestion, statusCode) = await _singleplayerApiService.GetQuestionForSingleQuiz(quizId, userId);

			if (statusCode == HttpStatusCode.OK)
			{
                quizQuestion.IsMultipleChoice = quizQuestion.Answers.Count(a => a.IsCorrectAnswer) > 1;

                if (HttpContext.Session.GetString("QuizQuestion") != null)
                {
                    HttpContext.Session.Remove("QuizQuestion");
                }

                HttpContext.Session.SetString("QuizQuestion", JsonConvert.SerializeObject(quizQuestion));

                return View("SingleQuizSession", quizQuestion);
            }

			TempData["ErrorMessage"] = "Es gab ein Problem mit der Anfrage. Bitte versuchen Sie es erneut oder wenden Sie sich an den Support.";
			return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Submit the answer for a single quiz
        /// </summary>
        /// <param name="selectedAnswerIds">The selected answers as int</param>
        /// <returns>Return a view depending on the result</returns>
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(List<int> selectedAnswerIds)
        {
            var userId = _userManager.GetUserId(User);
            var quizQuestionJson = HttpContext.Session.GetString("QuizQuestion");
            var quizQuestion = JsonConvert.DeserializeObject<GetQuizQuestionDto>(quizQuestionJson);
            var answers = quizQuestion.Answers.Where(x => selectedAnswerIds.Contains(x.Id)).ToList();

            quizQuestion.IsMultipleChoice = quizQuestion.Answers.Count(a => a.IsCorrectAnswer) > 1;

            var correctAnswerIds = quizQuestion.Answers.Where(a => a.IsCorrectAnswer).Select(a => a.Id).ToList();

            /// Check that all correct answers have been selected
            bool allCorrectAnswersSelected = selectedAnswerIds.All(id => correctAnswerIds.Contains(id)) &&
                                             correctAnswerIds.All(id => selectedAnswerIds.Contains(id)) &&
                                             correctAnswerIds.Count == selectedAnswerIds.Count;

            var viewModel = new SoloQuizAnswerResultViewModel
            {
                QuizId = quizQuestion.QuizId,
                QuizQuestionDto = quizQuestion,
                GivenAnswerIds = new List<SingleQuizGivenAnswerIdsViewModel>(),
                QuestionCount = quizQuestion.QuestionCount - 1,
                IsMultipleChoice = quizQuestion.IsMultipleChoice,
                IsAnswerCorrect = allCorrectAnswersSelected,
            };

            /// Add the selected answers to the view model
            foreach (var answer in answers)
            {
                viewModel.GivenAnswerIds.Add(new SingleQuizGivenAnswerIdsViewModel
                {
                    QuizQuestionAnswerId = answer.Id,
                    IsCorrectAnswer = answer.IsCorrectAnswer,
                });
            }

            /// If no answer was selected
            if (selectedAnswerIds.IsNullOrEmpty())
            {
                TempData["ErrorMessageBadRequest"] = "Bitte wähle mindestens eine Antwort aus!";
                return View("SingleQuizSession", quizQuestion);
            }

            var updateSingleQuizSessionObj = new UpdateSingleQuizSessionDto
            {
                QuizId = quizQuestion.QuizId,
                QuestionId = quizQuestion.QuestionId,
                GivenAnswerIds = new List<SingleQuizGivenAnswerIdsDto>(),
                UserId = userId,
            };

            /// Add the selected answers to the update object
            foreach (var answer in answers)
            {
                updateSingleQuizSessionObj.GivenAnswerIds.Add(new SingleQuizGivenAnswerIdsDto
                {
                    QuizQuestionAnswerId = answer.Id,
                });
            }

            var response = await _singleplayerApiService.UpdateSingleQuizSession(updateSingleQuizSessionObj);

			if(response.StatusCode == HttpStatusCode.BadRequest)
			{
				TempData["ErrorMessageBadRequest"] = "Es gab ein Problem mit der Anfrage. Bitte versuchen Sie es erneut oder wenden Sie sich an den Support.";
                return View("SingleQuizSession", quizQuestion);
			}

			if(response.StatusCode == HttpStatusCode.Unauthorized)
			{
				TempData["ErrorMessageUnauthorized"] = "Eine bereits beantwortete Frage kann nicht nocheinmal Beantwortet werden. Um fortzufahren klicken Sie bitte auf \"Nächste Frage\"";
                return View("SingleQuizSession", quizQuestion);
			}

			foreach (var answer in answers)
			{
				viewModel.GivenAnswerIds.Add(new SingleQuizGivenAnswerIdsViewModel
				{
					QuizQuestionAnswerId = answer.Id,
					IsCorrectAnswer = answer.IsCorrectAnswer,
				});
			}

			if (response.StatusCode == HttpStatusCode.Accepted)
			{
				viewModel.QuizComplete = true;
                return View("SingleQuizAnswerResult", viewModel);
			}

			if(response.StatusCode == HttpStatusCode.OK)
			{
				viewModel.QuizComplete = false;
                return View("SingleQuizAnswerResult", viewModel);
			}

			TempData["ErrorMessage"] = "Es ist ein Fehler aufgetreten, bitte versuche es nochmal.";
            return View("SingleQuizSession", quizQuestion);

		}

        /// <summary>
        /// To complete a single quiz
        /// </summary>
        /// <param name="quizId">The quiz id to be completed</param>
        /// <returns>Return SingleQuizCompleteResult view</returns>
		public async Task<IActionResult> QuizComplete(int quizId)
		{
			var userId = _userManager.GetUserId(User);
			var (quizResult, statusCode) = await _singleplayerApiService.GetResultFromSingleQuiz(quizId, userId);

			if(statusCode == HttpStatusCode.OK)
			{
				return View("SingleQuizCompleteResult", quizResult);
			}

			HttpContext.Session.Remove("QuizQuestion");

			TempData["ErrorMessage"] = "Es gab ein Problem mit der Anfrage. Bitte versuchen Sie es erneut oder wenden Sie sich an den Support.";
			return View("SingleQuizCompleteResult", quizResult);
		}

        /// <summary>
        /// To load the categories
        /// </summary>
        /// <returns>Return the loaded CategorieIdDto</returns>
		private async Task<List<CategorieIdDto>> LoadCategories()
		{
			var categories = CategorieCache.Categories;

			if (categories == null)
			{
				categories = await _quizApiService.GetAllCategoriesAsync();
				CategorieCache.Categories = categories;
			}
			return categories;
		}

        /// <summary>
        /// To load the singleplayer notification
        /// </summary>
        /// <returns>Return the result as int</returns>
        private async Task<int> LoadSingleplayerNotification()
        {
            var userId = _userManager.GetUserId(User);

            var (result, statusCode) = await _singleplayerApiService.GetSingleplayerNotificationsFromUser(userId);

            if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Es konnten keine Notifications geladen werden. Bitte wende dich an den Support!";
                return result;
            }

            return result;
        }

        /// <summary>
        /// To load the single quizzes from user
        /// </summary>
        /// <returns>Return the result GetSingleQuizzesFromUserDto as list</returns>
		private async Task<List<GetSingleQuizzesFromUserDto>> LoadSingleQuizzesFromUser()
		{
            var userId = _userManager.GetUserId(User);

            var (result, statusCode) = await _singleplayerApiService.GetSingleQuizzesFromUser(userId);

            if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Es ist ein Fehler aufgetreten, bitte versuche es nochmal.";
                return null;
            }

            return result;
        }
    }
}