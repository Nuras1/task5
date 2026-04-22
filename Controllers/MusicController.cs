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
        public IActionResult Audio(int seed, int page)
        {
            var bytes = _audio.Generate(seed, page);
            return File(bytes, "audio/wav");
        }

        [HttpGet("cover")]
        public async Task<IActionResult> GetCover(string title, string artist, int seed)
        {
            var bytes = await _cover.Generate(title, artist, seed);
            return File(bytes, "image/png");
        }
    }
}