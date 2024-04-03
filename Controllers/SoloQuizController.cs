using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

		public SoloQuizController(QuizApiService quizApiService)
		{
			_quizApiService = quizApiService;
		}

		public async Task<IActionResult> SoloQuiz(int singleQuizId = 1)
		{
			var userId = "PylzTest2"; // Adjust as necessary
			var quizSession = await _quizApiService.GetSingleQuizSession(singleQuizId, userId);

			if (quizSession != null)
			{
				// Serialize and store the quiz session in ASP.NET Core Session
				var quizSessionJson = JsonSerializer.Serialize(quizSession);
				HttpContext.Session.SetString("QuizSession", quizSessionJson);

				return View("~/Views/Quiz/SoloQuiz.cshtml", quizSession);
			}
			return RedirectToAction("Error", "Home");
		}

		[HttpPost]
		public async Task<IActionResult> SubmitAnswer(int selectedAnswerId, string action)
		{
			var quizSessionJson = HttpContext.Session.GetString("QuizSession");
			if (string.IsNullOrEmpty(quizSessionJson))
			{
				return RedirectToAction("Error", "Home");
			}

			var quizSession = JsonSerializer.Deserialize<SoloQuizModel>(quizSessionJson);

			// Now use quizSession as before
			// After updating quizSession, don't forget to store it back to the session
			quizSessionJson = JsonSerializer.Serialize(quizSession);
			HttpContext.Session.SetString("QuizSession", quizSessionJson);

			// Ensure quizSession is properly populated, possibly by fetching again or ensuring it's passed correctly
			// This example assumes quizSession is correctly populated and includes the current state of the quiz

			if (quizSession == null)
			{
				// Handle null quizSession appropriately
				return RedirectToAction("Error", "Home");
			}

			var currentQuestion = quizSession.CurrentQuestion;

			// Determine if the selected answer is correct
			var isAnswerCorrect = currentQuestion.answers.Any(a => a.id == selectedAnswerId && a.isCorrectAnswer);

			// Update attempt for the current question with selected answer and correctness
			var currentAttempt = quizSession.quiz_Attempts.FirstOrDefault(a => a.askedQuestionId == currentQuestion.id);
			if (currentAttempt != null)
			{
				currentAttempt.givenAnswerId = selectedAnswerId;
				// Optionally update answerDate if your model and requirements include tracking the answer time
				currentAttempt.answerDate = DateTime.UtcNow;
			}

			// Update the score if the answer is correct
			if (isAnswerCorrect)
			{
				quizSession.score++;
			}

			// Prepare to move to the next question or end the quiz
			if (action.Equals("next", StringComparison.OrdinalIgnoreCase) && quizSession.HasNextQuestion)
			{
				// Increment to the next question
				quizSession.CurrentQuestionIndex++;
			}
			else if (!quizSession.HasNextQuestion)
			{
				// Mark the quiz as completed if there are no more questions
				quizSession.quizCompleted = true;
			}

			// Call API to update the quiz session state
			var updateSuccessful = await _quizApiService.UpdateSingleQuizSession(quizSession);
			if (!updateSuccessful)
			{
				// Log error or handle unsuccessful update accordingly
				TempData["Error"] = "There was a problem updating your quiz session. Please try again.";
				// Consider what action to take if the update fails (e.g., retry, show error message, etc.)
			}

			// Determine redirect action after handling answer submission
			if (quizSession.quizCompleted)
			{
				// Redirect to a completion page if the quiz is complete
				return RedirectToAction("QuizComplete", new { score = quizSession.score });
			}
			else
			{
				// Otherwise, render the quiz view again with the next question or the same question if the answer was incorrect
				return View("~/Views/Quiz/SoloQuiz.cshtml", quizSession);
			}
		}
		public IActionResult QuizComplete(int score)
		{
			ViewBag.Score = score;
			return View();
		}
	}
}