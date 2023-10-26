using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace ShounenGaming.Business.Helpers
{
    //This class will serve as a Client for AniList GraphQL Endpoint to get only what I need until there's a good package out there
    public static class AniListHelper
    {
        private static readonly string BaseAddress = "https://graphql.anilist.co";
        private static readonly string MediaBody = @"{id idMal status format type source chapters averageScore title { romaji english native userPreferred } synonyms description startDate {year month day} endDate {year month  day} countryOfOrigin coverImage { large } genres averageScore meanScore popularity staff { edges { role node { id } } nodes { id  name { first  middle last  full  native userPreferred } image { large } } } }";


        public static async Task<List<ALManga>> SearchMangaByName(string name)
        {
            var query = "query{Page{media(type:MANGA, format: MANGA, search: \"" + name + "\")" + MediaBody + "}}";
            return (await SendRequest<ALResponse<ALSearchResponse>>(query)).Data.Page.Media;
        }

        public static async Task<List<ALManga>> SearchAllMangaTypeByTags(List<string> tags, int page = 1)
        {
            var str = tags.Aggregate((a, b) => a + ", " + b);
            var query = "query{Page (page:" + page + "){media(type:MANGA, format: MANGA, sort:POPULARITY_DESC, genre_in: [\"" + str + "\"])" + MediaBody + "}}";
            return (await SendRequest<ALResponse<ALSearchResponse>>(query)).Data.Page.Media;
        }
        public static async Task<List<ALManga>> SearchMangaByTags(List<string> tags, int page = 1)
        {
            var str = tags.Aggregate((a, b) => a + ", " + b);
            var query = "query{Page (page:" + page + "){media(type:MANGA, format: MANGA, countryOfOrigin: \"JP\", sort:POPULARITY_DESC, genre_in: [\"" + str + "\"])" + MediaBody + "}}";
            return (await SendRequest<ALResponse<ALSearchResponse>>(query)).Data.Page.Media;
        }
        public static async Task<List<ALManga>> SearchManwhaByTags(List<string> tags, int page = 1)
        {
            var str = tags.Aggregate((a, b) => a + ", " + b);
            var query = "query{Page (page:" + page + "){media(type:MANGA, format: MANGA, countryOfOrigin: \"KR\", sort:POPULARITY_DESC, genre_in: [\"" + str + "\"])" + MediaBody + "}}";
            return (await SendRequest<ALResponse<ALSearchResponse>>(query)).Data.Page.Media;
        }
        public static async Task<List<ALManga>> SearchManhuaByTags(List<string> tags, int page = 1)
        {
            var str = tags.Aggregate((a, b) => a + ", " + b);
            var query = "query{Page (page:" + page + "){media(type:MANGA, format: MANGA, countryOfOrigin: \"CN\", sort:POPULARITY_DESC, genre_in: [\"" + str + "\"])" + MediaBody + "}}";
            return (await SendRequest<ALResponse<ALSearchResponse>>(query)).Data.Page.Media;
        }

        public static async Task<List<ALManga>> GetPopularMangas()
        {
            var query = "query{Page{media(type:MANGA, format: MANGA, countryOfOrigin: \"JP\", sort:POPULARITY_DESC)" + MediaBody + "}}";
            return (await SendRequest<ALResponse<ALSearchResponse>>(query)).Data.Page.Media;
        }
        public static async Task<List<ALManga>> GetPopularManhwas()
        {
            var query = "query{Page{media(type:MANGA, format: MANGA, countryOfOrigin: \"KR\", sort:POPULARITY_DESC)" + MediaBody + "}}";
            return (await SendRequest<ALResponse<ALSearchResponse>>(query)).Data.Page.Media;
        }
        public static async Task<List<ALManga>> GetPopularManhuas()
        {
            var query = "query{Page{media(type:MANGA, format: MANGA, countryOfOrigin: \"CN\", sort:POPULARITY_DESC)" + MediaBody + "}}";
            return (await SendRequest<ALResponse<ALSearchResponse>>(query)).Data.Page.Media;
        }
        public static async Task<ALManga> GetMangaById(long alId)
        {
            var query = @"query{Media(id:" + alId + ",type:MANGA)" + MediaBody + "}";
            return (await SendRequest<ALResponse<ALGetMediaResponse>>(query)).Data.Media;
        }

        public static async Task<ALManga> GetMangaByMALId(long malId)
        {
            var query = @"query{Media(idMal:" + malId + ",type:MANGA)" + MediaBody + "}";
            return (await SendRequest<ALResponse<ALGetMediaResponse>>(query)).Data.Media;
        }

        private async static Task<T> SendRequest<T>(string query)
        {
            using HttpClient client = new() { BaseAddress = new Uri(BaseAddress) };
            var queryObject = JObject.FromObject(new { query }); 
            var payload = new StringContent(queryObject.ToString(), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(string.Empty, payload);
            response.EnsureSuccessStatusCode();
            var stringResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(stringResponse)!;
        }

        private class ALSearchResponse
        {
            public ALSearchMediaResponse Page { get; set; } 
        }
        private class ALResponse<T>
        {
            public T Data { get; set; }
        }
        private class ALSearchMediaResponse
        {
            public List<ALManga> Media { get; set; }
        }
        private class ALGetMediaResponse
        {
            public ALManga Media { get; set; }
        }

        public class ALManga
        {
            public long Id { get; set; }
            public long? IdMal { get; set; }
            public string Status { get; set; }
            public string Format { get; set; }
            public int? Volumes { get; set; }
            public int? Chapters { get; set; }
            public string Source { get; set; }
            public string Type { get; set; }
            public ALTitle Title { get; set; }
            public List<string> Synonyms { get; set; }
            public string Description { get; set; }
            public string CountryOfOrigin { get; set; }
            public List<string> Genres { get; set; }
            public int? AverageScore { get; set; }
            public int? MeanScore { get; set; }
            public int? Popularity { get; set; }
            public ALImage CoverImage { get; set; }
            public ALDate StartDate { get; set; }
            public ALDate EndDate { get; set; }
            public ALStaff Staff { get; set; }
        }

        public class ALTitle
        {
            public string Romaji { get; set; }
            public string English { get; set; }
            public string Native { get; set; }
            public string UserPreferred { get; set; }
        }
        public class ALDate
        {
            public int? Year { get; set; }
            public int? Month { get; set; }
            public int? Day { get; set; }
        }
        public class ALImage
        {
            public string Large { get; set; }
        }

        public class ALStaff
        {
            public List<ALEdge> Edges { get; set; }
            public List<ALNode> Nodes { get; set; }
        }
        public class ALEdge
        {
            public string Role { get; set; }
            public ALEdgeNode Node { get; set; }
        }
        public class ALEdgeNode
        {
            public long Id { get; set; }
        }
        public class ALNode
        {
            public long Id { get; set; }
            public ALStaffName Name { get; set; }
            public ALImage Image { get; set; }
        }
        public class ALStaffName
        {
            public string First { get; set; }
            public string Middle { get; set; }
            public string Last { get; set; }
            public string Full { get; set; }
            public string Native { get; set; }
            public string UserPreferred { get; set; }
        }
    }
}
