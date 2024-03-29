using System.ComponentModel.DataAnnotations;

namespace Project_Quizz_Frontend.Models // Replace with your actual namespace
{
	public class SoloQuizModel
	{
		public int Id { get; set; }

		[Required] public string QuestionText { get; set; }

		public List<QuizAnswerModel> Answers { get; set; } = new List<QuizAnswerModel>();

		// Indicate if there is a next quiz available (it is available if the current QuestionId is less than the total number of questions)
		public int QuestionId { get; set; }

		// The ID of the selected answer
		public int SelectedAnswerId { get; set; }

		// Total number of questions, hardcoded to 2 for now
		public int TotalQuestions { get; set; } = 2;

		// Update HasNextQuiz to check if QuestionId is less than TotalQuestions
		public bool HasNextQuiz => QuestionId < TotalQuestions;
		public bool? IsAnswerCorrect { get; set; }
	}

	public class QuizAnswerModel
	{
		public int AnswerId { get; set; }

		public string AnswerText { get; set; }

		public bool IsCorrect { get; set; }
	}
}