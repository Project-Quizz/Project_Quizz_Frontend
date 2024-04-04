using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Project_Quizz_Frontend.Models;

namespace Project_Quizz_Frontend.Services;

public class QuizApiService
{
	private readonly HttpClient _httpClient;
	private readonly string _apiBaseUrl = "https://trusting-black.188-40-219-72.plesk.page/api";

	public QuizApiService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<SoloQuizModel> GetSingleQuizSession(int singleQuizId, string userId)
	{
		var response = await _httpClient.GetFromJsonAsync<SoloQuizModel>($"{_apiBaseUrl}/SingleQuizWorkshop/GetSingleQuizSession?id={singleQuizId}&userId={userId}");
		return response;
	}

	public async Task<bool> SubmitAnswerAsync(QuizAnswerModel answer)
	{
		var response = await _httpClient.PutAsJsonAsync($"{_apiBaseUrl}/SingleQuizWorkshop/UpdateSingleQuizSession", answer);
		return response.IsSuccessStatusCode;
	}

	public async Task<SoloQuizModel> CreateSingleQuizSession(int singleQuizId)
	{
		var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/SingleQuizWorkshop/CreateSingleQuizSession", new { singleQuizId });
		if (response.IsSuccessStatusCode)
		{
			var quizSession = await response.Content.ReadFromJsonAsync<SoloQuizModel>();
			return quizSession;
		}
		return null;
	}

	// This method is used for creating a new quiz question
	public async Task<HttpResponseMessage> CreateQuestionAsync(QuizQuestionViewModel questionViewModel)
	{
		var json = JsonSerializer.Serialize(questionViewModel);
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		var response = await _httpClient.PostAsync($"{_apiBaseUrl}/QuestionWorkshop/CreateQuestion", content);
		return response;
	}

	public async Task<bool> UpdateSingleQuizSession(SoloQuizModel quizSession)
	{
		// Construct the payload for updating the quiz session
		var updatePayload = new
		{
			id = quizSession.id,
			score = quizSession.score,
			quizCompleted = quizSession.quizCompleted,
			quiz_Attempts = quizSession.quiz_Attempts.Select(attempt => new
			{
				id = attempt.id,
				givenAnswerId = attempt.givenAnswerId,
				answerDate = attempt.answerDate.HasValue ? attempt.answerDate.Value.ToString("o") : null
			}).ToList()
		};

		var json = JsonSerializer.Serialize(updatePayload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await _httpClient.PutAsync($"{_apiBaseUrl}/SingleQuizWorkshop/UpdateSingleQuizSession", content);
		return response.IsSuccessStatusCode;
	}
	public async Task<List<CategorieIdDto>> GetAllCategoriesAsync()
	{
		var response = await _httpClient.GetAsync($"{_apiBaseUrl}/CategorieWorkshop/GetAllCategories");
		if (response.IsSuccessStatusCode)
		{
			var categories = await response.Content.ReadFromJsonAsync<List<CategorieIdDto>>();
			return categories;
		}
		return new List<CategorieIdDto>();
	}
}