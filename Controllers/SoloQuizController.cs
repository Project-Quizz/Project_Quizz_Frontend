using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;
using System.Linq;
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

		public async Task<IActionResult> SoloQuiz(int singleQuizId = 1) // Example quizId for demonstration
		{
			var userId = "PylzTest2"; // Example userId for demonstration
			var quizSession = await _quizApiService.GetSingleQuizSession(singleQuizId, userId);

			if (quizSession != null)
			{
				return View("~/Views/Quiz/SoloQuiz.cshtml", quizSession);
			}

			return RedirectToAction("Error", "Home");
		}

		[HttpPost]
		public async Task<IActionResult> SubmitAnswer(SoloQuizModel quizSession, int selectedAnswerId, string action)
		{
			// Ensure quizSession is properly populated, possibly by fetching again or ensuring it's passed correctly
			// This example assumes quizSession is correctly populated and includes the current state of the quiz

			if (quizSession == null)
			{
				// Handle null quizSession appropriately
				return RedirectToAction("Error", "Home");
			}

			var currentQuestion = quizSession.CurrentQuestion;

			// Determine if the selected answer is correct
			var isAnswerCorrect = currentQuestion.Answers.Any(a => a.id == selectedAnswerId && a.IsCorrectAnswer);

			// Update attempt for the current question with selected answer and correctness
			var currentAttempt = quizSession.QuizAttempts.FirstOrDefault(a => a.AskedQuestionId == currentQuestion.Id);
			if (currentAttempt != null)
			{
				currentAttempt.GivenAnswerId = selectedAnswerId;
				// Optionally update answerDate if your model and requirements include tracking the answer time
				currentAttempt.AnswerDate = DateTime.UtcNow;
			}

			// Update the score if the answer is correct
			if (isAnswerCorrect)
			{
				quizSession.Score++;
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
				quizSession.QuizCompleted = true;
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
			if (quizSession.QuizCompleted)
			{
				// Redirect to a completion page if the quiz is complete
				return RedirectToAction("QuizComplete", new { score = quizSession.Score });
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