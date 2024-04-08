using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Project_Quizz_Frontend.Models
{
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
	public class QuizAttemptModel
	{
		public int id { get; set; }
		public int askedQuestionId { get; set; }
		public int? givenAnswerId { get; set; }
		public DateTime? answerDate { get; set; }
	}

	public class QuizQuestionModel
	{
		public int id { get; set; }
		[Required]
		public string questionText { get; set; }
		public List<QuizAnswerModel> answers { get; set; }
	}

	public class QuizAnswerModel
	{
		public int id { get; set; }
		public string answerText { get; set; }
		public bool isCorrectAnswer { get; set; }
	}

    public class GetSingleQuizzesFromUserDto
    {
        public int QuizId { get; set; }
        public DateTime QuizCreated { get; set; }
        public bool UserCompletedQuiz { get; set; }
        public int Score { get; set; }
        public QuizCategorieDto Categorie { get; set; }
    }
}