using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;


namespace Project_Quizz_Frontend.Controllers
{
	[Authorize]
	public class SoloQuizController : Controller
	{
		private readonly QuizApiService _quizApiService;
		private readonly UserManager<IdentityUser> _userManager;

		public SoloQuizController(QuizApiService quizApiService, UserManager<IdentityUser> userManager)
		{
			_quizApiService = quizApiService;
			_userManager = userManager;
		}

		public IActionResult SoloQuiz(GetQuizQuestionDto newQuizQuestion)
		{
			return View(newQuizQuestion);
		}

		public IActionResult SoloQuizAnswerResult(SoloQuizAnswerResultViewModel answerResult)
		{
			return View(answerResult);
		}

		public IActionResult SoloQuizCompleteResult(GetResultFromSingleQuizDto result)
		{
			return View(result);
		}

		public IActionResult SoloQuizSetup()
		{
			return View();
		}

		public async Task<IActionResult> CreateNewSingleQuizSession(int categorieId)
		{
			var userId = _userManager.GetUserId(User);
			var quizSession = await _quizApiService.CreateSingleQuizSession(userId, categorieId);

			var newQuizSessionId = quizSession.CreatedQuizSessionId;

			if (!quizSession.HttpResponse.IsSuccessStatusCode)
			{
				return RedirectToAction("Error", "Home");
			}

			return RedirectToAction("GetQuestion", new { quizId = newQuizSessionId});
		}

		public async Task<IActionResult> GetQuestion(int quizId)
		{
			var userId = _userManager.GetUserId(User);

			var quizQuestion = await _quizApiService.GetQuestionForSingleQuiz(quizId, userId);

			if (HttpContext.Session.GetString("QuizQuestion") != null)
			{
				HttpContext.Session.Remove("QuizQuestion");
			}

			HttpContext.Session.SetString("QuizQuestion", JsonConvert.SerializeObject(quizQuestion));

			return View("SoloQuiz", quizQuestion);
		}

		[HttpPost]
		public async Task<IActionResult> SubmitAnswer(int selectedAnswerId)
		{
			var userId = _userManager.GetUserId(User);
			var quizQuestionJson = HttpContext.Session.GetString("QuizQuestion");
			var quizQuestion = JsonConvert.DeserializeObject<GetQuizQuestionDto>(quizQuestionJson);
			var answer = quizQuestion.Answers.FirstOrDefault(x => x.Id == selectedAnswerId);

			var updateSinbgleQuizSessionObj = new UpdateSingleQuizSessionDto
			{
				QuizId = quizQuestion.QuizId,
				QuestionId = quizQuestion.QuestionId,
				AnswerFromUserId = selectedAnswerId,
				UserId = userId,
			};

			var response = await _quizApiService.UpdateSingleQuizSession(updateSinbgleQuizSessionObj);

			if(response.StatusCode == HttpStatusCode.BadRequest)
			{
				TempData["ErrorMessageBadRequest"] = "Es gab ein Problem mit der Anfrage. Bitte versuchen Sie es erneut oder wenden Sie sich an den Support.";
				return View("SoloQuiz", quizQuestion);
			}

			if(response.StatusCode == HttpStatusCode.Unauthorized)
			{
				TempData["ErrorMessageUnauthorized"] = "Eine bereits beantwortete Frage kann nicht nocheinmal Beantwortet werden. Um fortzufahren klicken Sie bitte auf \"Nächste Frage\"";
				return View("SoloQuiz", quizQuestion);
			}

			var viewModel = new SoloQuizAnswerResultViewModel
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
				return View("SoloQuizAnswerResult", viewModel);
			}

			if(response.StatusCode == HttpStatusCode.OK)
			{
				viewModel.QuizComplete = false;
				return View("SoloQuizAnswerResult", viewModel);
			}

			TempData["ErrorMessage"] = "Es ist ein Fehler aufgetreten, bitte versuche es nochmal.";
			return View("SoloQuiz", quizQuestion);

		}

		public async Task<IActionResult> QuizComplete(int quizId)
		{
			var userId = _userManager.GetUserId(User);
			var (quizResult, statusCode) = await _quizApiService.GetResultFromSingleQuiz(quizId, userId);

			if(statusCode == HttpStatusCode.OK)
			{
				return View("SoloQuizCompleteResult", quizResult);
			}

			HttpContext.Session.Remove("QuizQuestion");

			TempData["ErrorMessage"] = "Es gab ein Problem mit der Anfrage. Bitte versuchen Sie es erneut oder wenden Sie sich an den Support.";
			return View("SoloQuizCompleteResult", quizResult);
		}

		public async Task<IActionResult> SoloQuizCategorySelection()
		{
			var categories = await _quizApiService.GetAllCategoriesAsync();
			ViewBag.Categories = categories;
			return View("SoloQuizSetup");
		}
	}
}