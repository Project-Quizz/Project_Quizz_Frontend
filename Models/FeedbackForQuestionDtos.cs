namespace Project_Quizz_Frontend.Models
{
	/// <summary>
	/// The DTO for creating a feedback for a question
	/// </summary>
	public class CreateQuizQuestionFeedbackDto
	{
		public int QuestionId { get; set; }
		public string Feedback { get; set; }
		public string UserId { get; set; }
	}

	/// <summary>
	/// The DTO for getting a feedback for a question
	/// </summary>
	public class GetQuizQuestionFeedbackDto
	{
		public int QuestionId { get; set; }
		public string Feedback { get; set; }
		public string UserId { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
