using HtmlAgilityPack;
using Serilog;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Web;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    public class DiskusScanScrapper : IBaseMangaScrapper
    {
        public string GetBaseURLForManga()
        {
            return "https://diskusscan.com/manga";
        }

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://diskusscan.com/{urlPart}");

            List<string> imagesUrls = new();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@id='readerarea']/noscript/div/dl/dt/a/img");

            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("src", "").ToString().Trim());
            }
            return imagesUrls;
        }

        public MangaTranslationEnumDTO GetLanguage()
        {
            return MangaTranslationEnumDTO.PT;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://diskusscan.com/manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='entry-title']")?.InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@itemprop='description']")?.InnerText.Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='thumb']/img")?.GetAttributeValue("src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//div[@id='chapterlist']/ul/li/div/div");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.SelectSingleNode("a/span[@class='chapternum']").InnerText.Replace("Chapter", "").Trim() ?? "";
                var chapterUrl = chapter.SelectSingleNode("a").GetAttributeValue("href", "") ?? "";
                var chapterPageDate = chapter.SelectSingleNode("span[@class='chapter-release-date']/i")?.InnerText ?? "";
                var parsed = DateTime.TryParseExact(chapterPageDate, "MMMM dd, yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName.Trim(),
                    Link = chapterUrl.Remove(chapterUrl.Length - 1).Split("/").Last(),
                    ReleasedAt = parsed ? chapterDate : null
                });
            }
            chapters.Reverse();
            return new ScrappedManga
            {
                Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                Description = HttpUtility.HtmlDecode(mangaDescription),
                Chapters = chapters,
                ImageURL = imageUrl,
                Source = GetMangaSourceEnumDTO()
            };
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.DISKUS_SCAN;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            int currentPage = 1;

            try
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://diskusscan.com/page/{currentPage}/?s={name.Replace(" ", "+")}");
                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='listupd']/div/div");
                if (mangasFetched == null || !mangasFetched.Any())
                    return mangasList;

                foreach (var manga in mangasFetched)
                {
                    var mangaName = manga.SelectSingleNode("a/div/div[@class='tt']")?.InnerText ?? "";

                    var mangaUrl = manga.SelectSingleNode("a")?.GetAttributeValue("href", "") ?? "";
                    var imageUrl = manga.SelectSingleNode("a/div/img")?.GetAttributeValue("src", "") ?? "";
                    mangasList.Add(new MangaSourceDTO
                    {
                        Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                        Url = mangaUrl.Remove(mangaUrl.Length - 1).Split("/").Last(),
                        ImageURL = imageUrl,
                        Source = GetMangaSourceEnumDTO()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DiskusScan - SearchManga: {ex.Message}");
            }

            return mangasList;
        }
    }
}
