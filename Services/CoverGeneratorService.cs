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
            try
            {
                var path = Path.Combine(_env.WebRootPath, "covers");

                if (!Directory.Exists(path))
                    throw new Exception("covers folder not found");

                var files = Directory.GetFiles(path);

                if (files.Length == 0)
                    throw new Exception("no images");

                var index = (int)(Math.Abs(seed) % files.Length);
                var file = files[index];

                using var image = await Image.LoadAsync<Rgba32>(file);

                // базовая обработка
                image.Mutate(ctx =>
                {
                    ctx.Resize(new ResizeOptions
                    {
                        Size = new Size(300, 300),
                        Mode = ResizeMode.Crop
                    });

                    ctx.GaussianBlur(2);
                    ctx.Brightness(0.85f);
                    ctx.Fill(Color.Black.WithAlpha(0.35f));
                });

                FontFamily family;

                var fontPath = Path.Combine(_env.WebRootPath, "fonts", "Roboto-Regular.ttf");

                if (File.Exists(fontPath))
                {
                    var collection = new FontCollection();
                    family = collection.Add(fontPath);
                }
                else if (SystemFonts.Families.Any())
                {
                    family = SystemFonts.Families.First();
                }
                else
                {
                    Console.WriteLine("NO FONTS → fallback WITHOUT TEXT");
                    return await SaveImage(image);
                }

                var titleFont = family.CreateFont(22, FontStyle.Bold);
                var artistFont = family.CreateFont(16);

                image.Mutate(ctx =>
                {
                    ctx.DrawText(title, titleFont, Color.White, new PointF(15, 20));
                    ctx.DrawText(artist, artistFont, Color.LightGray, new PointF(15, 260));
                });

                return await SaveImage(image);
            }
            catch (Exception ex)
            {
                Console.WriteLine("COVER ERROR: " + ex.Message);

                // fallback — просто картинка без обработки
                var path = Path.Combine(_env.WebRootPath, "covers");
                var file = Directory.GetFiles(path).First();

                var bytes = await File.ReadAllBytesAsync(file);
                return bytes;
            }
        }

        private async Task<byte[]> SaveImage(Image<Rgba32> image)
        {
            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            return ms.ToArray();
        }
    }
}