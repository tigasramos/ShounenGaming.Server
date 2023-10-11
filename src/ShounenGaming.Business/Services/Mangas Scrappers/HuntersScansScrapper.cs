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
    public class HuntersScansScrapper : IBaseMangaScrapper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HuntersScansScrapper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://huntersscan.xyz/series/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//title").InnerText.Replace("- Hunters Comics", "").Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", "").Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='summary_image']/a/img")?.GetAttributeValue("data-src", "") ?? "";


            var httpClient = _httpClientFactory.CreateClient();
            using var htmlChapters = await httpClient.PostAsync($"https://huntersscan.xyz/series/{urlPart}/ajax/chapters/", null);
            var htmlChaptersDoc = new HtmlDocument();
            htmlChaptersDoc.LoadHtml(await htmlChapters.Content.ReadAsStringAsync());

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlChaptersDoc.DocumentNode.SelectNodes("//ul[@class='main version-chap no-volumn']/li");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.SelectSingleNode("a").InnerText.Replace("Capítulo", "").Trim() ?? "";
                var chapterUrl = chapter.SelectSingleNode("a")?.GetAttributeValue("href", "") ?? "";
                var chapterPageDate = chapter.SelectSingleNode("span/i")?.InnerText;
                var parsed = DateTime.TryParseExact(chapterPageDate, "dd/MM/yyyy",CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName.Trim(),
                    Link = chapterUrl.Replace("https://huntersscan.xyz/series/", ""),
                    ReleasedAt = parsed ? chapterDate : null
                });
            }

            return new ScrappedManga
            {
                Name = HttpUtility.HtmlDecode(mangaName.Replace("– Hunters Scan", "").Trim()),
                Description = HttpUtility.HtmlDecode(mangaDescription),
                Chapters = chapters,
                ImageURL = imageUrl,
                Source = GetMangaSourceEnumDTO()
            };
        }

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://huntersscan.xyz/series/{urlPart}");

            List<string> imagesUrls = new();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@class='page-break no-gaps']/img");
            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("data-src", "").Trim());
            }
            return imagesUrls;
        }
        public MangaTranslationEnumDTO GetLanguage()
        {
            return MangaTranslationEnumDTO.PT;
        }

        public string GetBaseURLForManga()
        {
            return "https://huntersscan.xyz/series";
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.HUNTERS_SCANS;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            var currentPage = 1;

            try
            {
                while (true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://huntersscan.xyz/page/{currentPage}/?s={name.Replace(" ", "+")}&post_type=wp-manga&op&author&artist&release&adult");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='row c-tabs-item__content']");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("div/div[@class='tab-summary']/div/h3/a").InnerText ?? "";
                        if (mangaName.Contains("(Novel)"))
                            continue;
                        var mangaURL = manga.SelectSingleNode("div/div[@class='tab-summary']/div/h3/a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("div/div/a/img")?.GetAttributeValue("data-src", "") ?? "";
                        mangasList.Add(new MangaSourceDTO
                        {
                            Name = mangaName.Trim(),
                            Url = mangaURL.Remove(mangaURL.Length - 1).Split("/").Last(),
                            ImageURL = imageUrl,
                            Source = GetMangaSourceEnumDTO()
                        });
                    }

                    currentPage++;
                }
            } catch(Exception ex) 
            {
                Log.Error($"HuntersScans - SearchManga: {ex.Message}");
            }

            return mangasList;
        }
    }
}
