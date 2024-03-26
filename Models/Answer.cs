using System.ComponentModel.DataAnnotations;

namespace Project_Quizz_Frontend.Models
{
    // Answer class for quiz questions
    public class Answer
    {
        // The unique ID of the answer
        public int Id { get; set; }

        // The text of the answer, which is required
        [Required(ErrorMessage = "The answer text is required.")]
        public string AnswerText { get; set; }

        // A flag indicating whether the answer is correct or not
        public bool IsCorrect { get; set; }
    }
}