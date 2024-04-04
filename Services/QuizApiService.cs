using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Project_Quizz_API.Models.DTOs;
using Project_Quizz_Frontend.Models;

namespace Project_Quizz_Frontend.Services;

public class QuizApiService
{
	private readonly HttpClient _httpClient;
	private readonly string _apiBaseUrl = "https://trusting-black.188-40-219-72.plesk.page/api";
	private readonly string _apiTestUrl = "https://localhost:7241/api";


    public QuizApiService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<CreateQuizSessionResponse> CreateSingleQuizSession(string userId, int categorieId)
	{
		var response = await _httpClient.PostAsync($"{_apiTestUrl}/SingleQuizWorkshop/CreateSingleQuizSession?userId={userId}&categorieId={categorieId}", null);

		var quizSessionResponse = new CreateQuizSessionResponse
		{
			HttpResponse = response,
		};

		if(response.IsSuccessStatusCode)
		{
			var responseConent = await response.Content.ReadAsStringAsync();
			var responseJson = JsonConvert.DeserializeObject<Dictionary<string, int>>(responseConent);
			var singleQuizId = responseJson["singleQuizId"];
			quizSessionResponse.CreatedQuizSessionId = singleQuizId;
		}

		return quizSessionResponse;
	}

	public async Task<GetQuizQuestionDto> GetQuestionForSingleQuiz(int quizId, string userId)
	{
		var response = await _httpClient.GetFromJsonAsync<GetQuizQuestionDto>($"{_apiTestUrl}/SingleQuizWorkshop/GetQuestionFromQuizSession?quizId={quizId}&userId={userId}");
		return response;
	}

	//public async Task<SoloQuizModel> GetSingleQuizSession(int singleQuizId, string userId)
	//{
	//	var response = await _httpClient.GetFromJsonAsync<SoloQuizModel>($"{_apiBaseUrl}/SingleQuizWorkshop/GetSingleQuizSession?id={singleQuizId}&userId={userId}");
	//	return response;
	//}

	//public async Task<bool> SubmitAnswerAsync(QuizAnswerModel answer)
	//{
	//	var response = await _httpClient.PutAsJsonAsync($"{_apiBaseUrl}/SingleQuizWorkshop/UpdateSingleQuizSession", answer);
	//	return response.IsSuccessStatusCode;
	//}

	//public async Task<SoloQuizModel> CreateSingleQuizSession(int singleQuizId)
	//{
	//	var response = await _httpClient.PostAsJsonAsync($"{_apiTestUrl}/SingleQuizWorkshop/CreateSingleQuizSession", new { singleQuizId });
	//	if (response.IsSuccessStatusCode)
	//	{
	//		var quizSession = await response.Content.ReadFromJsonAsync<SoloQuizModel>();
	//		return quizSession;
	//	}
	//	return null;
	//}

	// This method is used for creating a new quiz question
	public async Task<HttpResponseMessage> CreateQuestionAsync(Models.CreateQuizQuestionDto questionViewModel)
	{
		var json = System.Text.Json.JsonSerializer.Serialize(questionViewModel);
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		var response = await _httpClient.PostAsync($"{_apiBaseUrl}/QuestionWorkshop/CreateQuestion", content);
		return response;
	}

	//public async Task<bool> UpdateSingleQuizSession(SoloQuizModel quizSession)
	//{
	//	// Construct the payload for updating the quiz session
	//	var updatePayload = new
	//	{
	//		id = quizSession.id,
	//		score = quizSession.score,
	//		quizCompleted = quizSession.quizCompleted,
	//		quiz_Attempts = quizSession.quiz_Attempts.Select(attempt => new
	//		{
	//			id = attempt.id,
	//			givenAnswerId = attempt.givenAnswerId,
	//			answerDate = attempt.answerDate.HasValue ? attempt.answerDate.Value.ToString("o") : null
	//		}).ToList()
	//	};

	//	var json = JsonSerializer.Serialize(updatePayload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
	//	var content = new StringContent(json, Encoding.UTF8, "application/json");

	//	var response = await _httpClient.PutAsync($"{_apiBaseUrl}/SingleQuizWorkshop/UpdateSingleQuizSession", content);
	//	return response.IsSuccessStatusCode;
	//}
}