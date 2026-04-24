using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimeController : ControllerBase
    {
        private readonly AnimeSer _animeService; //сервіс для роботи з API Jikan

        public AnimeController(AnimeSer animeService)
        {
            _animeService = animeService; //підключаємо сервіс через конструктор
        }

        [HttpGet("search/{name}")]
        public async Task<IActionResult> Search(string name)
        {
            var result = await _animeService.SearchAnimeAsync(name); //шукаємо аніме за назвою

            if (result == null || result.data == null || result.data.Length == 0)
            {
                return NotFound("Аніме не знайдено"); 
            }

            var sortedAnime = result.data.OrderByDescending(a => a.score).ToList(); //сортуємо за рейтингом від найвищого

            return Ok(sortedAnime); 
        }

        [HttpGet("{id}/main-characters")]
        public async Task<IActionResult> GetMainCharacters(int id)
        {
            var result = await _animeService.GetCharactersAsync(id); //всі персонажів за ID аніме

            if (result == null || result.data == null)
            {
                return NotFound(); 
            }

            var mainChars = result.data.Where(c => c.role == "Main").ToList(); //сортування для головних персонажів (щоб їх виводило тільки)

            return Ok(mainChars); 
        }

        [HttpGet("{id}/themes")]
        public async Task<IActionResult> GetThemes(int id)
        {
            var result = await _animeService.GetThemesAsync(id); //опенінг за ID аніме

            if (result == null || result.data == null || result.data.openings == null)
            {
                return NotFound("Опенінгів для цього аніме не знайдено");
            }

            return Ok(result.data.openings);
        }
    }
}