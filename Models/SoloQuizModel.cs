using System.ComponentModel.DataAnnotations;

namespace Project_Quizz_Frontend.Models // Replace with your actual namespace
{
	public class QuizAnswerModel
	{
		public int AnswerId { get; set; }

		public string AnswerText { get; set; }

		public bool IsCorrect { get; set; }
	}

	public class QuizQuestionModel
	{
		public int Id { get; set; }

		[Required]
		public string QuestionText { get; set; }

		public List<QuizAnswerModel> Answers { get; set; } = new List<QuizAnswerModel>();
	}

	public class SoloQuizModel
	{
		public int SessionId { get; set; }
		public string UserId { get; set; }
		public int Score { get; set; }
		public bool QuizCompleted { get; set; }
		public List<QuizQuestionModel> Questions { get; set; } = new List<QuizQuestionModel>();

		// Helper properties
		public int CurrentQuestionIndex { get; set; } = 0;

		public QuizQuestionModel CurrentQuestion => Questions.Count > CurrentQuestionIndex
			? Questions[CurrentQuestionIndex]
			: null;

		public bool HasNextQuestion => CurrentQuestionIndex + 1 < Questions.Count;
	}
}