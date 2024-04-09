﻿using Microsoft.AspNetCore.Authorization;
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
	public class SingleplayerQuizController : Controller
	{
		private readonly QuizApiService _quizApiService;
        private readonly SingleplayerApiService _singleplayerApiService;
        private readonly UserManager<IdentityUser> _userManager;

		public SingleplayerQuizController(QuizApiService quizApiService,SingleplayerApiService singleplayerApiService, UserManager<IdentityUser> userManager)
		{
			_quizApiService = quizApiService;
			_singleplayerApiService = singleplayerApiService;
			_userManager = userManager;
		}

		public async  Task<IActionResult> SingleplayerIndex()
		{
            var notificationCount = await LoadSingleplayerNotification();
            ViewBag.NotificationCount = notificationCount.ToString();
            return View();
		}

		public IActionResult SingleQuizSession(GetQuizQuestionDto newQuizQuestion)
		{
			return View(newQuizQuestion);
		}

		public IActionResult SingleQuizAnswerResult(SoloQuizAnswerResultViewModel answerResult)
		{
			return View(answerResult);
		}

		public IActionResult SingleQuizCompleteResult(GetResultFromSingleQuizDto result)
		{
			return View(result);
		}

		public async Task<IActionResult>  SingleplayerSettings()
		{
			var categories = await LoadCategories();
            ViewBag.Categories = categories;
            return View();
		}

		public async Task<IActionResult> OverviewOfOpenSingleQuizzes()
		{
			var quizList = await LoadSingleQuizzesFromUser();
			if (quizList == null)
			{
				quizList = new List<GetSingleQuizzesFromUserDto>();
			}
			return View(quizList);
		}


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

		public async Task<IActionResult> GetQuestion(int quizId)
		{
			var userId = _userManager.GetUserId(User);

			var (quizQuestion, statusCode) = await _singleplayerApiService.GetQuestionForSingleQuiz(quizId, userId);

			if (statusCode == HttpStatusCode.OK)
			{
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

			var response = await _singleplayerApiService.UpdateSingleQuizSession(updateSinbgleQuizSessionObj);

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

		private async Task<List<CategorieIdDto>> LoadCategories()
		{
			// Get categories from cache
			var categories = CategorieCache.Categories;

			// If cache is empty, get categories from API
			if (categories == null)
			{
				categories = await _quizApiService.GetAllCategoriesAsync();
				CategorieCache.Categories = categories;
			}
			return categories;
		}

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