using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Linq;
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

		public IActionResult SoloQuizAnswerResult(SoloQuizAnswerResultViewModel viewModel)
		{
			return View(viewModel);
		}

		public async Task<IActionResult> CreateNewSingleQuizSession(int categorieId = 1)
		{
			var userId = _userManager.GetUserId(User);
			var quizSession = await _quizApiService.CreateSingleQuizSession(userId, categorieId);

			var newQuizSessionId = quizSession.CreatedQuizSessionId;

			if (!quizSession.HttpResponse.IsSuccessStatusCode)
			{
				return RedirectToAction("Error", "Home");
			}

			return RedirectToAction("GetQuestion", new { quizId = newQuizSessionId });
		}

		public async Task<IActionResult> GetQuestion(int quizId)
		{
			var userId = _userManager.GetUserId(User);

			var quizQuestion = await _quizApiService.GetQuestionForSingleQuiz(quizId, userId);

			HttpContext.Session.SetString("QuizQuestion", JsonConvert.SerializeObject(quizQuestion));

			return View("SoloQuiz", quizQuestion);
		}

		[HttpPost]
		public async Task<IActionResult> SubmitAnswer(int selectedAnswerId)
		{
			var quizQuestionJson = HttpContext.Session.GetString("QuizQuestion");
			var quizQuestion = JsonConvert.DeserializeObject<GetQuizQuestionDto>(quizQuestionJson);
			var answer = quizQuestion.Answers.FirstOrDefault(x => x.Id == selectedAnswerId);

            var viewModel = new SoloQuizAnswerResultViewModel
			{
				QuizQuestionDto = quizQuestion,
				SelectedAnswerId = selectedAnswerId
			};

			if(quizQuestion.QuestionCount == 1)
			{
				viewModel.QuizComplete = true;
			} 
			else
			{
				viewModel.QuizComplete = false;
			}

			if(answer.IsCorrectAnswer)
			{
				viewModel.CorrectAnswer = true;
			}
			else
			{
				viewModel.CorrectAnswer= false;
			}

			return View("SoloQuizAnswerResult", viewModel);
		}

		public IActionResult QuizComplete(int score)
		{
			ViewBag.Score = score;
			return View();
		}
	}
}