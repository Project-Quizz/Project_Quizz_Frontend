namespace Project_Quizz_Frontend.Models
{
	public class CreateQuizQuestionFeedbackDto
	{
		public int QuestionId { get; set; }
		public string Feedback { get; set; }
		public string UserId { get; set; }
	}

	public class GetQuizQuestionFeedbackDto
	{
		public int QuestionId { get; set; }
		public string Feedback { get; set; }
		public string UserId { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
