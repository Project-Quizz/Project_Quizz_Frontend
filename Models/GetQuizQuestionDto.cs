namespace Project_Quizz_Frontend.Models
{
	public class GetQuizQuestionDto
	{
		public int QuestionId { get; set; }
		public int QuizId { get; set; }
		public int? QuestionCount { get; set; }
		public string QuestionText { get; set; }
		public List<QuizAnswersDto> Answers { get; set; }
	}

	public class QuizAnswersDto
	{
		public int Id { get; set; }
		public string AnswerText { get; set; }
		public bool IsCorrectAnswer { get; set; }
	}
}
