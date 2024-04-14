using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Net;

namespace Project_Quizz_Frontend.Controllers
{
    /// <summary>
    /// Controller for Multiplayer-Quiz
    /// </summary>
    [Authorize]
    public class MultiplayerQuizController : Controller
    {
		private readonly MultiplayerApiService _multiQuizApiService;
        private readonly QuizApiService _quizApiService;
		private readonly UserManager<IdentityUser> _userManager;

        public MultiplayerQuizController(MultiplayerApiService multiQuizApiService, QuizApiService quizApiService, UserManager<IdentityUser> userManager)
        {
            _multiQuizApiService = multiQuizApiService;
            _quizApiService = quizApiService;
            _userManager = userManager;
        }

        /// <summary>
        /// Index for Multiplayer-Quiz
        /// </summary>
        /// <returns>Return the index for multiplayer</returns>
        public async Task<IActionResult> MultiplayerIndex()
        {
            var notificationCount = await LoadMultiplayerNotification();
            ViewBag.NotificationCount = notificationCount.ToString();
            return View();
        }

        /// <summary>
        /// Multiplayer-Quiz-Session
        /// </summary>
        /// <param name="newQuestion">New or next question as GetQuizQuestionDto</param>
        /// <returns>Return MultiQuizSession view</returns>
        public IActionResult MultiQuizSession(GetQuizQuestionDto newQuestion)
        {
            return View(newQuestion);
        }

        /// <summary>
        /// Multiplayer-Quiz-Answer-Result
        /// </summary>
        /// <param name="answerResult">The result of the quiz session from user as MultiQuizAnswerResultViewModel</param>
        /// <returns>Return MultiQuizAnswerResult view</returns>
        public IActionResult MultiQuizAnswerResult(MultiQuizAnswerResultViewModel answerResult)
        {
            return View(answerResult);
        }

        /// <summary>
        /// Multiplayer-Quiz-Complete-Result
        /// </summary>
        /// <returns>Return MultiQuizCompleteResult</returns>
        public IActionResult MultiQuizCompleteResult()
        {
            return View();
        }

        /// <summary>
        /// Multiplayer-Quiz overview of open challenges
        /// </summary>
        /// <returns>Return ChallengesOverview</returns>
        public async Task<IActionResult> ChallengesOverview()
        {
            var quizList = await LoadChallengesFromUser();
            if (quizList == null)
            {
                quizList = new List<GetMultiQuizzesFromUserDto>();
            } else
            {
                /// Get the opponent user name
                for (int i = quizList.Count - 1; i >= 0; i--)
                {
                    var quiz = quizList[i];
                    var opponend = _userManager.Users.FirstOrDefault(x => x.Id == quiz.OpponentUser);
                    if (opponend == null)
                    {
                        quizList.RemoveAt(i);
                    }
                    else
                    {
                        quiz.OpponentUser = await _userManager.GetUserNameAsync(opponend);
                    }
                }
            }
            return View(quizList);
        }

        /// <summary>
        /// Multiplayer-Quiz select opponent for new quiz
        /// </summary>
        /// <returns>Return SelectOpponent view</returns>
        public async Task<IActionResult> SelectOpponent()
        {
            var userName = _userManager.GetUserName(User);
            var users = await _userManager.Users
                .Where(u => u.UserName != userName)
                .Select(u => u.UserName)
                .ToListAsync();
            ViewBag.Users = users;
            return View();
        }

        /// <summary>
        /// Multiplayer-Quiz settings for new quiz
        /// </summary>
        /// <param name="selectedOpponent">The opponent that was selected in SelectOpponent view</param>
        /// <returns>Return MultiplayerSettings view</returns>
        public async Task<IActionResult> MultiplayerSettings(string selectedOpponent)
        {
            var users = await _userManager.Users.Select(u => u.UserName).ToListAsync();
            if (string.IsNullOrWhiteSpace(selectedOpponent) || !users.Any(u => u.Equals(selectedOpponent)))
            {
                TempData["ErrorMessage"] = "Der ausgewählte User ist leider nicht vorhanden!";
                return RedirectToAction("SelectOpponent");
            }

            /// Save the selected opponent in the session
            HttpContext.Session.SetString("SelectedOpponent", selectedOpponent);

            var categories = CategorieCache.Categories;

            if (categories == null)
            {
				categories = await _quizApiService.GetAllCategoriesAsync();
				CategorieCache.Categories = categories;
			}

			ViewBag.Categories = categories;
			return View();
        }

        /// <summary>
        /// Create a new multiplayer session
        /// </summary>
        /// <param name="categorieId">The categorie from the new quiz, that was selected in MultiplayerSettings view</param>
        /// <returns>Return RedirectToAction</returns>
        public async Task<IActionResult> CreateMultiplayerSession(int categorieId)
        {
            var userOne = _userManager.GetUserId(User);
            if(HttpContext.Session.GetString("SelectedOpponent") == null)
            {
                TempData["ErrorMessage"] = "Der ausgewählte User ist leider nicht vorhanden!";
                return RedirectToAction("SelectOpponent");
            }

			var userTwo = HttpContext.Session.GetString("SelectedOpponent");
			var userTwoId = await _userManager.FindByNameAsync(userTwo);

			if (userTwoId == null)
			{
				TempData["ErrorMessage"] = "Der ausgewählte User ist leider nicht vorhanden!";
				return RedirectToAction("SelectOpponent");
			}

			var response = await _multiQuizApiService.CreateMultiplayerQuizSession(userOne, userTwoId.Id, categorieId);

            if (!response.HttpResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Beim erstellen des Quiz ist leider ein Fehler aufgetaucht!";
                return RedirectToAction("SelectOpponent");
            }

            return RedirectToAction("GetQuestion", new { quizId = response.CreatedQuizSessionId });
        }

        /// <summary>
        /// Get the next question for the quiz
        /// </summary>
        /// <param name="quizId">Id of the quiz for the next question</param>
        /// <returns>Return MultiQuizSession view with actual question</returns>
        public async Task<IActionResult> GetQuestion(int quizId)
        {
            var userId = _userManager.GetUserId(User);

            var (quizQuestion, statusCode) = await _multiQuizApiService.GetQuestionForMultiQuiz(quizId, userId);

            /// Check if the response is OK
            if (statusCode == HttpStatusCode.OK)
            {
	            quizQuestion.IsMultipleChoice = quizQuestion.Answers.Count(a => a.IsCorrectAnswer) > 1;

				if (HttpContext.Session.GetString("MultiQuizQuestion") != null)
                {
                    HttpContext.Session.Remove("MultiQuizQuestion");
                }

                HttpContext.Session.SetString("MultiQuizQuestion", JsonConvert.SerializeObject(quizQuestion));

                return View("MultiQuizSession", quizQuestion);
            }

            TempData["ErrorMessage"] = "Es gab ein Problem mit der Anfrage. Bitte versuchen Sie es erneut oder wenden Sie sich an den Support.";
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// If a quiz is completed, show the result
        /// </summary>
        /// <param name="quizId">Id of the quiz</param>
        /// <returns>Return MultiQuizCompleteResult view</returns>
        public async Task<IActionResult> QuizComplete(int quizId)
        {
            var userId = _userManager.GetUserId(User);
            var (quizResult, statusCode) = await _multiQuizApiService.GetResultFromMultiQuiz(quizId, userId);

            if(statusCode == HttpStatusCode.OK)
            {
                return View("MultiQuizCompleteResult", quizResult);
            }

            /// Remove the session
			HttpContext.Session.Remove("MultiQuizQuestion");

			TempData["ErrorMessage"] = "Es gab ein Problem mit der Anfrage. Bitte versuchen Sie es erneut oder wenden Sie sich an den Support.";
			return View("MultiQuizCompleteResult", quizResult);
		}

        /// <summary>
        /// Submit the answer for the question and check if the answer is correct or not and check if the quiz is completed
        /// </summary>
        /// <param name="selectedAnswerIds">The selected answers from user</param>
        /// <returns>Return the MultiQuizAnswerResult view</returns>
		[HttpPost]
        public async Task<IActionResult> SubmitAnswer(List<int> selectedAnswerIds)
        {
            var userId = _userManager.GetUserId(User);
            var quizQuestionJson = HttpContext.Session.GetString("MultiQuizQuestion");
            var quizQuestion = JsonConvert.DeserializeObject<GetQuizQuestionDto>(quizQuestionJson);
            var answers = quizQuestion.Answers.Where(x => selectedAnswerIds.Contains(x.Id)).ToList();

            // Setzen Sie das IsMultipleChoice-Feld basierend auf der Art der Frage
            quizQuestion.IsMultipleChoice = quizQuestion.Answers.Count(a => a.IsCorrectAnswer) > 1;

            // Erhalten Sie die Ids der korrekten Antworten
            var correctAnswerIds = quizQuestion.Answers.Where(a => a.IsCorrectAnswer).Select(a => a.Id).ToList();

            // Prüfe, ob alle korrekten Antworten ausgewählt wurden und keine zusätzlichen falschen Antworten ausgewählt sind
            bool allCorrectAnswersSelected = selectedAnswerIds.All(id => correctAnswerIds.Contains(id)) &&
                                             correctAnswerIds.All(id => selectedAnswerIds.Contains(id)) &&
                                             correctAnswerIds.Count == selectedAnswerIds.Count;

            var viewModel = new MultiQuizAnswerResultViewModel
            {
                QuizId = quizQuestion.QuizId,
                QuizQuestionDto = quizQuestion,
                GivenAnswerIds = new List<MultiQuizGivenAnswerIdsViewModel>(),
                QuestionCount = quizQuestion.QuestionCount - 1,
                IsMultipleChoice = quizQuestion.IsMultipleChoice,
                IsAnswerCorrect = allCorrectAnswersSelected,
            };

            // Füge die gegebenen Antworten zur ViewModel-Liste hinzu
            foreach (var answer in answers)
            {
                viewModel.GivenAnswerIds.Add(new MultiQuizGivenAnswerIdsViewModel
                {
                    // Setzen der IsCorrectAnswer-Eigenschaft basierend auf der Antwort
                    QuizQuestionAnswerId = answer.Id,
                    IsCorrectAnswer = answer.IsCorrectAnswer,
                });
            }
            if (selectedAnswerIds.IsNullOrEmpty())
            {
                TempData["ErrorMessageBadRequest"] = "Bitte wähle mindestens eine Antwort aus!";
                return View("MultiQuizSession", quizQuestion);
            }

            var updateMultiQuizSessionObj = new UpdateMultiQuizSessionDto
            {
                QuizId = quizQuestion.QuizId,
                QuestionId = quizQuestion.QuestionId,
				GivenAnswerIds = new List<MultiQuizGivenAnswerIdsDto>(),
				UserId = userId,
            };

            foreach (var answer in answers)
            {
                updateMultiQuizSessionObj.GivenAnswerIds.Add(new MultiQuizGivenAnswerIdsDto
                {
                    QuizQuestionAnswerId = answer.Id,
                });
            }

            var response = await _multiQuizApiService.UpdateMultiQuizSession(updateMultiQuizSessionObj);

			if (response.StatusCode == HttpStatusCode.BadRequest)
			{
				TempData["ErrorMessageBadRequest"] = "Es gab ein Problem mit der Anfrage. Bitte versuchen Sie es erneut oder wenden Sie sich an den Support.";
				return View("MultiQuizSession", quizQuestion);
			}

            /// Check if the given answer was also given 
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				TempData["ErrorMessageUnauthorized"] = "Eine bereits beantwortete Frage kann nicht nocheinmal Beantwortet werden. Um fortzufahren klicken Sie bitte auf \"Nächste Frage\"";
				return View("MultiQuizSession", quizQuestion);
			}

            foreach (var answer in answers)
            {
                viewModel.GivenAnswerIds.Add(new MultiQuizGivenAnswerIdsViewModel
                {
                    QuizQuestionAnswerId = answer.Id,
                    IsCorrectAnswer = answer.IsCorrectAnswer,
                });
            }

            /// Check if the quiz is completed
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                viewModel.QuizComplete = true;
				return View("MultiQuizAnswerResult", viewModel);
			}

            /// Check if the quiz is not completed
			if (response.StatusCode == HttpStatusCode.OK)
			{
				viewModel.QuizComplete = false;
				return View("MultiQuizAnswerResult", viewModel);
			}

			TempData["ErrorMessage"] = "Es ist ein Fehler aufgetreten, bitte versuche es nochmal.";
			return View("MultiQuizSession", quizQuestion);
		}

        /// <summary>
        /// Load all challenges (multiplayer quizzes) from logged user
        /// </summary>
        /// <returns>Return a List<GetMultiQuizzesFromUserDto></returns>
        private async Task<List<GetMultiQuizzesFromUserDto>> LoadChallengesFromUser()
        {
            var userId = _userManager.GetUserId(User);

            var (result, statusCode) = await _multiQuizApiService.GetMultiQuizzesFromUser(userId);

            if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Es ist ein Fehler aufgetreten, bitte versuche es nochmal.";
                return null;
            }

            return result;
        }

        /// <summary>
        /// Load all multiplayer notifications for the logged user
        /// </summary>
        /// <returns>Return the open multiplayer games as int to show as notification</returns>
        private async Task<int> LoadMultiplayerNotification()
        {
            var userId = _userManager.GetUserId(User);

            var (result, statusCode) = await _multiQuizApiService.GetMultiplayerNotificationsFromUser(userId);

            if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Es konnten keine Notifications geladen werden. Bitte wende dich an den Support!";
                return result;
            }

            return result;
        }
    }
}
