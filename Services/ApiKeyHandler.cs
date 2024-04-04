namespace Project_Quizz_Frontend.Services
{
    public class ApiKeyHandler : DelegatingHandler
    {
        private readonly string _apiKey;

        public ApiKeyHandler(string apiKey)
        {
            _apiKey = apiKey;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("apiKey", _apiKey);

			Console.WriteLine($"Request Headers: {request.Headers}"); // Zum Debuggen

			return await base.SendAsync(request, cancellationToken);
        }
    }
}
