using Microsoft.AspNetCore.Mvc;
using WebApi.Services;
using System.Linq;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MangaController : ControllerBase
    {
        private readonly MangaSer _mangaService;

        public MangaController(MangaSer mangaService)
        {
            _mangaService = mangaService; 
        }

        [HttpGet("search/{title}")]
        public async Task<IActionResult> Search(string title)
        {
            var result = await _mangaService.SearchMangaAsync(title); 

            if (result == null || result.data == null || result.data.Length == 0)
            {
                return NotFound("Мангу не знайдено"); 
            }

            var sortedManga = result.data.OrderByDescending(m => m.score).ToList();

            return Ok(sortedManga);
        }
    }
}