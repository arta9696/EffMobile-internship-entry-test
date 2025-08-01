using AdvertisingPlatform.Models;
using AdvertisingPlatform.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformsController : Controller
    {
        private readonly AdvertisingPlatformService _service;

        public PlatformsController(AdvertisingPlatformService service)
        {
            _service = service;
        }

        [HttpPost("load")]
        public async Task<ActionResult<LoadResult>> LoadPlatforms(IFormFile file)
        {
            var result = await _service.LoadPlatformsFromFile(file);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        [HttpGet("search")]
        public ActionResult<SearchResult> SearchPlatforms([FromQuery] string location)
        {
            var result = _service.SearchPlatforms(location);
            return Ok(result);
        }
    }
}
