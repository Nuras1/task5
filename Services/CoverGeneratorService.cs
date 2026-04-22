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
            Console.WriteLine($"COVERS PATH: {path}");

            Console.WriteLine($"FINAL PATH: {path}");
            Console.WriteLine($"EXISTS: {Directory.Exists(path)}");

            if (Directory.Exists(path))
            {
                foreach (var f in Directory.GetFiles(path))
                    Console.WriteLine($"FOUND FILE: {f}");
            }

            var files = Directory.GetFiles(path);

            if (files.Length == 0)
                throw new Exception("No images in covers folder");

            var index = (int)(Math.Abs(seed) % files.Length);
            var file = files[index];

            using var image = await Image.LoadAsync<Rgba32>(file);

            image.Mutate(ctx =>
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(300, 300),
                    Mode = ResizeMode.Crop
                });
            });

            image.Mutate(ctx =>
            {
                ctx.GaussianBlur(2);
                ctx.Brightness(0.85f);
                ctx.Fill(Color.Black.WithAlpha(0.35f));
            });

            var fontPath = Path.Combine(_env.ContentRootPath, "wwwroot", "fonts", "Roboto-Regular.ttf");

            var collection = new FontCollection();
            var family = SystemFonts.Families.First();

            var titleFont = family.CreateFont(22, FontStyle.Bold);
            var artistFont = family.CreateFont(16);

            image.Mutate(ctx =>
            {
                ctx.DrawText(title, titleFont, Color.White, new PointF(15, 20));
                ctx.DrawText(artist, artistFont, Color.LightGray, new PointF(15, 270));
            });
            Console.WriteLine("FILES:");
            foreach (var f in files)
            {
                Console.WriteLine(f);
            }
            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);

            return ms.ToArray();
        }
    }
}