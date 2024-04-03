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
    public class HentaiTecaScrapper : IBaseMangaScrapper
    {

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://hentaiteca.net/obra/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='post-title']/h1").InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectNodes("//div[@class='summary__content ']/p")[1].InnerText.Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='summary_image']/a/img")?.GetAttributeValue("data-src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//ul[@class='main version-chap no-volumn']/li");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.SelectSingleNode("a").InnerText.Replace("Capítulo", "").Trim() ?? "";
                var chapterUrl = chapter.SelectSingleNode("a")?.GetAttributeValue("href", "") ?? "";
                var chapterPageDate = chapter.SelectSingleNode("span/i")?.InnerText;
                var parsed = DateTime.TryParseExact(chapterPageDate, "dd/MM/yyyy",CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName.Trim(),
                    Link = chapterUrl.Replace("https://hentaiteca.net/obra/", ""),
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

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://hentaiteca.net/obra/{urlPart}");

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
            return "https://hentaiteca.net";
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.HENTAI_TECA;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            var currentPage = 1;

            try
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://hentaiteca.net/page/{currentPage}/?s={name.Replace(" ", "+")}&post_type=wp-manga&op=&author=&artist=&release=&adult=");
                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='row c-tabs-item__content']");
                if (mangasFetched == null || !mangasFetched.Any())
                    return mangasList;

                foreach (var manga in mangasFetched)
                {
                    var mangaName = HttpUtility.HtmlDecode(manga.SelectSingleNode("div/div[@class='tab-summary']/div/h3/a").InnerText) ?? "";
                    if (mangaName.Contains("(Novel)") || mangaName.Contains("– Novel"))
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
            } catch(Exception ex) 
            {
                Log.Error($"HentaiTeca - SearchManga: {ex.Message}");
            }

            return mangasList;
        }
    }
}
