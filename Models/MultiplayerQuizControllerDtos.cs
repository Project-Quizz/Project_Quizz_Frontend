namespace Project_Quizz_Frontend.Models
{
    /// <summary>
    /// The ViewModel for the MultiplayerQuizController
    /// </summary>
    public class MultiQuizAnswerResultViewModel
    {
        public int QuizId { get; set; }
        public GetQuizQuestionDto QuizQuestionDto { get; set; }
		public List<MultiQuizGivenAnswerIdsViewModel> GivenAnswerIds { get; set; }
        public bool QuizComplete { get; set; }
        public int? QuestionCount { get; set; }
        public bool IsMultipleChoice { get; set; }
        public bool IsAnswerCorrect { get; set; }
    }

    /// <summary>
    /// The ViewModel for the MultiplayerQuizController
    /// </summary>
	public class MultiQuizGivenAnswerIdsViewModel
	{
		public int QuizQuestionAnswerId { get; set; }
        public bool IsCorrectAnswer { get; set; }
	}

    /// <summary>
    /// The DTO for the UpdateMultiQuizSession
    /// </summary>
	public class UpdateMultiQuizSessionDto
    {
        public int QuizId { get; set; }
        public int QuestionId { get; set; }
		public List<MultiQuizGivenAnswerIdsDto> GivenAnswerIds { get; set; }
		public string UserId { get; set; }
    }

    /// <summary>
    /// The DTO for the MultiQuizGivenAnswerIds
    /// </summary>
	public class MultiQuizGivenAnswerIdsDto
	{
		public int QuizQuestionAnswerId { get; set; }
	}

    /// <summary>
    /// The DTO for create a new MultiplayerQuiz
    /// </summary>
    /// </summary>
	public class IniMultiplayerDtos
    {
        public string UserOne { get; set; }
        public string UserTwo { get; set; }
        public int CategorieId { get; set; }
    }

    /// <summary>
    /// The DTO for the GetResultFromMultiQuiz
    /// </summary>
    public class GetResultFromMultiQuizDto
    {
        public int quizId { get; set; }
        public int Score { get; set; }
        public bool QuizCompleted { get; set; }
        public bool MultiQuizComplete { get; set; }
        public int QuestionCount { get; set; }
        public OpponentDto Opponent { get; set; }
    }

    /// <summary>
    /// The DTO for the Opponent
    /// </summary>
    public class OpponentDto
    {
        public string UserId { get; set; }
        public int Score { get; set; }
        public bool QuizComplete { get; set; }
    }

    /// <summary>
    /// The DTO for the GetMultiQuizzesFromUser
    /// </summary>
    public class GetMultiQuizzesFromUserDto
    {
        public int MultiQuizId { get; set; }
        public DateTime QuizCreated { get; set; }
        public bool UserCompletedQuiz { get; set; }
        public int Score { get; set; }
        public string OpponentUser { get; set; }
        public QuizCategorieDto Categorie { get; set; }
    }

    /// <summary>
    /// The DTO for the QuizCategorie
    /// </summary>
    public class QuizCategorieDto
    {
        public int CategorieId { get; set; }
        public string Name { get; set; }
    }
}
