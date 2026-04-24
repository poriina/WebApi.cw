using Newtonsoft.Json;
using WebApi.Models;

namespace WebApi.Services
{
    public class AnimeSer
    {
        private readonly HttpClient _client;

        public AnimeSer()
        {
            _client = new HttpClient(); 
        }

        public async Task<AnimeSearchResponse> SearchAnimeAsync(string animeName)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.jikan.moe/v4/anime?q={animeName}&limit=5")
            };

            using (var response = await _client.SendAsync(request)) //запит до арі
            {
                response.EnsureSuccessStatusCode(); 
                var body = await response.Content.ReadAsStringAsync(); 
                return JsonConvert.DeserializeObject<AnimeSearchResponse>(body); //перетворюємо json-текст у наші моделі
            }
        }

        public async Task<AnimeCharactersResponse> GetCharactersAsync(int animeId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.jikan.moe/v4/anime/{animeId}/characters") 
            };

            using (var response = await _client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AnimeCharactersResponse>(body);
            }
        }

        public async Task<AnimeThemesResponse> GetThemesAsync(int animeId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.jikan.moe/v4/anime/{animeId}/themes") 
            };

            using (var response = await _client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AnimeThemesResponse>(body);
            }
        }
    }
}