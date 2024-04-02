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

        public async Task<IActionResult> SoloQuiz(int sessionId = 9) // Assuming a fixed session ID for now
        {
            var userId = "PylzTest"; // Assuming a fixed user ID for now
            var quizSession = await _quizApiService.GetSingleQuizSession(sessionId, userId);

            if (quizSession != null)
            {
                // Directly pass the fetched QuizSession to the view
                return View("~/Views/Quiz/SoloQuiz.cshtml", quizSession);
            }

            // Handle case where quiz session is not found or an error occurs
            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(SoloQuizModel quizSession, int selectedAnswerId, string action)
        {
            // Update the current question index based on the action
            if (action == "next" && quizSession.HasNextQuestion)
            {
                quizSession.CurrentQuestionIndex++;
            }

            // Check if the selected answer is correct for the current question
            var currentQuestion = quizSession.Questions[quizSession.CurrentQuestionIndex];
            var isCorrect = currentQuestion.Answers.Any(a => a.AnswerId == selectedAnswerId && a.IsCorrect);

            // Update the quiz session state accordingly
            quizSession.Questions[quizSession.CurrentQuestionIndex].IsAnswerCorrect = isCorrect;

            // If there are more questions, stay on the quiz; otherwise, go to quiz completion
            if (quizSession.HasNextQuestion)
            {
                return View("~/Views/Quiz/SoloQuiz.cshtml", quizSession);
            }
            else
            {
                // Optionally, update the quiz session to mark it as completed
                // await _quizApiService.UpdateSingleQuizSession(quizSession);
                return RedirectToAction("QuizComplete");
            }
        }

        public IActionResult QuizComplete()
        {
            // Display the final score or a completion message
            return View();
        }
    }
}