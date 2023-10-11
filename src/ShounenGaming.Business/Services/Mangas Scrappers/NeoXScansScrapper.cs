using HtmlAgilityPack;
using Serilog;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Globalization;
using System.Net.Http;
using System.Web;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    public class NeoXScansScrapper : IBaseMangaScrapper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NeoXScansScrapper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://neoxscan.net/manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='post-title']/h1")?.InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='manga-excerpt']")?.InnerText.Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='summary_image']/a/img")?.GetAttributeValue("src", "") ?? "";

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Host = "neoxscan.net";
            using var htmlChapters = await httpClient.PostAsync($"https://neoxscan.net/manga/{urlPart}/ajax/chapters/", new StringContent(string.Empty));
            var htmlChaptersDoc = new HtmlDocument();
            htmlChaptersDoc.LoadHtml(await htmlChapters.Content.ReadAsStringAsync());

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlChaptersDoc.DocumentNode.SelectNodes("//li[@class='wp-manga-chapter   has-thumb  ']");
            if (scrappedChapters != null)
                foreach (var chapter in scrappedChapters)
                {
                    var chapterName = chapter.SelectSingleNode("a").InnerText.Replace(mangaName, "").Replace("Capítulo", "").Replace("Cap.", "").Trim() ?? "";
                    var chapterUrl = chapter.SelectSingleNode("a").GetAttributeValue("href", "") ?? "";
                    var releasedDate = chapter.SelectSingleNode("span/i").InnerText;
                    DateTime convertedDate = DateTime.Now;
                    var dateConverted = releasedDate is null ? false : DateTime.TryParseExact(releasedDate, "MMMM d, yyyy", new CultureInfo("pt-BR"), DateTimeStyles.None, out convertedDate);

                    chapters.Add(new ScrappedChapter
                    {
                        Name = chapterName.Trim(),
                        Link = chapterUrl.Replace("https://neoxscan.net/manga/", "").Trim(),
                        ReleasedAt = dateConverted ? DateTime.SpecifyKind(convertedDate, DateTimeKind.Utc) : null,
                    });
                }

            return new ScrappedManga
            {
                Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                Description = mangaDescription,
                Chapters = chapters,
                ImageURL = imageUrl,
                Source = GetMangaSourceEnumDTO()
            };
        }
        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            int repeat = 0;

            while (repeat < 3)
            {
                try
                {
                    var web = new HtmlWeb();
                    var htmlDoc = await web.LoadFromWebAsync($"https://neoxscan.net/manga/{urlPart}");

                    List<string> imagesUrls = new();
                    var images = htmlDoc.DocumentNode.SelectNodes("//div[@class='page-break ']/img");
                    foreach (var image in images)
                    {
                        imagesUrls.Add(image.GetAttributeValue("src", "").Trim());
                    }
                    return imagesUrls;
                }
                catch
                {
                    repeat++;
                }
               
            }

            throw new Exception();
        }
        public MangaTranslationEnumDTO GetLanguage()
        {
            return MangaTranslationEnumDTO.PT;
        }

        public string GetBaseURLForManga()
        {
            return "https://neoxscan.net/manga";
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.NEO_X_SCANS;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            int currentPage = 1;

            try { 
                while(true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://neoxscan.net/page/{currentPage}/?s={name.Replace(" ", "+")}&post_type=wp-manga");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='row c-tabs-item__content']");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("div/div[@class='tab-summary']/div/h3/a")?.InnerText ?? "";
                        if (mangaName.Contains("[Novel]"))
                            continue;
                        var mangaUrl = manga.SelectSingleNode("div/div[@class='tab-summary']/div/h3/a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("div/div[@class='tab-thumb c-image-hover']/a/img").GetAttributeValue("src", "") ?? "";
                        mangasList.Add(new MangaSourceDTO
                        {
                            Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                            Url = mangaUrl.Remove(mangaUrl.Length - 1).Split("/").Last(),
                            ImageURL = imageUrl,
                            Source = GetMangaSourceEnumDTO()
                        });
                    }
                    currentPage++;
                }
            } catch (Exception ex) 
            {
                Log.Error($"NeoXScans - SearchManga: {ex.Message}");
            }

            return mangasList;
        }
    }
}
