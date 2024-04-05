namespace Project_Quizz_Frontend.Models
{
	public class UpdateSingleQuizSessionDto
	{
		public int QuizId { get; set; }
		public int QuestionId { get; set; }
		public int AnswerFromUserId { get; set; }
		public string UserId { get; set; }
	}
}
