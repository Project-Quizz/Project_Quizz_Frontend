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
    /// Is responsible for the multiplayer quiz
    /// </summary>
    [Authorize]
    public class MultiplayerQuizController : Controller
    {
		private readonly MultiplayerApiService _multiQuizApiService;
        private readonly QuizApiService _quizApiService;
		private readonly UserManager<IdentityUser> _userManager;

        /// <summary>
        /// Constructor for MultiplayerQuizController
        /// </summary>
        /// <param name="multiQuizApiService"></param>
        /// <param name="quizApiService"></param>
        /// <param name="userManager"></param>
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
        /// <param name="newQuestion">The new question for the quiz</param>
        /// <returns>Multiplayer Session view</returns>
        public IActionResult MultiQuizSession(GetQuizQuestionDto newQuestion)
        {
            return View(newQuestion);
        }

        /// <summary>
        /// Multiplayer-Quiz-Answer-Result
        /// </summary>
        /// <param name="answerResult">The result of the interaction</param>
        /// <returns>Return the MultiQuizAnswerResult view</returns>
        public IActionResult MultiQuizAnswerResult(MultiQuizAnswerResultViewModel answerResult)
        {
            return View(answerResult);
        }

        /// <summary>
        /// Multiplayer-Quiz-Complete-Result
        /// </summary>
        /// <returns>If Quiz complete return the MultiQuizCompleteResult</returns>
        public IActionResult MultiQuizCompleteResult()
        {
            return View();
        }

        /// <summary>
        /// Challenges Overview
        /// </summary>
        /// <returns>Return the ChallengesOverview view</returns>
        public async Task<IActionResult> ChallengesOverview()
        {
            /// Load the challenges from the user
            var quizList = await LoadChallengesFromUser();
            if (quizList == null)
            {
                quizList = new List<GetMultiQuizzesFromUserDto>();
            } else
            {
                /// Load the user names for the opponent
                for (int i = quizList.Count - 1; i >= 0; i--)
                {
                    var quiz = quizList[i];
                    var opponend = _userManager.Users.FirstOrDefault(x => x.Id == quiz.OpponentUser);
                    /// Remove the challenge if the opponent is not available
                    if (opponend == null)
                    {
                        quizList.RemoveAt(i);
                    }
                    /// Set the user name for the opponent
                    else
                    {
                        quiz.OpponentUser = await _userManager.GetUserNameAsync(opponend);
                    }
                }
            }
            return View(quizList);
        }

        /// <summary>
        /// Select the opponent for the multiplayer quiz
        /// </summary>
        /// <returns>Return the SelectOpponent view</returns>
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
        /// Multiplayer-Settings
        /// </summary>
        /// <param name="selectedOpponent">The selected opponent from the SelectOpponent view</param>
        /// <returns>Return the MultiplayerSettings view</returns>
        public async Task<IActionResult> MultiplayerSettings(string selectedOpponent)
        {
            /// Check if the selected opponent is available
            var users = await _userManager.Users.Select(u => u.UserName).ToListAsync();
            if (string.IsNullOrWhiteSpace(selectedOpponent) || !users.Any(u => u.Equals(selectedOpponent)))
            {
                TempData["ErrorMessage"] = "Der ausgewählte User ist leider nicht vorhanden!";
                return RedirectToAction("SelectOpponent");
            }
            
            /// Set the selected opponent in the session
            HttpContext.Session.SetString("SelectedOpponent", selectedOpponent);

            /// Load the categories for the quiz
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

            /// Check if the selected opponent is available
			if (userTwoId == null)
			{
				TempData["ErrorMessage"] = "Der ausgewählte User ist leider nicht vorhanden!";
				return RedirectToAction("SelectOpponent");
			}

			var response = await _multiQuizApiService.CreateMultiplayerQuizSession(userOne, userTwoId.Id, categorieId);

            /// Check if the quiz was created successfully
            if (!response.HttpResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Beim erstellen des Quiz ist leider ein Fehler aufgetaucht!";
                return RedirectToAction("SelectOpponent");
            }

            /// Redirect to the first question
            return RedirectToAction("GetQuestion", new { quizId = response.CreatedQuizSessionId });
        }

        /// <summary>
        /// Get the next question for the multiplayer quiz
        /// </summary>
        /// <param name="quizId">Quiz id from where the question is</param>
        /// <returns>Return RedirectToAction</returns>
        public async Task<IActionResult> GetQuestion(int quizId)
        {
            var userId = _userManager.GetUserId(User);

            var (quizQuestion, statusCode) = await _multiQuizApiService.GetQuestionForMultiQuiz(quizId, userId);


            /// Check if the question was loaded successfully
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
        /// If quiz complete return the MultiQuizCompleteResult 
        /// </summary>
        /// <param name="quizId">The quiz id</param>
        /// <returns>Return View</returns>
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
        /// Submit the answer for the multiplayer quiz
        /// </summary>
        /// <param name="selectedAnswerIds">The selected answer ids as list</param>
        /// <returns>Return View</returns>
		[HttpPost]
        public async Task<IActionResult> SubmitAnswer(List<int> selectedAnswerIds)
        {
            var userId = _userManager.GetUserId(User);
            var quizQuestionJson = HttpContext.Session.GetString("MultiQuizQuestion");
            var quizQuestion = JsonConvert.DeserializeObject<GetQuizQuestionDto>(quizQuestionJson);
            var answers = quizQuestion.Answers.Where(x => selectedAnswerIds.Contains(x.Id)).ToList();

            quizQuestion.IsMultipleChoice = quizQuestion.Answers.Count(a => a.IsCorrectAnswer) > 1;

            var correctAnswerIds = quizQuestion.Answers.Where(a => a.IsCorrectAnswer).Select(a => a.Id).ToList();

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

            /// Add the given answers to the view model
            foreach (var answer in answers)
            {
                viewModel.GivenAnswerIds.Add(new MultiQuizGivenAnswerIdsViewModel
                {
                    QuizQuestionAnswerId = answer.Id,
                    IsCorrectAnswer = answer.IsCorrectAnswer,
                });
            }
            /// Check if the answer is correct
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

            /// Add the given answers to the update object
            foreach (var answer in answers)
            {
                updateMultiQuizSessionObj.GivenAnswerIds.Add(new MultiQuizGivenAnswerIdsDto
                {
                    QuizQuestionAnswerId = answer.Id,
                });
            }

            /// Update the quiz session
            var response = await _multiQuizApiService.UpdateMultiQuizSession(updateMultiQuizSessionObj);

            /// Check if the answer was submitted successfully
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

            /// Check if the quiz is complete
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
        /// Load the challenges from the user
        /// </summary>
        /// <returns>Return the result GetMultiQuizzesFromUserDto as list</returns>
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

        /// Load the multiplayer notification
        /// </summary>
        /// <returns>Return the result as int</returns>
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
