namespace ShounenGaming.Business.Interfaces.Base
{
    public interface IImageService
    {
        Task SaveImage(byte[] image, string pathToSave);
        Task DeleteFolder(string mangaName, string chapter, string translation);
        Task<List<string>> GetChapterImages(string mangaName, string chapter, string translation);
        Task<MangaFileData?> GetAllMangaChapters(string mangaName);
    }
    public class MangaFileData
    {
        public string Name { get; set; }
        public List<ChapterFileData> Chapters { get; set; }
    }
    public class ChapterFileData
    {
        public string Name { get; set; }
        public string Translation { get; set; }
        public int Pages { get; set; }
    }
}

/*
    mangas/
        one-piece/
            thumbnail.jpeg
            chapters/
                en/
                    1/
                        1-page.jpeg
                        2-page.jpeg
                pt/
                    1/
                        {order}-page.jpeg
                        2-page.jpeg



 */