using Newtonsoft.Json;
using Serilog;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Text.RegularExpressions;
using System.Web;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    public class SaikaiScansScrapper : IBaseMangaScrapper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SaikaiScansScrapper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public string GetBaseURLForManga()
        {
            return "https://s3-alpha.saikaiscans.net/";
        }

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"https://api.saikaiscans.net/api/releases/{urlPart}?reading=1&pageview=1&format=2&relationships=story.separators.releases,story.separatorType,editors,translators,revisors,checkers,releaseImages&cache=1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<SaikaiResponse<SaikaiChapterData>>(responseString) ?? throw new Exception();

            var images = new List<string>();
            foreach (var image in responseObject.Data.ReleaseImages)
            {
                images.Add($"https://s3-alpha.saikaiscans.net/{image.Image}");
            }
            return images;
        }

        public MangaTranslationEnumDTO GetLanguage()
        {
            return MangaTranslationEnumDTO.PT;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"https://api.saikaiscans.net/api/stories?pageview=1&relationships=firstRelease,tags,genres,associatedNames,authors.user,artists.user,translators,revisors,checkers,editors,separatorType,language,status,galleries,curiosities,separators.releases&first=true&slug={urlPart}");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<SaikaiResponse<SaikaiMangaData>>(responseString) ?? throw new Exception();
            
            return new ScrappedManga
            {
                Name = responseObject.Data.Title,
                ImageURL = $"https://s3-alpha.saikaiscans.net/{responseObject.Data.Image}",
                Description = Regex.Replace(HttpUtility.HtmlDecode(responseObject.Data.Synopsis), "<.*?>", string.Empty),
                Chapters = responseObject.Data.Separators.SelectMany(c => c.Releases).Select(s => new ScrappedChapter 
                { 
                    Name = s.Chapter,
                    Link = s.Id.ToString() ?? string.Empty,
                    ReleasedAt = s.PublishedAt
                }).ToList(),
                Source = GetMangaSourceEnumDTO()
            };
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.SAIKAI_SCANS;
        }
        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync($"https://api.saikaiscans.net/api/stories?&q={name}&paginate=0&limit=25&format=2&relationships=language,format");
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<SaikaiResponse<List<SaikaiSearchData>>>(responseString);

                return responseObject?.Data
                    .Select(x => new MangaSourceDTO()
                    {
                        Url = x.Slug,
                        ImageURL = $"https://s3-alpha.saikaiscans.net/{x.Image}",
                        Name = x.Title,
                        Source = GetMangaSourceEnumDTO()
                    }).ToList() ?? new List<MangaSourceDTO>();

            }
            catch (Exception ex)
            {
                Log.Error($"SaikaiScans - SearchManga: {ex.Message}");
            }

            return new List<MangaSourceDTO>();
        }


        public class SaikaiChapterData
        {
            public int? Id { get; set; }
            public object Title { get; set; }
            public string Chapter { get; set; }
            public object Source { get; set; }
            public string Slug { get; set; }

            [JsonProperty("published_at")]
            public DateTime? PublishedAt { get; set; }

            [JsonProperty("release_images")]
            public List<SaikaiReleaseImage> ReleaseImages { get; set; }

            [JsonProperty("created_at")]
            public DateTime? CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public DateTime? UpdatedAt { get; set; }
        }
        public class SaikaiReleaseImage
        {
            public int? Id { get; set; }
            public string Image { get; set; }
            public int? Order { get; set; }

            [JsonProperty("release_id")]
            public int? ReleaseId { get; set; }
            public int? Pending { get; set; }

            [JsonProperty("created_at")]
            public DateTime? CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public DateTime? UpdatedAt { get; set; }
        }

        public class SaikaiMangaData
        {
            public int? Id { get; set; }
            public string Title { get; set; }
            public string Acronym { get; set; }
            public string Slug { get; set; }
            public string Year { get; set; }
            public string Synopsis { get; set; }
            public string Resume { get; set; }
            public object Notes { get; set; }
            public string Image { get; set; }

            [JsonProperty("image_shared")]
            public string ImageShared { get; set; }
            public List<ChapterSeparator> Separators { get; set; }
        }
        public class ChapterSeparator
        {
            public int? Id { get; set; }
            public string Name { get; set; }

            [JsonProperty("nomenclature_chapter")]
            public string NomenclatureChapter { get; set; }
            public int? Order { get; set; }
            public List<ChapterRelease> Releases { get; set; }
        }
        public class ChapterRelease
        {
            public int? Id { get; set; }
            public string Title { get; set; }
            public string Chapter { get; set; }
            public object Source { get; set; }
            public string Slug { get; set; }

            [JsonProperty("published_at")]
            public DateTime? PublishedAt { get; set; }
            public int? Order { get; set; }

            [JsonProperty("created_at")]
            public DateTime? CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public DateTime? UpdatedAt { get; set; }
        }
        
        public class SaikaiSearchData
        {
            public int? Id { get; set; }
            public string Title { get; set; }
            public string Acronym { get; set; }
            public string Slug { get; set; }
            public string Year { get; set; }

            [JsonProperty("format_id")]
            public int? FormatId { get; set; }
            public string Synopsis { get; set; }
            public string Resume { get; set; }
            public string Notes { get; set; }
            public string Image { get; set; }

            [JsonProperty("image_shared")]
            public string ImageShared { get; set; }
        }
        
        public class SaikaiResponse<T> 
        {
            public T Data { get; set; }
        }
    }
}
