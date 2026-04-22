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
        private async Task<byte[]> GenerateWithoutText(string file)
        {
            using var image = await Image.LoadAsync<Rgba32>(file);

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

            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);

            return ms.ToArray();
        }
        public async Task<byte[]> Generate(string title, string artist, long seed)
        {
            try
            {
                var path = Path.Combine(_env.WebRootPath, "covers");

                if (!Directory.Exists(path))
                    throw new Exception($"COVERS FOLDER NOT FOUND: {path}");

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

                    ctx.GaussianBlur(2);
                    ctx.Brightness(0.85f);
                    ctx.Fill(Color.Black.WithAlpha(0.35f));
                });

                // ===== ШРИФТ =====
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
                    Console.WriteLine("NO FONTS → fallback");
                    return await GenerateWithoutText(file);
                }

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
                    Console.WriteLine("NO FONTS → fallback");
                    return await GenerateWithoutText(file);
                }


                // ===== РИСУЕМ ТЕКСТ =====
                var titleFont = family!.CreateFont(22, FontStyle.Bold);
                var artistFont = family!.CreateFont(16);

                image.Mutate(ctx =>
                {
                    ctx.DrawText(title, titleFont, Color.White, new PointF(15, 20));
                    ctx.DrawText(artist, artistFont, Color.LightGray, new PointF(15, 260));
                });

                using var ms = new MemoryStream();
                await image.SaveAsPngAsync(ms);

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine("COVER GENERATION ERROR: " + ex.Message);

                // fallback: просто первая картинка
                var fallbackPath = Path.Combine(_env.WebRootPath, "covers");
                var fallbackFile = Directory.GetFiles(fallbackPath).First();

                using var fallbackImage = await Image.LoadAsync<Rgba32>(fallbackFile);
                using var ms = new MemoryStream();
                await fallbackImage.SaveAsPngAsync(ms);

                return ms.ToArray();
            }

        }
    }
}