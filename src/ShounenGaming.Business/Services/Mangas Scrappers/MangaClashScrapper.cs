using HtmlAgilityPack;
using Serilog;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Web;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    public class MangaClashScrapper : IBaseMangaScrapper
    {

        public string GetBaseURLForManga()
        {
            return "https://mangaclash.com/manga/";
        }

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://mangaclash.com/manga/{urlPart}");

            List<string> imagesUrls = new();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@class='page-break ']/img");
            if (images == null)
                images = htmlDoc.DocumentNode.SelectNodes("//div[@class='page-break no-gaps']/img");

            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("data-src", "").ToString().Trim());
            }
            return imagesUrls;
        }

        public MangaTranslationEnumDTO GetLanguage()
        {
            return MangaTranslationEnumDTO.EN;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://mangaclash.com/manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='post-title']/h1")?.InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectNodes("//div[@class='summary__content show-more']/p").Select(n => n.InnerText.Trim()).Aggregate((a, b) => a + b) ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='summary_image']/a/img")?.GetAttributeValue("data-src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//li[@class='wp-manga-chapter    ']/a");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.InnerText.Replace("Chapter", "") ?? "";
                var chapterUrl = chapter.GetAttributeValue("href", "") ?? "";
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName.Trim(),
                    Link = chapterUrl.Replace("https://mangaclash.com/manga/", ""),
                });
            }
            chapters.Reverse();
            return new ScrappedManga
            {
                Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                Description = mangaDescription,
                Chapters = chapters,
                ImageURL = imageUrl,
                Source = GetMangaSourceEnumDTO()
            };
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.MANGA_CLASH;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            try
            {

                var htmlDoc = await web.LoadFromWebAsync($"https://mangaclash.com/?s={name.Replace(" ", "+")}&post_type=wp-manga");
                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='row c-tabs-item__content']");
                if (mangasFetched == null || !mangasFetched.Any()) return mangasList;

                foreach (var manga in mangasFetched)
                {
                    var mangaName = manga.ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[1].InnerText ?? "";
                    var mangaURL = manga.ChildNodes[1].ChildNodes[1].ChildNodes[1].GetAttributeValue("href", "") ?? "";
                    var imageURL = manga.ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].GetAttributeValue("data-src", "") ?? ""; ;
                    mangasList.Add(new MangaSourceDTO
                    {
                        Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                        Url = mangaURL.Remove(mangaURL.Length - 1).Split("/").Last(),
                        ImageURL = imageURL,
                        Source = GetMangaSourceEnumDTO()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"MangaClash - SearchManga: {ex.Message}");
            }


            return mangasList;
        }
    }
}
