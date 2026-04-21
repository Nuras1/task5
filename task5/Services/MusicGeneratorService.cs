using Bogus;
using task5.Models;
using System.Text.Json;

namespace task5.Services
{
    public class MusicGeneratorService
    {
        public List<Song> Generate(RequestParams param)
        {
            var baseSeed = param.Seed + param.Page * 1000;

            Randomizer.Seed = new Random((int)baseSeed);

            // 🔥 Faker (Bogus)
            var fakerLocale = param.Region == "de" ? "de" : "en";
            var faker = new Faker(fakerLocale);

            // 🔥 JSON локализация
            var localeFile = param.Region == "de" ? "de-DE" : "en-US";
            var localeData = LoadLocale(localeFile);

            var list = new List<Song>();

            for (int i = 0; i < 20; i++)
            {
                var dataRng = new Random((int)(baseSeed + i));
                var likesRng = new Random((int)(baseSeed + 10000 + i));

                var title = faker.Commerce.ProductName();
                var artist = faker.Name.FullName();

                var album = dataRng.NextDouble() > 0.5
                    ? faker.PickRandom("Single", "EP")
                    : faker.Company.CompanyName();

                var genre = faker.PickRandom(
                    faker.Music.Genre(),
                    faker.Commerce.Categories(1)[0]
                );

                var song = new Song
                {
                    Id = (param.Page - 1) * 20 + i + 1,
                    Title = title,
                    Artist = artist,
                    Album = album,
                    Genre = genre,

                    Likes = FractionalHelperService.Generate(param.LikesAvg, likesRng),

                    CoverUrl = $"/api/music/cover?title={Uri.EscapeDataString(title)}&artist={Uri.EscapeDataString(artist)}&seed={baseSeed + i}",
                    AudioUrl = $"/api/music/audio?seed={baseSeed + i}",

                    Description = faker.Lorem.Sentence(),

                    // 🔥 ЛОКАЛИЗОВАННЫЕ ТЕКСТЫ
                    Lyrics = GenerateLyrics(localeData, dataRng)
                };

                list.Add(song);
            }

            return list;
        }

        private LyricsData LoadLocale(string locale)
        {
            var path = Path.Combine("Data", $"{locale}.json");

            if (!File.Exists(path))
                throw new Exception($"Locale file not found: {path}");

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<LyricsData>(json);
        }

        private List<string> GenerateLyrics(LyricsData data, Random rng)
        {
            var lyrics = new List<string>();

            int totalLines = 8 + rng.Next(8);

            // 🎤 создаём припев (hook)
            var hook = GenerateLine(data, rng);

            for (int i = 0; i < totalLines; i++)
            {
                string line;

                // 🎶 каждая 4-я строка — припев
                if (i % 4 == 3)
                {
                    line = hook;
                }
                else
                {
                    int type = rng.Next(4);

                    switch (type)
                    {
                        case 0:
                            line = GenerateLine(data, rng);
                            break;

                        case 1:
                            line = $"{data.Feel} {Pick(data.Subjects, rng)} when {GenerateLine(data, rng)}";
                            break;

                        case 2:
                            line = GenerateShortLine(data, rng);
                            break;

                        default:
                            line = GenerateLine(data, rng) + ", " + GenerateShortLine(data, rng);
                            break;
                    }
                }

                lyrics.Add(Cap(line));
            }

            return lyrics;
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

        private string Cap(string s)
        {
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}