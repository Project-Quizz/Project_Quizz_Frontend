using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project_Quizz_Frontend.Models;
using System.Collections.Generic;
using System.Linq;

namespace Project_Quizz_Frontend.Controllers
{
	public static class SessionExtensions
	{
		public static void Set<T>(this ISession session, string key, T value)
		{
			session.SetString(key, JsonConvert.SerializeObject(value));
		}

		public static T Get<T>(this ISession session, string key)
		{
			var value = session.GetString(key);
			return value == null ? default : JsonConvert.DeserializeObject<T>(value);
		}
	}

	public class SoloQuizController : Controller
	{
		public IActionResult SoloQuiz()
		{
			var quiz = new SoloQuizModel
			{
				QuestionId = 1,
				QuestionText = "What is the capital of France?",
				Answers = new List<QuizAnswerModel>
				{
					new QuizAnswerModel { AnswerId = 1, AnswerText = "Paris", IsCorrect = true },
					new QuizAnswerModel { AnswerId = 2, AnswerText = "Berlin", IsCorrect = false },
					new QuizAnswerModel { AnswerId = 3, AnswerText = "Madrid", IsCorrect = false },
					new QuizAnswerModel { AnswerId = 4, AnswerText = "Rome", IsCorrect = false }
				}
			};

			HttpContext.Session.Set("quiz", quiz);
			return View("~/Views/Quiz/SoloQuiz.cshtml", quiz);
		}

		[HttpPost]
		public IActionResult SubmitAnswer(int selectedAnswerId, string action)
		{
			var quiz = HttpContext.Session.Get<SoloQuizModel>("quiz");
			var selectedAnswer = quiz.Answers.FirstOrDefault(a => a.AnswerId == selectedAnswerId);
			bool isCorrect = selectedAnswer != null && selectedAnswer.IsCorrect;

			// Set the IsAnswerCorrect property
			quiz.IsAnswerCorrect = isCorrect;

			return View("~/Views/Quiz/SoloQuiz.cshtml", quiz);
		}
	}
}
