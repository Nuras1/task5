using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace task5.Services
{
    public class CoverGeneratorService
    {
        private readonly IWebHostEnvironment _env;

        public CoverGeneratorService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<byte[]> Generate(string title, string artist, long seed)
        {
            var path = Path.Combine(_env.WebRootPath, "covers");

            if (!Directory.Exists(path))
                throw new Exception("covers not found");

            var files = Directory.GetFiles(path);

            if (files.Length == 0)
                throw new Exception("no images");

            var index = (int)(Math.Abs(seed) % files.Length);
            var file = files[index];

            return await File.ReadAllBytesAsync(file);
        }

        private async Task<byte[]> SaveImage(Image<Rgba32> image)
        {
            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            return ms.ToArray();
        }
    }
}