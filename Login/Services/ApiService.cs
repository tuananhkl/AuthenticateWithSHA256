using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyProjectName.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> LoginAsync(string username, string password)
        {
            var content = new StringContent(JsonSerializer.Serialize(new { Username = username, Password = password }), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("auth/login", content);

            return response;
        }

        public async Task<HttpResponseMessage> RegisterAsync(string username, string password)
        {
            var content = new StringContent(JsonSerializer.Serialize(new { Username = username, Password = password }), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("auth/register", content);

            return response;
        }
    }
}