using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

	public async Task<HttpResponseMessage> CreateQuestionAsync(Models.CreateQuizQuestionDto questionViewModel)
	{
		var json = System.Text.Json.JsonSerializer.Serialize(questionViewModel);
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		var response = await _httpClient.PostAsync($"{_apiTestUrl}/QuestionWorkshop/CreateQuestion", content);
		return response;
	}

	public async Task<List<CategorieIdDto>> GetAllCategoriesAsync()
	{
		var response = await _httpClient.GetAsync($"{_apiTestUrl}/CategorieWorkshop/GetAllCategories");
		if (response.IsSuccessStatusCode)
		{
			var categories = await response.Content.ReadFromJsonAsync<List<CategorieIdDto>>();
			return categories;
		}
		return new List<CategorieIdDto>();
	}

	public async Task<(List<GetAllQuestionsFromUserDto> Result, HttpStatusCode StatusCode)> GetAllQuestionsFromUser(string userId)
	{
		var response = await _httpClient.GetAsync($"{_apiTestUrl}/QuestionWorkshop/GetAllQuestionsFromUser?userId={userId}");

		if(response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<List<GetAllQuestionsFromUserDto>>();
			return (result, response.StatusCode);
		}
		return (null, response.StatusCode);
	}

	public async Task<(GetQuestionForEditingDto Result, HttpStatusCode StatusCode)> GetQuestionForEditing(int questionId)
	{
		var response = await _httpClient.GetAsync($"{_apiTestUrl}/QuestionWorkshop/GetQuestion?id={questionId}");

		if(response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<GetQuestionForEditingDto>();
			return (result, response.StatusCode);
		}
		return (null, response.StatusCode);
	}

	public async Task<HttpResponseMessage> UpdateQuestion(GetQuestionForEditingDto modifiedQuestion)
	{
		var json = System.Text.Json.JsonSerializer.Serialize(modifiedQuestion);
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		var response = await _httpClient.PutAsync($"{_apiTestUrl}/QuestionWorkshop/UpdateQuestion", content);
		return response;
	}

	public async Task<HttpResponseMessage> CreateFeedbackForQuestion(CreateQuizQuestionFeedbackDto feedbackObj)
	{
		var json = System.Text.Json.JsonSerializer.Serialize(feedbackObj);
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		var response = await _httpClient.PostAsync($"{_apiTestUrl}/QuestionWorkshop/CreateFeedbackForQuestion", content);
		return response;
	}

	public async Task<(List<GetQuizQuestionFeedbackDto> Result, HttpStatusCode StatusCode)> GetQuizQuestionFeedback(int questionId)
	{
		var response = await _httpClient.GetAsync($"{_apiTestUrl}/QuestionWorkshop/GetQuestionFeedbacks?questionId={questionId}");

		if(response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<List<GetQuizQuestionFeedbackDto>>();
			return (result, response.StatusCode);
		}
		return (null, response.StatusCode);
	}
}