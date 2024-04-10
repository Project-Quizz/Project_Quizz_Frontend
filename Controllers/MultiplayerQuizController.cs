using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Newtonsoft.Json;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Net;

namespace Project_Quizz_Frontend.Controllers
{
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

        public async Task<IActionResult> MultiplayerIndex()
        {
            var notificationCount = await LoadMultiplayerNotification();
            ViewBag.NotificationCount = notificationCount.ToString();
            return View();
        }

        public IActionResult MultiQuizSession(GetQuizQuestionDto newQuestion)
        {
            return View(newQuestion);
        }

        public IActionResult MultiQuizAnswerResult(MultiQuizAnswerResultViewModel answerResult)
        {
            return View(answerResult);
        }

        public IActionResult MultiQuizCompleteResult()
        {
            return View();
        }

        public async Task<IActionResult> ChallengesOverview()
        {
            var quizList = await LoadChallengesFromUser();
            if (quizList == null)
            {
                quizList = new List<GetMultiQuizzesFromUserDto>();
            } else
            {
                foreach (var quiz in quizList)
                {
                    var opponend = _userManager.Users.FirstOrDefault(x => x.Id == quiz.OpponentUser);
                    if (opponend == null)
                    {
                        quiz.OpponentUser = "Anonym";
                    }
                    quiz.OpponentUser = await _userManager.GetUserNameAsync(opponend);
                }
            }
            return View(quizList);
        }

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

        public async Task<IActionResult> MultiplayerSettings(string selectedOpponent)
        {
            var users = await _userManager.Users.Select(u => u.UserName).ToListAsync();
            if (string.IsNullOrWhiteSpace(selectedOpponent) || !users.Any(u => u.Equals(selectedOpponent)))
            {
                TempData["ErrorMessage"] = "Der ausgewählte User ist leider nicht vorhanden!";
                return RedirectToAction("SelectOpponent");
            }

            HttpContext.Session.SetString("SelectedOpponent", selectedOpponent);

            // Get the categories from the cache
            var categories = CategorieCache.Categories;

            //If the cache is empty, get the categories from the API
            if (categories == null)
            {
				categories = await _quizApiService.GetAllCategoriesAsync();
				CategorieCache.Categories = categories;
			}

			ViewBag.Categories = categories;
			return View();
        }

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

        public async Task<IActionResult> GetQuestion(int quizId)
        {
            var userId = _userManager.GetUserId(User);

            var (quizQuestion, statusCode) = await _multiQuizApiService.GetQuestionForMultiQuiz(quizId, userId);

            if (statusCode == HttpStatusCode.OK)
            {
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

        public async Task<IActionResult> QuizComplete(int quizId)
        {
            var userId = _userManager.GetUserId(User);
            var (quizResult, statusCode) = await _multiQuizApiService.GetResultFromMultiQuiz(quizId, userId);

            if(statusCode == HttpStatusCode.OK)
            {
                return View("MultiQuizCompleteResult", quizResult);
            }

			HttpContext.Session.Remove("MultiQuizQuestion");

			TempData["ErrorMessage"] = "Es gab ein Problem mit der Anfrage. Bitte versuchen Sie es erneut oder wenden Sie sich an den Support.";
			return View("MultiQuizCompleteResult", quizResult);
		}

		[HttpPost]
        public async Task<IActionResult> SubmitAnswer(List<int> selectedAnswerIds)
        {
            var userId = _userManager.GetUserId(User);
            var quizQuestionJson = HttpContext.Session.GetString("MultiQuizQuestion");
            var quizQuestion = JsonConvert.DeserializeObject<GetQuizQuestionDto>(quizQuestionJson);
            var answers = quizQuestion.Answers.Where(x => selectedAnswerIds.Contains(x.Id)).ToList();

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

			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				TempData["ErrorMessageUnauthorized"] = "Eine bereits beantwortete Frage kann nicht nocheinmal Beantwortet werden. Um fortzufahren klicken Sie bitte auf \"Nächste Frage\"";
				return View("MultiQuizSession", quizQuestion);
			}

            var viewModel = new MultiQuizAnswerResultViewModel
            {
                QuizId = quizQuestion.QuizId,
                QuizQuestionDto = quizQuestion,
                GivenAnswerIds = new List<MultiQuizGivenAnswerIdsViewModel>(),
				QuestionCount = quizQuestion.QuestionCount - 1
            };

            foreach(var answer in answers)
            {
                viewModel.GivenAnswerIds.Add(new MultiQuizGivenAnswerIdsViewModel
                {
                    QuizQuestionAnswerId = answer.Id,
                    IsCorrectAnswer = answer.IsCorrectAnswer,
                });
            }

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                viewModel.QuizComplete = true;
				return View("MultiQuizAnswerResult", viewModel);
			}

			if (response.StatusCode == HttpStatusCode.OK)
			{
				viewModel.QuizComplete = false;
				return View("MultiQuizAnswerResult", viewModel);
			}

			TempData["ErrorMessage"] = "Es ist ein Fehler aufgetreten, bitte versuche es nochmal.";
			return View("MultiQuizSession", quizQuestion);
		}

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
