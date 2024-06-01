using ZiggyCreatures.Caching.Fusion;

namespace ShounenGaming.Business.Helpers
{
    public class CacheHelper
    {
        private static readonly string CHAPTERS_QUEUE = "chapters_queue";
        private static readonly string RECENT_MANGAS = "recent_mangas";
        private static readonly string RECENT_CHAPTERS = "recent_chapters";
        private static readonly string ADDED_MANGAS_ACTION = "all_added_mangas";
        private static readonly string STATUS_CHANGED_ACTION = "all_status_changed";
        private static readonly string CHAPTERS_CHANGED_ACTION = "all_chapters_changed";
        private static readonly string MANGA_DTO = "manga_dto";
        private static readonly string MANGA_SOURCES_DTO = "manga_sources_dto";
        private static readonly string SEASON_MANGAS = "season_mangas";
        private static readonly string MANGA_TAGS = "manga_tags";
        private static readonly string SEARCHED_AL = "searched_al";
        private static readonly string SEARCHED_MAL = "searched_mal";
        private static readonly string SCRAPPED_MANGA = "scrapped_manga";


        private static readonly Dictionary<CacheKey, string> keysString = new Dictionary<CacheKey, string> 
        {
            {CacheKey.ADD_MANGA_ACTION, ADDED_MANGAS_ACTION },
            {CacheKey.CHAPTERS_CHANGED_ACTION, CHAPTERS_CHANGED_ACTION },
            {CacheKey.CHAPTERS_QUEUE, CHAPTERS_QUEUE },
            {CacheKey.CUSTOM, string.Empty },
            {CacheKey.MANGA_DTO, MANGA_DTO },
            {CacheKey.MANGA_SOURCES_DTO, MANGA_SOURCES_DTO },
            {CacheKey.MANGA_TAGS, MANGA_TAGS },
            {CacheKey.RECENT_CHAPTERS, RECENT_CHAPTERS },
            {CacheKey.RECENT_MANGAS, RECENT_MANGAS },
            {CacheKey.SCRAPPED_MANGA, SCRAPPED_MANGA },
            {CacheKey.SEARCHED_AL, SEARCHED_AL },
            {CacheKey.SEARCHED_MAL, SEARCHED_MAL },
            {CacheKey.SEASON_MANGAS, SEASON_MANGAS },
            {CacheKey.STATUS_CHANGED_ACTION, STATUS_CHANGED_ACTION },
        };



        private readonly IFusionCache _fusionCache;

        public CacheHelper(IFusionCache fusionCache)
        {
            _fusionCache = fusionCache;
        }

        public async Task<T?> GetCache<T>(CacheKey key, string? keyExtra = null)
        {
            var foundKey = keysString.TryGetValue(key, out var keyString);
            if (!foundKey)
                return default;

            var result = await _fusionCache.TryGetAsync<T>(keyString + (keyExtra != null ? "_" + keyExtra : ""));
            return result.HasValue ? result.Value : default;
        }
        public async Task SetCache<T>(CacheKey key, T data, string? keyExtra = null)
        {
            var foundKey = keysString.TryGetValue(key, out var keyString);
            if (!foundKey)
                return;

            FusionCacheEntryOptions? options = null;
            if (new List<CacheKey> { CacheKey.ADD_MANGA_ACTION, CacheKey.STATUS_CHANGED_ACTION, CacheKey.CHAPTERS_CHANGED_ACTION }.Contains(key))
            {
                options = new FusionCacheEntryOptions(TimeSpan.FromDays(7));
            }
            else if (new List<CacheKey> { CacheKey.SEARCHED_MAL, CacheKey.SEARCHED_AL }.Contains(key))
            {
                options = new FusionCacheEntryOptions(TimeSpan.FromDays(1));
            }

            await _fusionCache.SetAsync<T>(keyString + (keyExtra != null ? "_" + keyExtra : ""), data, options);
        }
        public async Task<T?> GetOrSetCache<T>(CacheKey key, Func<CancellationToken, Task<T?>> factory, string? keyExtra = null)
        {
            var foundKey = keysString.TryGetValue(key, out var keyString);
            if (!foundKey)
                return default;

            FusionCacheEntryOptions? options = null;
            if (new List<CacheKey> { CacheKey.ADD_MANGA_ACTION, CacheKey.STATUS_CHANGED_ACTION, CacheKey.CHAPTERS_CHANGED_ACTION }.Contains(key))
            {
                options = new FusionCacheEntryOptions(TimeSpan.FromDays(7));
            } 
            else if (new List<CacheKey> { CacheKey.SEARCHED_MAL, CacheKey.SEARCHED_AL }.Contains(key))
            {
                options = new FusionCacheEntryOptions(TimeSpan.FromDays(1));
            } 
            else
            {
                options = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
            }

            var result = await _fusionCache.GetOrSetAsync<T>(keyString + (keyExtra != null ? "_" + keyExtra : ""), factory, options);
            return result;
        }


        public async Task DeleteCache(CacheKey key, string? keyExtra = null)
        {
            var foundKey = keysString.TryGetValue(key, out var keyString);
            if (!foundKey)
                return;

            await _fusionCache.ExpireAsync(keyString + (keyExtra != null ? "_" + keyExtra : ""));
        }

        public enum CacheKey
        {
            SCRAPPED_MANGA,
            CHAPTERS_QUEUE,
            SEASON_MANGAS,
            RECENT_MANGAS,
            MANGA_DTO, 
            MANGA_SOURCES_DTO, 
            RECENT_CHAPTERS,
            MANGA_TAGS, 
            ADD_MANGA_ACTION, 
            STATUS_CHANGED_ACTION,
            CHAPTERS_CHANGED_ACTION, 
            SEARCHED_MAL, 
            SEARCHED_AL, 
            CUSTOM, 
        }
    }
}
