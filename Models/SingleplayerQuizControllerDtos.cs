using System.ComponentModel.DataAnnotations;

namespace Project_Quizz_Frontend.Models
{
    /// <summary>
    /// The ViewModel for the SoloQuizAnswer
    /// </summary>
    public class SoloQuizAnswerResultViewModel
    {
        public int QuizId { get; set; }
        public GetQuizQuestionDto QuizQuestionDto { get; set; }
        public List<SingleQuizGivenAnswerIdsViewModel> GivenAnswerIds { get; set; }
        public bool QuizComplete { get; set; }
        public int? QuestionCount { get; set; }
        public bool IsMultipleChoice { get; set; }
        public bool IsAnswerCorrect { get; set; }
    }

    /// <summary>
    /// The ViewModel for the SingleQuizGivenAnswerIds
    /// </summary>
    public class SingleQuizGivenAnswerIdsViewModel
	{
		public int QuizQuestionAnswerId { get; set; }
		public bool IsCorrectAnswer { get; set; }
	}

    /// <summary>
    /// The model of a solo quiz
    /// </summary>
	public class SoloQuizModel
    {
        public int id { get; set; }
        public string userId { get; set; }
        public int score { get; set; }
        public DateTime createDate { get; set; }
        public bool quizCompleted { get; set; }
        public int questionCount { get; set; }
        public List<QuizAttemptModel> quiz_Attempts { get; set; }
        public List<QuizQuestionModel> question { get; set; }
    }

    /// <summary>
    /// The model of a quiz attempt
    /// </summary>
    public class QuizAttemptModel
    {
        public int id { get; set; }
        public int askedQuestionId { get; set; }
        public int? givenAnswerId { get; set; }
        public DateTime? answerDate { get; set; }
    }

    /// <summary>
    /// The model of a quiz question
    /// </summary>
    public class QuizQuestionModel
    {
        public int id { get; set; }
        [Required]
        public string questionText { get; set; }
        public List<QuizAnswerModel> answers { get; set; }
        public bool isMultipleChoice { get; set; }
    }

    /// <summary>
    /// The model of a quiz answer
    /// </summary>
    public class QuizAnswerModel
    {
        public int id { get; set; }
        public string answerText { get; set; }
        public bool isCorrectAnswer { get; set; }
        public bool isSelected { get; set; }
    }

    /// <summary>
    /// The DTO to get a single quiz from a user
    /// </summary>
    public class GetSingleQuizzesFromUserDto
    {
        public int QuizId { get; set; }
        public DateTime QuizCreated { get; set; }
        public bool UserCompletedQuiz { get; set; }
        public int Score { get; set; }
        public QuizCategorieDto Categorie { get; set; }
    }

    /// <summary>
    /// The DTO to update single quiz session
    /// </summary>
    public class UpdateSingleQuizSessionDto
    {
        public int QuizId { get; set; }
        public int QuestionId { get; set; }
        public List<SingleQuizGivenAnswerIdsDto> GivenAnswerIds { get; set; }
        public string UserId { get; set; }
    }

    /// <summary>
    /// The DTO for the given answer ids
    /// </summary>
    public class SingleQuizGivenAnswerIdsDto
	{
		public int QuizQuestionAnswerId { get; set; }
	}

    /// <summary>
    /// The DTO to get the result from a single quiz
    /// </summary>
	public class GetResultFromSingleQuizDto
    {
        public int quizId { get; set; }
        public int Score { get; set; }
        public bool QuizCompleted { get; set; }
        public int QuestionCount { get; set; }
    }
}
