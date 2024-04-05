namespace Project_Quizz_Frontend.Models
{
	public class GetResultFromSingleQuizDto
	{
		public int quizId { get; set; }
		public int Score { get; set; }
		public bool QuizCompleted { get; set; }
		public int QuestionCount { get; set; }
	}
}
