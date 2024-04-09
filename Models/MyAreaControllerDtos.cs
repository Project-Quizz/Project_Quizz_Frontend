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
    }
}
