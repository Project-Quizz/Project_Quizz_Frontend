namespace Project_Quizz_Frontend.Models
{
	public class SoloQuizAnswerResultViewModel
	{
		public GetQuizQuestionDto QuizQuestionDto { get; set; }
		public int SelectedAnswerId { get; set; }
		public bool CorrectAnswer { get; set; }
		public bool QuizComplete { get; set; }
	}
}
