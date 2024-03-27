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

	public async Task<HttpResponseMessage> CreateQuestionAsync(QuizQuestionViewModel questionViewModel)
	{
		var json = JsonSerializer.Serialize(questionViewModel);
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		var response = await _httpClient.PostAsync($"{_apiBaseUrl}/QuestionWorkshop/CreateQuestion", content);
		return response;
	}
}