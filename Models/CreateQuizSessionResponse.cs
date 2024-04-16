namespace Project_Quizz_Frontend.Models
{
	/// <summary>
	/// Response from the CreateQuizSession method in the QuizSessionController.
	/// </summary>
	public class CreateQuizSessionResponse
	{
		public HttpResponseMessage HttpResponse { get; set; }
		public int CreatedQuizSessionId { get; set; }
	}
}
