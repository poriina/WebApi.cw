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
            var response = await _client.GetAsync($"https://api.jikan.moe/v4/anime?q={animeName}&limit=5");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AnimeSearchResponse>(body);
        }

        public async Task<AnimeFullResponse> GetAnimeByIdAsync(int malId)
        {
            var response = await _client.GetAsync($"https://api.jikan.moe/v4/anime/{malId}/full");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AnimeFullResponse>(body);
        }

        public async Task<AnimeCharactersResponse> GetCharactersAsync(int animeId)
        {
            var response = await _client.GetAsync($"https://api.jikan.moe/v4/anime/{animeId}/characters");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AnimeCharactersResponse>(body);
        }

        public async Task<AnimeThemesResponse> GetThemesAsync(int animeId)
        {
            var response = await _client.GetAsync($"https://api.jikan.moe/v4/anime/{animeId}/themes");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AnimeThemesResponse>(body);
        }
        public async Task<AnimeSearchResponse> SearchAnimeByGenreAsync(int genreId)
        {

            var response = await _client.GetAsync($"https://api.jikan.moe/v4/anime?genres={genreId}&order_by=score&sort=desc&limit=5");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AnimeSearchResponse>(body);
        }
    }
}