using Newtonsoft.Json;
using WebApi.Models;

namespace WebApi.Services
{
    public class MangaSer
    {
        private readonly HttpClient _client;

        public MangaSer()
        {
            _client = new HttpClient();
        }

        public async Task<MangaSearchResponse> SearchMangaAsync(string title)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.jikan.moe/v4/manga?q={title}&limit=5") 
            };

            using (var response = await _client.SendAsync(request)) 
            {
                response.EnsureSuccessStatusCode(); 
                var body = await response.Content.ReadAsStringAsync(); 
                return JsonConvert.DeserializeObject<MangaSearchResponse>(body);
            }
        }
    }
}