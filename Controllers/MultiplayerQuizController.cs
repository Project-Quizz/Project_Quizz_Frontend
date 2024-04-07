using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Project_Quizz_API.Data;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Net;

namespace Project_Quizz_Frontend.Controllers
{
    public class MultiplayerQuizController : Controller
    {
		private readonly QuizApiService _quizApiService;
		private readonly UserManager<IdentityUser> _userManager;

        public MultiplayerQuizController(QuizApiService quizApiService, UserManager<IdentityUser> userManager)
        {
            _quizApiService = quizApiService;
            _userManager = userManager;
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
            foreach (var quiz in quizList)
            {
                var opponend = _userManager.Users.FirstOrDefault(x => x.Id == quiz.OpponentUser);
                quiz.OpponentUser = await _userManager.GetUserNameAsync(opponend);
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

            var categories = await _quizApiService.GetAllCategoriesAsync();
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

			var response = await _quizApiService.CreateMultiplayerQuizSession(userOne, userTwoId.Id, categorieId);

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
            var response = await _quizApiService.GetQuestionForMultiQuiz(quizId, userId);

            if (HttpContext.Session.GetString("MultiQuizQuestion") != null)
            {
                HttpContext.Session.Remove("MultiQuizQuestion");
            }

            HttpContext.Session.SetString("MultiQuizQuestion", JsonConvert.SerializeObject(response.Result));

            return View("MultiQuizSession", response.Result);
        }

        public async Task<IActionResult> QuizComplete(int quizId)
        {
            var userId = _userManager.GetUserId(User);
            var (quizResult, statusCode) = await _quizApiService.GetResultFromMultiQuiz(quizId, userId);

            if(statusCode == HttpStatusCode.OK)
            {
                return View("MultiQuizCompleteResult", quizResult);
            }

			HttpContext.Session.Remove("MultiQuizQuestion");

			TempData["ErrorMessage"] = "Es gab ein Problem mit der Anfrage. Bitte versuchen Sie es erneut oder wenden Sie sich an den Support.";
			return View("MultiQuizCompleteResult", quizResult);
		}

		[HttpPost]
        public async Task<IActionResult> SubmitAnswer(int selectedAnswerId)
        {
            var userId = _userManager.GetUserId(User);
            var quizQuestionJson = HttpContext.Session.GetString("MultiQuizQuestion");
            var quizQuestion = JsonConvert.DeserializeObject<GetQuizQuestionDto>(quizQuestionJson);
            var answer = quizQuestion.Answers.FirstOrDefault(x => x.Id == selectedAnswerId);

            var updateMultiQuizSessionObj = new UpdateMultiQuizSessionDto
            {
                QuizId = quizQuestion.QuizId,
                QuestionId = quizQuestion.QuestionId,
                AnswerFromUserId = selectedAnswerId,
                UserId = userId,
            };

            var response = await _quizApiService.UpdateMultiQuizSession(updateMultiQuizSessionObj);

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
                SelectedAnswerId = selectedAnswerId,
                QuestionCount = quizQuestion.QuestionCount - 1
            };

            if (answer != null && answer.IsCorrectAnswer)
            {
                viewModel.CorrectAnswer = true;
            }
            else
            {
                viewModel.CorrectAnswer = false;
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

            var (result, statusCode) = await _quizApiService.GetMultiQuizzesFromUser(userId);

            if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Es ist ein Fehler aufgetreten, bitte versuche es nochmal.";
                return null;
            }

            return result;
            
        }
    }
}
