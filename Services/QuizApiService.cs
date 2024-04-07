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

	public async Task<CreateQuizSessionResponse> CreateSingleQuizSession(string userId, int categorieId)
	{
		var response = await _httpClient.PostAsync($"{_apiBaseUrl}/SingleQuizWorkshop/CreateSingleQuizSession?userId={userId}&categorieId={categorieId}", null);

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
		var response = await _httpClient.GetFromJsonAsync<GetQuizQuestionDto>($"{_apiBaseUrl}/SingleQuizWorkshop/GetQuestionFromQuizSession?quizId={quizId}&userId={userId}");
		return response;
	}

	public async Task<HttpResponseMessage> UpdateSingleQuizSession(UpdateSingleQuizSessionDto updateSessionObj)
	{
		var url = $"{_apiBaseUrl}/SingleQuizWorkshop/UpdateSingleQuizSession";

		var json = JsonConvert.SerializeObject(updateSessionObj);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await _httpClient.PutAsync(url, content);

		return response;
	}

	public async Task<HttpResponseMessage> UpdateMultiQuizSession(UpdateMultiQuizSessionDto updateSessionObj)
	{
		var url = $"{_apiTestUrl}/MultiQuizWorkshop/UpdateMultiQuizSession";

		var json = JsonConvert.SerializeObject(updateSessionObj);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await _httpClient.PutAsync(url, content);

		return response;
	}

	public async Task<(GetResultFromSingleQuizDto Result, HttpStatusCode StatusCode)> GetResultFromSingleQuiz(int quizId, string userId)
	{
		var response = await _httpClient.GetAsync($"{_apiBaseUrl}/SingleQuizWorkshop/GetResultFromSingleQuiz?quizId={quizId}&userId={userId}");

		if(response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<GetResultFromSingleQuizDto>();
			return (result, response.StatusCode);
		}
		return (null, response.StatusCode);
	}

	public async Task<(GetResultFromMultiQuizDto Result, HttpStatusCode StatusCode)> GetResultFromMultiQuiz(int quizId, string userId)
	{
		var response = await _httpClient.GetAsync($"{_apiTestUrl}/MultiQuizWorkshop/GetResultFromMultiQuiz?quizId={quizId}&userId={userId}");

		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<GetResultFromMultiQuizDto>();
			return (result, response.StatusCode);
		}

		return (null, response.StatusCode);
	}

	public async Task<HttpResponseMessage> CreateQuestionAsync(Models.CreateQuizQuestionDto questionViewModel)
	{
		var json = System.Text.Json.JsonSerializer.Serialize(questionViewModel);
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		var response = await _httpClient.PostAsync($"{_apiBaseUrl}/QuestionWorkshop/CreateQuestion", content);
		return response;
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

	public async Task<(List<GetAllQuestionsFromUserDto> Result, HttpStatusCode StatusCode)> GetAllQuestionsFromUser(string userId)
	{
		var response = await _httpClient.GetAsync($"{_apiBaseUrl}/QuestionWorkshop/GetAllQuestionsFromUser?userId={userId}");

		if(response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<List<GetAllQuestionsFromUserDto>>();
			return (result, response.StatusCode);
		}
		return (null, response.StatusCode);
	}

	public async Task<(GetQuestionForEditingDto Result, HttpStatusCode StatusCode)> GetQuestionForEditing(int questionId)
	{
		var response = await _httpClient.GetAsync($"{_apiBaseUrl}/QuestionWorkshop/GetQuestion?id={questionId}");

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
		var response = await _httpClient.PutAsync($"{_apiBaseUrl}/QuestionWorkshop/UpdateQuestion", content);
		return response;
	}

	public async Task<CreateQuizSessionResponse> CreateMultiplayerQuizSession(string userOne, string userTwo, int categoryId)
	{
		var objectSession = new IniMultiplayerDtos
		{
			UserOne = userOne,
			UserTwo = userTwo,
			CategorieId = categoryId
		};
		var url = $"{_apiTestUrl}/MultiQuizWorkshop/CreateMultiQuizSession";

		var json = JsonConvert.SerializeObject(objectSession);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        var quizSessionResponse = new CreateQuizSessionResponse
        {
            HttpResponse = response,
        };

        if (response.IsSuccessStatusCode)
        {
            var responseConent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonConvert.DeserializeObject<Dictionary<string, int>>(responseConent);
            var multiQuizId = responseJson["multiQuizId"];
            quizSessionResponse.CreatedQuizSessionId = multiQuizId;
        }

        return quizSessionResponse;
    }

	public async Task<(GetQuizQuestionDto Result, HttpStatusCode StatusCode)> GetQuestionForMultiQuiz(int quizId, string userId)
	{
		var response = await _httpClient.GetAsync($"{_apiTestUrl}/MultiQuizWorkshop/GetQuestionFromMultiQuizSession?quizId={quizId}&userId={userId}");
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<GetQuizQuestionDto>();
			return (result, response.StatusCode);
		}
		return (null, response.StatusCode);
	}

	public async Task<(List<GetMultiQuizzesFromUserDto> Result, HttpStatusCode StatusCode)> GetMultiQuizzesFromUser(string userId)
	{
		var response = await _httpClient.GetAsync($"{_apiTestUrl}/MultiQuizWorkshop/GetMultiQuizzesFromUser?userId={userId}");
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<List<GetMultiQuizzesFromUserDto>>();
			return (result, response.StatusCode);
		}
		return (null, response.StatusCode);
	}
}