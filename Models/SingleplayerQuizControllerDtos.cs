﻿using System.ComponentModel.DataAnnotations;

namespace Project_Quizz_Frontend.Models
{
    public class SoloQuizAnswerResultViewModel
    {
        public int QuizId { get; set; }
        public GetQuizQuestionDto QuizQuestionDto { get; set; }
        public int SelectedAnswerId { get; set; }
        public bool CorrectAnswer { get; set; }
        public bool QuizComplete { get; set; }
        public int? QuestionCount { get; set; }
    }

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

    public class UpdateSingleQuizSessionDto
    {
        public int QuizId { get; set; }
        public int QuestionId { get; set; }
        public int AnswerFromUserId { get; set; }
        public string UserId { get; set; }
    }

    public class GetResultFromSingleQuizDto
    {
        public int quizId { get; set; }
        public int Score { get; set; }
        public bool QuizCompleted { get; set; }
        public int QuestionCount { get; set; }
    }
}