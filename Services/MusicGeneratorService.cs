using Bogus;
using task5.Models;
using System.Text.Json;

namespace task5.Services
{
    public class MusicGeneratorService
    {
        public List<Song> Generate(RequestParams param)
        {
            var baseSeed = SeedHelperService.Combine(param.Seed, param.Page);

            var fakerLocale = GetLocale(param.Region);
            var localeData = LoadLocaleData(param.Region);

            var result = new List<Song>();

            for (int i = 0; i < 20; i++)
            {
                int itemSeed = SeedHelperService.Combine(param.Seed, param.Page, i);

                Randomizer.Seed = new Random(itemSeed);
                var faker = new Faker(fakerLocale);

                var dataRng = new Random(itemSeed);
                var likesRng = new Random(itemSeed + 10000);

                string title = faker.Commerce.ProductName();
                string artist = dataRng.NextDouble() > 0.5
                                ? faker.Name.FullName()
                                : faker.Company.CompanyName();

                string album = GenerateAlbum(faker, dataRng);
                string genre = GenerateGenre(faker);

                var song = new Song
                {
                    Id = (param.Page - 1) * 20 + i + 1,
                    Title = title,
                    Artist = artist,
                    Album = album,
                    Genre = genre,

                    Likes = FractionalHelperService.Generate(param.LikesAvg, likesRng),

                    CoverUrl = $"/api/music/cover?title={title}&artist={artist}&seed={param.Seed}",
                    AudioUrl = BuildAudioUrl(itemSeed, param.Page),

                    Lyrics = GenerateLyrics(localeData, dataRng)
                };

                result.Add(song);
            }

            return result;
        }

        private string GetLocale(string region)
        {
            return region switch
            {
                "de" => "de",
                _ => "en"
            };
        }

        private LyricsData LoadLocaleData(string region)
        {
            var locale = region == "de" ? "de-DE" : "en-US";
            var path = Path.Combine("Data", $"{locale}.json");

            if (!File.Exists(path))
                throw new Exception($"Locale file not found: {path}");

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<LyricsData>(json)!;
        }

        private string GenerateAlbum(Faker faker, Random rng)
        {
            return rng.NextDouble() > 0.5
                ? faker.Company.CompanyName()
                : faker.PickRandom("Single", "EP");
        }

        private string GenerateGenre(Faker faker)
        {
            return faker.Music.Genre();
        }

        private string BuildCoverUrl(string title, string artist, int seed)
        {
            return $"/api/music/cover?title={Uri.EscapeDataString(title)}&artist={Uri.EscapeDataString(artist)}&seed={seed}";
        }

        private string BuildAudioUrl(int seed, int page)
        {
            return $"/api/music/audio?seed={seed}&page={page}";
        }

        private List<string> GenerateLyrics(LyricsData data, Random rng)
        {
            var lines = new List<string>();

            int total = 8 + rng.Next(8);
            var hook = GenerateLine(data, rng);

            for (int i = 0; i < total; i++)
            {
                string line;

                if (i % 4 == 3)
                {
                    line = hook;
                }
                else
                {
                    int type = rng.Next(4);

                    line = type switch
                    {
                        0 => GenerateLine(data, rng),
                        1 => $"{data.Feel} {Pick(data.Subjects, rng)} when {GenerateLine(data, rng)}",
                        2 => GenerateShortLine(data, rng),
                        _ => $"{GenerateLine(data, rng)}, {GenerateShortLine(data, rng)}"
                    };
                }

                lines.Add(Capitalize(line));
            }

            return lines;
        }

        private string GenerateLine(LyricsData data, Random rng)
        {
            return $"{Pick(data.Subjects, rng)} {Pick(data.Verbs, rng)} {Pick(data.Places, rng)}";
        }

        private string GenerateShortLine(LyricsData data, Random rng)
        {
            return $"{Pick(data.Subjects, rng)} {Pick(data.Verbs, rng)}";
        }

        private string Pick(string[] arr, Random rng)
        {
            return arr[rng.Next(arr.Length)];
        }

        private string Capitalize(string text)
        {
            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }
}