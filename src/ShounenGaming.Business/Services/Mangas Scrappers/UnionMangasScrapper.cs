using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Text;
using System.Web;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    public class UnionMangasScrapper : IBaseMangaScrapper
    {
        public async Task<List<MangaSourceDTO>> GetAllMangasByPage(int page)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            try
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://unionleitor.top/lista-mangas/a-z/{page}");

                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-md-3 col-xs-4 text-center lista-mangas-novos']");
                if (mangasFetched == null || !mangasFetched.Any()) return new List<MangaSourceDTO>();

                foreach (var manga in mangasFetched)
                {
                    var mangaName = manga.SelectNodes("a")[1]?.InnerText ?? "";
                    if (mangaName.Contains("(Novel)")) continue;

                    var mangaUrl = manga.SelectNodes("a")[0].GetAttributeValue("href", "") ?? "";
                    var imageUrl = manga.SelectNodes("a")[0].SelectSingleNode("img").GetAttributeValue("src", "") ?? "";
                    mangasList.Add(new MangaSourceDTO
                    {
                        Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                        Url = mangaUrl.Replace("https://unionleitor.top/pagina-manga/", ""),
                        ImageURL = imageUrl,
                        Source = GetMangaSourceEnumDTO()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"UnionMangas - GetAllMangasByPage: {ex.Message}");
            }

            return mangasList;
        }

        public string GetBaseURLForManga()
        {
            return "https://unionleitor.top/pagina-manga";
        }

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://unionleitor.top/leitor/{urlPart}");

            List<string> imagesUrls = new();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@class='row']/div/img");
            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("src", "").Trim());
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
            var htmlDoc = await web.LoadFromWebAsync($"https://unionleitor.top/pagina-manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='row']/div[@class='col-md-12']/h2")?.InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='panel-body']")?.InnerText.Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//img[@class='img-thumbnail']")?.GetAttributeValue("src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//div[@class='row capitulos']");
            foreach (var chapter in scrappedChapters)
            {
                var firstNode = chapter.SelectNodes("div")[0];
                var chapterName = firstNode.SelectSingleNode("a")?.InnerText.Replace("Cap.", "").Trim() ?? "";
                var chapterUrl = firstNode.SelectSingleNode("a").GetAttributeValue("href", "") ?? "";
                var chapterPageDate = firstNode.SelectNodes("span")[1]?.InnerText ?? "";
                var parsed = DateTime.TryParseExact(chapterPageDate, "(dd/MM/yyyy)", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName.Trim(),
                    Link = chapterUrl.Replace("https://unionleitor.top/leitor/",""),
                    ReleasedAt = parsed ? chapterDate : null
                });
            }

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
            return MangaSourceEnumDTO.UNION_MANGAS;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            try
            {
                var response = await new HttpClient().GetAsync($"https://unionleitor.top/assets/busca.php?nomeManga={name.Replace(" ", "+")}");
                response.EnsureSuccessStatusCode();

                UnionMangaList? items = JsonConvert.DeserializeObject<UnionMangaList>(await response.Content.ReadAsStringAsync());
                return items == null
                    ? throw new Exception("Failed parsing Items")
                    : items.Items.Select(i => new MangaSourceDTO
                            {
                                ImageURL = i.Imagem,
                                Name = i.Titulo,
                                Url = i.Url,
                                Source = GetMangaSourceEnumDTO()
                            }).ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"UnionMangas - SearchManga: {ex}");
            }

            return new List<MangaSourceDTO>();
        }
        private class UnionMangaList
        {
            public List<UnionManga> Items { get; set; }
        }
        private class UnionManga
        {
            public string Imagem { get; set; }
            public string Titulo { get; set; }
            public string Url { get; set; }
        }
    }
}
