using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace task5.Services
{
    public class CoverGeneratorService
    {
        public async Task<byte[]> Generate(string title, string artist, int seed)
        {
            var url = $"https://picsum.photos/300/300?random={seed}";

            using var http = new HttpClient();
            var stream = await http.GetStreamAsync(url);

            using var image = await Image.LoadAsync<Rgba32>(stream);

            // затемнение (overlay)
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.Black.WithAlpha(0.4f));
            });

            var titleFont = SystemFonts.CreateFont("Arial", 22, FontStyle.Bold);
            var artistFont = SystemFonts.CreateFont("Arial", 16, FontStyle.Regular);

            image.Mutate(ctx =>
            {
                // title
                ctx.DrawText(title, titleFont, Color.White, new PointF(15, 200));

                // artist
                ctx.DrawText(artist, artistFont, Color.LightGray, new PointF(15, 240));
            });

            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);

            return ms.ToArray();
        }
    }
}