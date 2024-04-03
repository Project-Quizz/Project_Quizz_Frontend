using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project_Quizz_Frontend.Models
{
	public class QuizAttemptModel
	{
		public int Id { get; set; }
		public int AskedQuestionId { get; set; }
		public int? GivenAnswerId { get; set; }
		public DateTime? AnswerDate { get; set; }
	}

	public class QuizQuestionModel
	{
		public int Id { get; set; }
		[Required]
		public string QuestionText { get; set; }
		public List<QuizAnswerModel> Answers { get; set; }
	}

	public class QuizAnswerModel
	{
		public int id { get; set; }
		public string AnswerText { get; set; }
		public bool IsCorrectAnswer { get; set; }
	}

	public class SoloQuizModel
	{
		public int id { get; set; }
		public string UserId { get; set; }
		public int Score { get; set; }
		public DateTime CreateDate { get; set; }
		public bool QuizCompleted { get; set; }
		public int QuestionCount { get; set; }
		public List<QuizAttemptModel> QuizAttempts { get; set; }
		public List<QuizQuestionModel> question { get; set; }

		// Additional properties to support quiz flow
		public int CurrentQuestionIndex { get; set; } = 0;
		public QuizQuestionModel CurrentQuestion => question != null && question.Count > CurrentQuestionIndex ? question[CurrentQuestionIndex] : null;
		public bool HasNextQuestion => CurrentQuestionIndex + 1 < question.Count;
	}
}