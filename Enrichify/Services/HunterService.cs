using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Enrichify.Services
{
    public class HunterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public HunterService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Hunter:ApiKey"]; // reads from appsettings.json
        }

        public async Task<string> FindEmail(string domain, string firstName, string lastName)
        {
            var url = $"https://api.hunter.io/v2/email-finder?domain={domain}&first_name={firstName}&last_name={lastName}&api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            return doc.RootElement.GetProperty("data").GetProperty("email").GetString();
        }
    }
}
