namespace Project_Quizz_Frontend.Models
{
    public class GetQuestionForEditingDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string UserId { get; set; }
        public CategorieIdDto Categorie { get; set; }
        public List<QuizAnswersDto> Answers { get; set; }
    }
}
