using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_Quizz_Frontend.Models;
using Project_Quizz_Frontend.Services;

namespace Project_Quizz_Frontend.Controllers
{
	public class QuestionFeedbackController : Controller
	{
		private readonly QuizApiService _quizApiService;
		private readonly UserManager<IdentityUser> _userManager;

        public QuestionFeedbackController(QuizApiService quizApiService, UserManager<IdentityUser> userManager)
        {
            _quizApiService = quizApiService;
			_userManager = userManager;
        }

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
