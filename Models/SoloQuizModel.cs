﻿using System;
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

		// Additional properties to support quiz flow
		public int CurrentQuestionIndex { get; set; } = 0;
		public QuizQuestionModel CurrentQuestion => question != null && question.Count > CurrentQuestionIndex ? question[CurrentQuestionIndex] : null;
		public bool HasNextQuestion => question != null && question.Count > 0 && CurrentQuestionIndex + 1 < question.Count;
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
}