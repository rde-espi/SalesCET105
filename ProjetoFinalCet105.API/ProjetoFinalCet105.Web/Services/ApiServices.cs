using System.Net.Http.Headers;

namespace ProjetoFinalCet105.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ProjetoFinalApi");
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            return await _httpClient.GetFromJsonAsync<T>(endpoint);
        }
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);

            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<T?> GetAuthenticatedAsync<T>(string endpoint)
        {
            var token = _httpContextAccessor
                .HttpContext?
                .Session
                .GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return default;
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}

