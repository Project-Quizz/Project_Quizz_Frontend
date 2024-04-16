using Newtonsoft.Json;
using Project_Quizz_Frontend.Models;
using System.Net;
using System.Text;

namespace Project_Quizz_Frontend.Services
{
    /// <summary>
    /// The API service for the multiplayer quiz
    /// </summary>
    public class MultiplayerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://trusting-black.188-40-219-72.plesk.page/api";
        private readonly string _apiTestUrl = "https://localhost:7241/api";

        public MultiplayerApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// To update the multiplayer quiz session
        /// </summary>
        /// <param name="updateSessionObj">The object of the quiz</param>
        /// <returns>Return the response as HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> UpdateMultiQuizSession(UpdateMultiQuizSessionDto updateSessionObj)
        {
            var url = $"{_apiTestUrl}/MultiQuizWorkshop/UpdateMultiQuizSession";

            var json = JsonConvert.SerializeObject(updateSessionObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content);

            return response;
        }

        /// <summary>
        /// To get the result from the multiplayer quiz
        /// </summary>
        /// <param name="quizId">The quiz id</param>
        /// <param name="userId">The user id</param>
        /// <returns>Return the response from the API</returns>
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

        /// <summary>
        /// Create a multiplayer quiz session
        /// </summary>
        /// <param name="userOne">The User id</param>
        /// <param name="userTwo">The User id</param>
        /// <param name="categoryId">The categorie id for the quiz</param>
        /// <returns>Return the response from the API</returns>
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

        /// <summary>
        /// Get the question for the multiplayer quiz
        /// </summary>
        /// <param name="quizId">The quiz id</param>
        /// <param name="userId">Teh user id</param>
        /// <returns>Return the response from the API</returns>
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

        /// <summary>
        /// Get all multiplayer quiz sessions from user
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <returns>Return the response from the API</returns>
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

        /// <summary>
        /// Get the notification for the multiplayer quiz
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <returns>Return the response from the API</returns>
        public async Task<(int result, HttpStatusCode StatusCode)> GetMultiplayerNotificationsFromUser(string userId)
        {
            var response = await _httpClient.GetAsync($"{_apiTestUrl}/Notifications/GetOpenMultiplayerNotifications?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var desResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var count = (int)desResponse.count;
                return (count, response.StatusCode);
            }

            return (0, response.StatusCode);
        }
    }
}
