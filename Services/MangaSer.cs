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

        // Пошук 5 результатів
        public async Task<MangaSearchResponse> SearchMangaAsync(string title)
        {
            var response = await _client.GetAsync($"https://api.jikan.moe/v4/manga?q={title}&limit=5");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MangaSearchResponse>(body);
        }

        // Отримання деталей конкретної манги
        public async Task<MangaFullResponse> GetMangaByIdAsync(int malId)
        {
            var response = await _client.GetAsync($"https://api.jikan.moe/v4/manga/{malId}/full");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MangaFullResponse>(body);
        }
        public async Task<MangaSearchResponse> SearchMangaByGenreAsync(int genreId)
        {
            var response = await _client.GetAsync($"https://api.jikan.moe/v4/manga?genres={genreId}&order_by=score&sort=desc&limit=5");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MangaSearchResponse>(body);
        }
    }
}