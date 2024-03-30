using ShounenGaming.Business.Interfaces.Base;
using System.Net.Http.Json;

namespace ShounenGaming.Business.Services.Base
{
    public class ImageService : IImageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ImageService(IHttpClientFactory httpClientFactory) 
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task SaveImage(byte[] image, string pathToSave)
        {
            var client = _httpClientFactory.CreateClient("FileServer");
            var response = await client.PostAsJsonAsync("mangas", 
                new {
                    Content = image,
                    Path = pathToSave
                });

            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteFolder(string mangaName, string chapter, string translation)
        {
            var client = _httpClientFactory.CreateClient("FileServer");
            var response = await client.DeleteAsync($"mangas/{mangaName}/chapters/{chapter}/{translation}");

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<string>> GetChapterImages(string mangaName, string chapter, string translation)
        {
            var client = _httpClientFactory.CreateClient("FileServer");
            var response = await client.GetAsync($"mangas/{mangaName}/chapters/{chapter}/{translation}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<string>>();
            return result ?? throw new Exception();
        }

        public async Task<MangaFileData?> GetAllMangaChapters(string mangaName)
        {
            var client = _httpClientFactory.CreateClient("FileServer");
            var response = await client.GetAsync($"mangas/{mangaName}/chapters");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<MangaFileData>();
            return null;
        }
    }
}
