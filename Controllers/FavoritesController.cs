using Microsoft.AspNetCore.Mvc;
using WebApi.Services;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly AnimeSer _publicApiService;
        private readonly FavorDbService _dbService;

        public FavoritesController(AnimeSer publicApiService, FavorDbService dbService)
        {
            _publicApiService = publicApiService;
            _dbService = dbService;
        }

        [HttpGet("search/{name}")]
        public async Task<IActionResult> GetArrayFromPublicApi(string name)
        {
            var result = await _publicApiService.SearchAnimeAsync(name);

            if (result == null || result.data == null || result.data.Length == 0)
            {
                return NotFound("Дані не знайдено");
            }

            var sorted = result.data.OrderByDescending(a => a.score).ToList();
            return Ok(sorted);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFromDatabase()
        {
            var favoritesList = await _dbService.GetAllFavoritesAsync();
            return Ok(favoritesList);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInDatabase([FromBody] favorModel newItem)
        {
            if (newItem == null) return BadRequest("Неправильні дані");

            await _dbService.InsertFavoriteAsync(newItem);
            return Ok("Аніме успішно збережено в базу даних!");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInDatabase(int id, [FromBody] favorModel updatedItem)
        {
            if (updatedItem == null) return BadRequest("Неправильні дані");

            bool isUpdated = await _dbService.UpdateFavoriteAsync(id, updatedItem);

            if (!isUpdated)
            {
                return NotFound("Запис з таким ID не знайдено в базі даних");
            }

            return Ok("Запис успішно оновлено!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFromDatabase(int id)
        {
            bool isDeleted = await _dbService.DeleteFavoriteAsync(id);

            if (!isDeleted)
            {
                return NotFound("Запис з таким ID не знайдено в базі даних");
            }

            return NoContent();
        }
    }
}