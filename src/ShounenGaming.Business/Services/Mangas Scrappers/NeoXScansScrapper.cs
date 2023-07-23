using HtmlAgilityPack;
using Serilog;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Web;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    internal class NeoXScansScrapper : IBaseMangaScrapper
    {
        public async Task<List<MangaSourceDTO>> GetAllMangasByPage(int page)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();


            try
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://neoxscans.net/manga/page/{page}");
                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-6 col-md-2 badge-pos-2']/div");
                if (mangasFetched == null || !mangasFetched.Any()) return new List<MangaSourceDTO>();

                foreach (var manga in mangasFetched)
                {
                    var mangaName = manga.SelectSingleNode("div[@class='item-summary']/div/h3/a")?.InnerText ?? "";
                    if (mangaName.Contains("[Novel]"))
                        continue;
                    var mangaUrl = manga.SelectSingleNode("div[@class='item-summary']/div/h3/a")?.GetAttributeValue("href", "") ?? "";
                    var imageUrl = manga.SelectSingleNode("div[@class='item-thumb  c-image-hover']/a/img").GetAttributeValue("src", "") ?? "";
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
                Log.Error($"NeoXScans - GetAllMangasByPage: {ex.Message}");
            }

            return mangasList;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://neoxscans.net/manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='post-title']/h1")?.InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='manga-excerpt']")?.InnerText.Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='summary_image']/a/img")?.GetAttributeValue("src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//ul[@class='main version-chap no-volumn']/li");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.SelectSingleNode("a").InnerText.Replace("Cap.", "").Trim() ?? "";
                var chapterUrl = chapter.SelectSingleNode("a").GetAttributeValue("href", "") ?? "";
                var chapterPageDate = chapter.SelectSingleNode("span[@class='chapter-release-date']/i")?.InnerText ?? "";
                var parsed = DateTime.TryParseExact(chapterPageDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName.Trim(),
                    Link = chapterUrl.Replace("https://neoxscans.net/manga/", "").Trim(),
                    ReleasedAt = parsed ? chapterDate : null
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
                    var htmlDoc = await web.LoadFromWebAsync($"https://neoxscans.net/manga/{urlPart}");

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
            return "https://neoxscans.net/manga";
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
                    var htmlDoc = await web.LoadFromWebAsync($"https://neoxscans.net/page/{currentPage}/?s={name.Replace(" ", "+")}&post_type=wp-manga");
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
