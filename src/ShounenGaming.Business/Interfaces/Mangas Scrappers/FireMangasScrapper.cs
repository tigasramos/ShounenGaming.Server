using HtmlAgilityPack;
using Newtonsoft.Json;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers
{
    internal class FireMangasScrapper : IBaseMangaScrapper
    {
        //FireMangas PT
        //40 seg -> 9196
        public async Task<List<ScrappedSimpleManga>> GetAllMangas()
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            int currentPage = 1, lastPage =  1;
            do
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://firemangas.com/lista-de-mangas/todos/page/{currentPage}");
                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//li[@class='item-x']/div");
                foreach(var manga in mangasFetched)
                {
                    var mangaUrl = manga.SelectSingleNode("a")?.GetAttributeValue("href", "").Split("/").Last() ?? string.Empty;
                    var mangaName = manga.SelectSingleNode("div/h2/a")?.InnerText ?? "";
                    mangasList.Add(new ScrappedSimpleManga
                    {
                        Name = mangaName,
                        Link = mangaUrl
                    });
                }
                currentPage++;
                lastPage = Convert.ToInt16(htmlDoc.DocumentNode.SelectNodes("//ul[@class='content-pagination alinhamento clear-fix']/li/a").Select(c => c.InnerText).Where(d => !string.IsNullOrEmpty(d)).Last());

            } while (currentPage <= lastPage);

            return mangasList;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://firemangas.com/manga/ler/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='f']/h1")?.InnerText ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sinopse']")?.InnerText ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='thumb']/img")?.GetAttributeValue("src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//ul[@class='full-chapters-list list-of-chapters']/li/a");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.SelectSingleNode("div[@class='chapter-info']/div").InnerText.Replace("Capítulo", "").Trim() ?? "";
                var chapterUrl = chapter.GetAttributeValue("href", "").Replace("https://firemangas.com/ler/","").Trim() ?? "";
                var chapterPageDate = chapter.SelectSingleNode("span[@class='chapter-date']")?.InnerText ?? "";
                var parsed = DateTime.TryParseExact(chapterPageDate, "yyyy-mm-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName,
                    Link = chapterUrl,
                    ReleasedAt = parsed ? chapterDate : null
                });
            }

            return new ScrappedManga
            {
                Name = mangaName,
                Description = mangaDescription,
                Chapters = chapters,
                ImageURL = imageUrl,
            };
        }

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var response = await new HttpClient().GetAsync("https://firemangas.com/jsons/series/images_list.json?id_serie=776");

            var responseContent = await response.Content.ReadAsStringAsync();
            var objectResponse = JsonConvert.DeserializeObject<ChapterPagesResponse>(responseContent);
            return objectResponse?.Images.OrderBy(c => Convert.ToInt16(c.Name)).Select(i => i.Url).ToList() ?? new List<string>();
        }

        private class ChapterPagesResponse
        {
            public int Total { get; set; }
            public List<ChapterImage> Images { get; set; }
        }
        private class ChapterImage
        {
            public string Url { get; set; }
            public string Name { get; set; }
        }
    }
}
