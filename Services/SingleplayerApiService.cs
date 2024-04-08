using Newtonsoft.Json;
using Project_Quizz_Frontend.Models;
using System.Net;
using System.Text;

namespace Project_Quizz_Frontend.Services
{
    public class SingleplayerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://trusting-black.188-40-219-72.plesk.page/api";
        private readonly string _apiTestUrl = "https://localhost:7241/api";

        public SingleplayerApiService(HttpClient httpClient)
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

            if (response.IsSuccessStatusCode)
            {
                var responseConent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<Dictionary<string, int>>(responseConent);
                var singleQuizId = responseJson["singleQuizId"];
                quizSessionResponse.CreatedQuizSessionId = singleQuizId;
            }

            return quizSessionResponse;
        }

        public async Task<(GetQuizQuestionDto Result, HttpStatusCode StatusCode)> GetQuestionForSingleQuiz(int quizId, string userId)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/SingleQuizWorkshop/GetQuestionFromQuizSession?quizId={quizId}&userId={userId}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GetQuizQuestionDto>();
                return (result, response.StatusCode);
            }
            return (null, response.StatusCode);
        }

        public async Task<HttpResponseMessage> UpdateSingleQuizSession(UpdateSingleQuizSessionDto updateSessionObj)
        {
            var url = $"{_apiBaseUrl}/SingleQuizWorkshop/UpdateSingleQuizSession";

            var json = JsonConvert.SerializeObject(updateSessionObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content);

            return response;
        }

        public async Task<(GetResultFromSingleQuizDto Result, HttpStatusCode StatusCode)> GetResultFromSingleQuiz(int quizId, string userId)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/SingleQuizWorkshop/GetResultFromSingleQuiz?quizId={quizId}&userId={userId}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GetResultFromSingleQuizDto>();
                return (result, response.StatusCode);
            }
            return (null, response.StatusCode);
        }
        public async Task<(int result, HttpStatusCode StatusCode)> GetSingleplayerNotificationsFromUser(string userId)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Notifications/GetOpenSingleplayerNotifications?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var desResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var count = (int)desResponse.count;
                return (count, response.StatusCode);
            }

            return (0, response.StatusCode);
        }

        public async Task<(List<GetSingleQuizzesFromUserDto> Result, HttpStatusCode StatusCode)> GetSingleQuizzesFromUser(string userId)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/SingleQuizWorkshop/GetSingleQuizzesFromUser?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<GetSingleQuizzesFromUserDto>>();
                return (result, response.StatusCode);
            }
            return (null, response.StatusCode);
        }
    }
}
