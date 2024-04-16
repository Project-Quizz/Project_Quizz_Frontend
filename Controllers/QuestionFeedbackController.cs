using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;

namespace Project_Quizz_Frontend.Controllers
{
	/// <summary>
	/// Is responsible for handling requests related to question feedback.
	/// </summary>
	public class QuestionFeedbackController : Controller
	{
		private readonly QuizApiService _quizApiService;
		private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Constructor for the QuestionFeedbackController.
		/// </summary>
		/// <param name="quizApiService"></param>
		/// <param name="userManager"></param>
        public QuestionFeedbackController(QuizApiService quizApiService, UserManager<IdentityUser> userManager)
        {
            _quizApiService = quizApiService;
			_userManager = userManager;
        }

		/// <summary>
		/// Creates feedback for a question.
		/// </summary>
		/// <param name="questionId">The question id for the feedback</param>
		/// <param name="feedback">The feedback as string</param>
		/// <returns>Return a Json with information if the create feedback was successful or not</returns>
		public async Task<IActionResult> CreateFeedbackForQuestion(int questionId, string feedback)
		{
			var feedbackDto = new CreateQuizQuestionFeedbackDto
			{
				QuestionId = questionId,
				Feedback = feedback,
				UserId = _userManager.GetUserId(User)
			};

			var response = await _quizApiService.CreateFeedbackForQuestion(feedbackDto);

			if (response.IsSuccessStatusCode)
			{
				return Json(new { success = true });
			}

			return Json(new { success = false });
		}
	}
}
