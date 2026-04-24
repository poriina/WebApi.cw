using Microsoft.AspNetCore.Mvc;
using WebApi.Services;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly AnimeSer _publicApiService; // сервіс для пошуку в інтернеті
        private readonly FavorSer _jsonService; // сервіс для нашого файлу fav.json

        public FavoritesController(AnimeSer publicApiService, FavorSer jsonService)
        {
            _publicApiService = publicApiService;
            _jsonService = jsonService; // підключаємо обидва сервіси
        }

        [HttpGet("search/{name}")]
        public async Task<IActionResult> GetArrayFromPublicApi(string name)
        {
            var result = await _publicApiService.SearchAnimeAsync(name); // шукаємо дані через jikan api

            if (result == null || result.data == null || result.data.Length == 0)
            {
                return NotFound("Дані не знайдено"); // віддаємо 404, якщо порожньо
            }

            var sorted = result.data.OrderByDescending(a => a.score).ToList(); // сортуємо за рейтингом

            return Ok(sorted); // повертаємо результат (статус 200)
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpecificFromPublicApi(int id)
        {
            var result = await _publicApiService.GetCharactersAsync(id); // шукаємо персонажів за id

            if (result == null || result.data == null)
            {
                return NotFound("Запис не знайдено"); // якщо немає персонажів - 404
            }

            return Ok(result.data);
        }

        [HttpPost]
        public IActionResult CreateInJsonFile([FromBody] favorModel newItem)
        {
            if (newItem == null) return BadRequest("Неправильні дані"); // перевіряємо чи не порожній запит

            var created = _jsonService.AddItem(newItem); // зберігаємо запис у файл

            return Created("", created); // повертаємо статус 201 (успішно створено)
        }

        [HttpPut("{id}")]
        public IActionResult UpdateInJsonFile(int id, [FromBody] favorModel updatedItem)
        {
            if (updatedItem == null) return BadRequest("Неправильні дані");

            bool isUpdated = _jsonService.UpdateItem(id, updatedItem); // пробуємо оновити запис у файлі

            if (!isUpdated)
            {
                return NotFound("Запис не знайдено у файлі"); // якщо такого id немає, видаємо 404
            }

            return Ok(updatedItem); // повертаємо оновлені дані
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteFromJsonFile(int id)
        {
            bool isDeleted = _jsonService.DeleteItem(id); // пробуємо видалити запис

            if (!isDeleted)
            {
                return NotFound("Запис не знайдено у файлі"); // якщо не знайшли - 404
            }

            return NoContent(); // повертаємо 204 (успішно видалено)
        }
    }
}