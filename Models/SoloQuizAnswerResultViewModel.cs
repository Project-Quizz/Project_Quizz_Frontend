namespace Project_Quizz_Frontend.Models
{
	public class SoloQuizAnswerResultViewModel
	{
		public int QuizId { get; set; }
		public GetQuizQuestionDto QuizQuestionDto { get; set; }
		public int SelectedAnswerId { get; set; }
		public bool CorrectAnswer { get; set; }
		public bool QuizComplete { get; set; }
		public int? QuestionCount { get; set; }
	}

	public class MultiQuizAnswerResultViewModel
	{
		public int QuizId { get; set; }
		public GetQuizQuestionDto QuizQuestionDto { get; set; }
		public int SelectedAnswerId { get; set; }
		public bool CorrectAnswer { get; set; }
		public bool QuizComplete { get; set; }
		public int? QuestionCount { get; set; }
	}
}
