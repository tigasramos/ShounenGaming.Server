using Newtonsoft.Json;
using Serilog;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    /*
     * Rate Limits:
     * 5 per Second
     */
    internal abstract class MangaDexAbstractScrapper
    {
        protected string GetSearchQuery(int limit = 15, int offset = 0)
        {
            var languages = GetLanguage() == MangaTranslationEnumDTO.PT ? "availableTranslatedLanguage%5B%5D=pt-br&availableTranslatedLanguage%5B%5D=pt" : "availableTranslatedLanguage%5B%5D=en";
            return $"https://api.mangadex.org/manga?limit={limit}&offset={offset}&includedTagsMode=AND&excludedTagsMode=OR&{languages}&contentRating%5B%5D=safe&contentRating%5B%5D=suggestive&contentRating%5B%5D=erotica&order%5BlatestUploadedChapter%5D=desc&includes%5B%5D=cover_art";
        }

        public async Task<List<MangaSourceDTO>> SearchMangasUnified(string query)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Host", "api.mangadex.org");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");

                var response = await client.GetAsync(query);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<MangaDexResponse<List<MangaDexManga>>>(responseString);

                return responseObject?.Data
                    .Select(x => new MangaSourceDTO()
                    {
                        Url = x.Id,
                        ImageURL = $"https://mangadex.org/covers/{x.Id}/{x.Relationships.SingleOrDefault(d => d.Type == "cover_art")?.Attributes?.FileName}",
                        Name = x.Attributes.Title.En ?? x.Attributes.AltTitles.FirstOrDefault(t => t.En != null)?.En ?? x.Attributes.AltTitles.FirstOrDefault(t => t.Ja_ro != null)?.Ja_ro ?? "",
                        Source = GetMangaSourceEnumDTO()
                    }).ToList() ?? new List<MangaSourceDTO>();

            } 
            catch (Exception ex)
            {
                Log.Error($"MangasDex - SearchManga: {ex.Message}");
            }

            return new List<MangaSourceDTO>();
        }
        public string GetBaseURLForManga()
        {
            return "https://api.mangadex.org";
        }
        public abstract MangaSourceEnumDTO GetMangaSourceEnumDTO();
        public abstract MangaTranslationEnumDTO GetLanguage();
        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"https://api.mangadex.org/at-home/server/{urlPart}?forcePort443=true");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<MangaDexChapterPagesResponse>(responseString);

            return responseObject?.Chapter.Data.Select(i => responseObject.BaseUrl + "/data/" + responseObject.Chapter.Hash + "/" + i).ToList() ?? new List<string>();
        }
        public async Task<ScrappedManga> GetMangaByLanguage(string urlPart)
        {
            // 1 Request -> Manga
            // NChapters / 500 Requests -> Chapters 

            // Check Language
            var languages = GetLanguage() == MangaTranslationEnumDTO.PT ? "translatedLanguage%5B%5D=pt-br&translatedLanguage%5B%5D=pt" : "translatedLanguage%5B%5D=en";

            // Get Manga
            var client = new HttpClient();
            var mangaResponse = await client.GetAsync($"https://api.mangadex.org/manga/{urlPart}?includes%5B%5D=cover_art");
            mangaResponse.EnsureSuccessStatusCode();

            var mangaResponseString = await mangaResponse.Content.ReadAsStringAsync();
            var mangaResponseObject = JsonConvert.DeserializeObject<MangaDexResponse<MangaDexManga>>(mangaResponseString) ?? throw new Exception("MangasDex Failed Fetching Manga");
            
            var scrappedManga = new ScrappedManga()
            {
                Name = mangaResponseObject.Data.Attributes.Title.En,
                Description = GetLanguage() == MangaTranslationEnumDTO.PT && !string.IsNullOrEmpty(mangaResponseObject.Data.Attributes.Description.PtBr) ? mangaResponseObject.Data.Attributes.Description.PtBr : mangaResponseObject.Data.Attributes.Description.En,
                ImageURL = $"https://mangadex.org/covers/{urlPart}/{mangaResponseObject.Data.Relationships.FirstOrDefault(x => x.Type == "cover_art")?.Attributes?.FileName}" ?? "",
                Source = GetMangaSourceEnumDTO(),
            };

            // Get Chapters
            var limit = 500;
            var total = 0;
            var chapters = new List<ScrappedChapter>();
            do
            {
                var response = await client.GetAsync($"https://api.mangadex.org/manga/{urlPart}/feed?limit={limit}&offset={chapters.Count}&{languages}&contentRating%5B%5D=safe&contentRating%5B%5D=suggestive&contentRating%5B%5D=erotica&includeFutureUpdates=1&order%5BcreatedAt%5D=asc&order%5BupdatedAt%5D=asc&order%5BpublishAt%5D=asc&order%5BreadableAt%5D=asc&order%5Bvolume%5D=asc&order%5Bchapter%5D=asc");
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<MangaDexResponse<List<MangaDexChapter>>>(responseString);

                responseObject?.Data.ForEach(c => chapters.Add(new ScrappedChapter
                {
                    Link = c.Id,
                    Name = c.Attributes.Chapter,
                    ReleasedAt = c.Attributes.PublishAt
                }));

                total = responseObject?.Total ?? 0;

                await Task.Delay(500);
            } while (total > chapters.Count);

            scrappedManga.Chapters = chapters.DistinctBy(c => c.Name).ToList();

            return scrappedManga;
        }

        protected class MangaDexChapterPagesResponse
        {
            public string Result { get; set; }
            public string BaseUrl { get; set; }
            public MangaDexChapterPages Chapter { get; set; }
        }

        protected class MangaDexChapterPages
        {
            public string Hash { get; set; }
            public List<string> Data { get; set; }
            public List<string> DataSaver { get; set; }
        }

        protected class MangaDexMangaAttributes
        {
            public MangaDexTitle Title { get; set; }
            public List<MangaDexMangaAltTitle> AltTitles { get; set; }
            public MangaDexDescription Description { get; set; }
            public List<string> AvailableTranslatedLanguages { get; set; }
        }

        protected class MangaDexMangaAltTitle
        {
            public string? En { get; set; }

            [JsonProperty("ja-ro")]
            public string? Ja_ro { get; set; }
        }

        protected class MangaDexManga
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public MangaDexMangaAttributes Attributes { get; set; }
            public List<MangaDexRelationship> Relationships { get; set; }
        }
        protected class MangaDexChapter
        {
            public string Id { get; set; }
            public MangaDexChapterAttributes Attributes { get; set; }
        }
        protected class MangaDexChapterAttributes
        {
            public string Chapter { get; set; }
            public string ExternalUrl { get; set; }
            public DateTime PublishAt { get; set; }

        }
        protected class MangaDexRelationship
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public MangaDexRelationshipAttributes? Attributes { get; set; }
        }
        protected class MangaDexRelationshipAttributes
        {
            public string? Volume { get; set; }
            public string? FileName { get; set; }
        }

        protected class MangaDexDescription
        {
            public string En { get; set; }

            [JsonProperty("pt-br")]
            public string PtBr { get; set; }
        }

        protected class MangaDexResponse<T>
        {
            public string Result { get; set; }
            public string Response { get; set; }
            public T Data { get; set; }
            public int Limit { get; set; }
            public int Offset { get; set; }
            public int Total { get; set; }
        }

        protected class MangaDexTitle
        {
            public string En { get; set; }
        }
    }
}
