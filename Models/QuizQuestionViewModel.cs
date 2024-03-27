using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Project_Quizz_Frontend.Models
{
	// Class for quiz questions
	public class QuizQuestionViewModel
	{
		public string QuestionText { get; set; }
		public List<AnswerViewModel> Answers { get; set; } = new List<AnswerViewModel>();
	}

	// AnswerViewModel class for quiz questions
	public class AnswerViewModel
	{
		// The unique ID of the answer
		public int Id { get; set; }

		// The text of the answer, which is required
		[Required(ErrorMessage = "The answer text is required.")]
		public string AnswerText { get; set; }

		// A flag indicating whether the answer is correct or not
		public bool IsCorrectAnswer { get; set; }
	}
}