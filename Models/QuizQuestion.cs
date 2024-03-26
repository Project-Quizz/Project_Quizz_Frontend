using System.ComponentModel.DataAnnotations;

namespace Project_Quizz_Frontend.Models
{
    // Class for quiz questions
    public class QuizQuestion
    {
        // The unique ID of the question
        public int Id { get; set; }

        // The text of the question, which is required
        [Required(ErrorMessage = "The question text is required.")]
        public string QuestionText { get; set; }

        // A list of answers to the question
        public List<Answer> Answers { get; set; } = [];
    }
}