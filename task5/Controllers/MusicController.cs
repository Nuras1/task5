using Microsoft.AspNetCore.Mvc;
using task5.Models;
using task5.Services;

namespace task5.Controllers
{
    [ApiController]
    [Route("api/music")]
    public class MusicController : ControllerBase
    {
        private readonly MusicGeneratorService _music;
        private readonly AudioGeneratorService _audio;
        private readonly CoverGeneratorService _cover;

        public MusicController(
            MusicGeneratorService music,
            AudioGeneratorService audio,
            CoverGeneratorService cover)
        {
            _music = music;
            _audio = audio;
            _cover = cover;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] RequestParams param)
        {
            return Ok(_music.Generate(param));
        }

        [HttpGet("audio")]
        public IActionResult Audio(int seed)
        {
            return File(_audio.Generate(seed), "audio/wav");
        }

        [HttpGet("cover")]
        public async Task<IActionResult> GetCover(string title, string artist, int seed)
        {
            var service = new CoverGeneratorService();
            var bytes = await service.Generate(title, artist, seed);

            return File(bytes, "image/png");
        }

    }
}
