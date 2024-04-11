using System.ComponentModel.DataAnnotations;

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

    public class GetQuestionForEditingDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string UserId { get; set; }
        public CategorieIdDto Categorie { get; set; }
        public List<QuizAnswersDto> Answers { get; set; }
        public bool IsMultipleChoice { get; set; }
    }

    public class CreateQuizQuestionDto
    {
        public string QuestionText { get; set; }
        public string UserId { get; set; }
        public CategorieIdDto Categorie { get; set; } = new CategorieIdDto
        {
            CategorieId = 1
        };
        public List<AnswerViewModel> Answers { get; set; } = new List<AnswerViewModel>();
        public bool IsMultipleChoice { get; set; }
    }

    public class AnswerViewModel
    {
        [Required(ErrorMessage = "The answer text is required.")]
        public string AnswerText { get; set; }

        public bool IsCorrectAnswer { get; set; }
    }

    public class CategorieIdDto
    {
        public int CategorieId { get; set; }
        public string Name { get; set; }
    }

    public static class CategorieCache
    {
        public static List<CategorieIdDto> Categories { get; set; }
    }

    public class GetAllQuestionsFromUserDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public CategorieIdDto Categorie { get; set; }
        public int FeedbackCount { get; set; }
    }

    public class QuizMatchOverviewUserDto
    {
        public string UserId { get; set; }
        public int TotalPoints { get; set; }

        public int TotalPointsSingle { get; set; }
        public int TotalSingleGamesCount { get; set; }
        public int SingleGoldCount { get; set; }
        public int SingleSilverCount { get; set; }
        public int SingleBronzeCount { get; set; }

        public int TotalPointsMulti { get; set; }
        public int TotalMultiGamesCount { get; set; }
        public int MultiGoldCount { get; set; }
        public int MultiSilverCount { get; set; }
        public int MultiBronzeCount { get; set; }
    }

    public class HighscoreDataDto
    {
        public string UserId { get; set; }
        public int TotalGames { get; set; }
        public int TotalSingleGames { get; set; }
        public int TotalMultiGames { get; set; }
        public int TotalPointWorth { get; set; }
        public int TotalPointWorthSingle { get; set; }
        public int TotalPointWorthMulti { get; set; }
    }
}
