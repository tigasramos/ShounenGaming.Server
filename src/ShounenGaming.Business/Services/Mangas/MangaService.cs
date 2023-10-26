using AutoMapper;
using JikanDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Helpers;
using ShounenGaming.Business.Hubs;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Services.Mangas_Scrappers;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DataAccess.Interfaces.Base;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.DTOs.Models.Base;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Globalization;
using System.Net;
using ZiggyCreatures.Caching.Fusion;

namespace ShounenGaming.Business.Services.Mangas
{
    public class MangaService : IMangaService
    {
        private static readonly string QUEUE_CACHE_KEY = "mangasQueue";

        private readonly IUserRepository _userRepository;
        private readonly IMangaRepository _mangaRepo;
        private readonly IMangaUserDataRepository _mangaUserDataRepo;
        private readonly IMangaWriterRepository _mangaWriterRepo;
        private readonly IMangaTagRepository _mangaTagRepo;
        private readonly IAddedMangaActionRepository _addedMangaRepo;

        private readonly IJikan _jikan;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;
        private readonly IFetchMangasQueue _queue;
        private readonly IHubContext<MangasHub, IMangasHubClient> _mangasHub;
        private readonly IMemoryCache _cache;
        private readonly IFusionCache _fusionCache;

        private readonly IEnumerable<IBaseMangaScrapper> _scrappers;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MangaService(IMangaRepository mangaRepo, IMangaWriterRepository mangaWriterRepo, IMangaTagRepository mangaTagRepo, IMapper mapper, IImageService imageService, IJikan jikan, IAddedMangaActionRepository addedMangaRepo, IUserRepository userRepository, IFetchMangasQueue queue, IMemoryCache cache, IHubContext<MangasHub, IMangasHubClient> mangasHub, IFusionCache fusionCache, IEnumerable<IBaseMangaScrapper> scrappers, IHttpContextAccessor httpContextAccessor, IMangaUserDataRepository mangaUserDataRepo)
        {
            _mangaRepo = mangaRepo;
            _mangaWriterRepo = mangaWriterRepo;
            _mangaTagRepo = mangaTagRepo;
            _mapper = mapper;
            _imageService = imageService;
            _jikan = jikan;
            _addedMangaRepo = addedMangaRepo;
            _userRepository = userRepository;
            _queue = queue;
            _cache = cache;
            _mangasHub = mangasHub;
            _fusionCache = fusionCache;
            _scrappers = scrappers;
            _httpContextAccessor = httpContextAccessor;
            _mangaUserDataRepo = mangaUserDataRepo;
        }


        public async Task<MangaDTO> GetMangaById(int id)
        {
            return (await _fusionCache.GetOrSetAsync($"manga_{id}_dto", async _ =>
            {
                var manga = await _mangaRepo.GetById(id);
                if (manga is null)
                    throw new EntityNotFoundException("Manga");

                return _mapper.Map<MangaDTO>(manga);
            }))!;
        }
        public async Task<List<MangaSourceDTO>> GetMangaSourcesById(int id)
        {
            return await _fusionCache.GetOrSetAsync($"manga_{id}_sources_dto", async _ =>
            {
                var manga= await _mangaRepo.GetById(id);
                if (manga is null) 
                    throw new EntityNotFoundException("Manga");
                return _mapper.Map<List<MangaSourceDTO>>(manga.Sources);
            }) ?? new List<MangaSourceDTO>();
        }
        public async Task<MangaTranslationDTO?> GetMangaTranslation(int userId, int mangaId, int chapterId, MangaTranslationEnumDTO translation)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
                throw new EntityNotFoundException("User");

            var manga = await _mangaRepo.GetById(mangaId);
            if (manga is null) 
                throw new EntityNotFoundException("Manga");

            var mangaChapter = manga.Chapters.FirstOrDefault(c => c.Id == chapterId) ?? 
                throw new EntityNotFoundException("MangaChapter");

            var mangaTranslation = mangaChapter.Translations.FirstOrDefault(t => t.Language == _mapper.Map<TranslationLanguageEnum>(translation) && t.IsWorking);
            mangaTranslation ??= mangaChapter.Translations.FirstOrDefault(t => t.IsWorking) ?? throw new EntityNotFoundException("MangaTranslation");

            var selectedSource = string.Empty;
            var pages = new List<string>();

            // Show Local Path
            if (mangaTranslation.Downloaded)
            {
                var mangaNameSimplified = manga.Name.NormalizeStringToDirectory();
                var files = _imageService.GetFilesFromFolder(BuildTranslationFolderPath(mangaNameSimplified, mangaTranslation.Language.ToString().ToLower(), mangaChapter.Name.ToString().NormalizeStringToDirectory()));
                files.ForEach(f => pages.Add(_httpContextAccessor.HttpContext?.Request.Scheme + "://" +_httpContextAccessor.HttpContext?.Request.Host + "/" + f));
                return MapMangaTranslation(mangaTranslation, selectedSource, pages, user.MangasConfigurations.SkipChapterToAnotherTranslation);
            }

            //Fetch Pages Runtime
            foreach(var source in manga.Sources)
            {
                try
                {
                    var scrapperEnum = (MangaSourceEnumDTO)Enum.Parse(typeof(MangaSourceEnumDTO), source.Source);
                    var scrapper = GetScrapperByEnum(scrapperEnum);
                    if (scrapper == null)
                        continue;

                    var scrapperTranslation = _mapper.Map<TranslationLanguageEnum>(scrapper?.GetLanguage());
                    if (scrapperTranslation != mangaTranslation.Language)
                        continue;

                    var cachedManga = _cache.TryGetValue(source.Url, out ScrappedManga? mangaInfo);
                    if (!cachedManga)
                    {
                        mangaInfo = await scrapper!.GetManga(source.Url);
                        _cache.Set(source.Url, mangaInfo, DateTimeOffset.Now.AddMinutes(30));
                    }
                    
                    foreach(var c in mangaInfo!.Chapters)
                    {
                        var treatedName = c.Name.Split(":").First().Split("-").First().Split(" ").First().Trim();
                        if (string.IsNullOrEmpty(treatedName) || !double.TryParse(treatedName, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var number))
                            continue;

                        if (number == mangaTranslation.MangaChapter.Name)
                        {
                            selectedSource = source.Source; 
                            pages = await scrapper.GetChapterImages(c.Link);

                            return MapMangaTranslation(mangaTranslation, selectedSource, pages, user.MangasConfigurations.SkipChapterToAnotherTranslation);
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error Fetching Manga Translation");
                }
            }

            return null;
        }
        public async Task<PaginatedResponse<MangaInfoDTO>> SearchMangas(SearchMangaQueryDTO query, int page, int? userId = null)
        {
            var includeNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _userRepository.GetById(userId.Value);
                if (user is null)
                    throw new EntityNotFoundException("User");
                includeNSFW = user.MangasConfigurations.NSFWBehaviour != NSFWBehaviourEnum.HIDE_ALL;
                showProgressAll = user.MangasConfigurations.ShowProgressForChaptersWithDecimals;
            }

            var mangas = await _mangaRepo.SearchManga(page, includeNSFW, query.Name, userId);

            if (!showProgressAll)
                mangas.ForEach(m => m.Chapters = m.Chapters.Where(c => (c.Name % 1) == 0).ToList());

            return new PaginatedResponse<MangaInfoDTO>
            {
                CurrentPage = page,
                Data = _mapper.Map<List<MangaInfoDTO>>(mangas),
                MaxCount = await _mangaRepo.GetAllCount(includeNSFW, query.Name, userId)
            };
        }
        public async Task<List<MangaInfoDTO>> GetWaitingMangas()
        {
            var waitingMangas = await _mangaRepo.GetWaitingMangas();
            return _mapper.Map<List<MangaInfoDTO>>(waitingMangas);
        }

        public async Task<List<MangaInfoDTO>> GetSeasonMangas()
        {
            return await _fusionCache.GetOrSetAsync($"season_mangas_dto", async _ =>
            {
                var mangas = await _mangaRepo.GetSeasonMangas();
                return _mapper.Map<List<MangaInfoDTO>>(mangas);
            }) ?? new List<MangaInfoDTO>();
        }

        public async Task<List<MangaInfoDTO>> GetPopularMangas(int? userId = null)
        {
            var includesNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _userRepository.GetById(userId.Value) ?? throw new EntityNotFoundException("User");
                includesNSFW = user.MangasConfigurations.NSFWBehaviour != NSFWBehaviourEnum.HIDE_ALL;
                showProgressAll = user.MangasConfigurations.ShowProgressForChaptersWithDecimals;
            }

            var popularMangas = await _mangaRepo.GetPopularMangas(includesNSFW);

            if (!showProgressAll)
                popularMangas.ForEach(m => m.Chapters = m.Chapters.Where(c => (c.Name % 1) == 0).ToList());

            return _mapper.Map<List<MangaInfoDTO>>(popularMangas);
        }
       
        public async Task<List<MangaInfoDTO>> GetRecentlyAddedMangas()
        {
            return await _fusionCache.GetOrSetAsync($"recent_mangas", async _ =>
            {
                var mangas = await _mangaRepo.GetRecentlyAddedMangas();
                return _mapper.Map<List<MangaInfoDTO>>(mangas.Take(15));
            }) ?? new List<MangaInfoDTO>();
        }
        public async Task<List<LatestReleaseMangaDTO>> GetRecentlyReleasedChapters(int? userId = null)
        {
            var includeNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _userRepository.GetById(userId.Value);
                if (user is null)
                    throw new EntityNotFoundException("User");
                includeNSFW = user.MangasConfigurations.NSFWBehaviour != NSFWBehaviourEnum.HIDE_ALL;
                showProgressAll = user.MangasConfigurations.ShowProgressForChaptersWithDecimals;
            }

            var cachedDto = await _fusionCache.TryGetAsync<List<LatestReleaseMangaDTO>>($"recent_chapters_{includeNSFW}");
            if (cachedDto.HasValue)
                return cachedDto.Value;

            var mangas = await _mangaRepo.GetRecentlyReleasedChapters(includeNSFW);
            var dto = new List<LatestReleaseMangaDTO>();
            foreach (var manga in mangas)
            {
                var dic = new Dictionary<MangaTranslationEnumDTO, ChapterReleaseDTO>();
                var orderedList = manga.Chapters.OrderByDescending(c => c.Name).ToList();
                foreach (var chapter in orderedList)
                {
                    foreach(var translation in chapter.Translations.Where(t => t.IsWorking))
                    {
                        var language = _mapper.Map<MangaTranslationEnumDTO>(translation.Language);
                        if (!dic.ContainsKey(language))
                        {
                            dic.Add(language, new ChapterReleaseDTO
                            {
                                Id = chapter.Id,
                                Name = chapter.Name,
                                Translation = language,
                                CreatedAt = translation.CreatedAt
                            });
                        }
                    }

                    if (dic.Count == Enum.GetNames(typeof(MangaTranslationEnumDTO)).Length) break;
                }

                if (!showProgressAll)
                    manga.Chapters = manga.Chapters.Where(c => (c.Name % 1) == 0).ToList();

                dto.Add(new LatestReleaseMangaDTO
                {
                    Manga = _mapper.Map<MangaInfoDTO>(manga),
                    ReleasedChapters = dic
                });
            }
            await _fusionCache.SetAsync($"recent_chapters_{includeNSFW}", dto);
            return dto;
        }
        public async Task<List<MangaWriterDTO>> GetMangaWriters()
        {
            var writers = await _mangaWriterRepo.GetAll();
            return _mapper.Map<List<MangaWriterDTO>>(writers);
        }
        
        public async Task<List<MangaInfoDTO>> GetMangasFromTag(string tag, int? userId = null)
        {
            var includesNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _fusionCache.GetOrSetAsync($"user_{userId.Value}", async _ => await _userRepository.GetById(userId.Value));
                if (user is null)
                    throw new EntityNotFoundException("User");
                includesNSFW = user.MangasConfigurations.NSFWBehaviour != NSFWBehaviourEnum.HIDE_ALL;
                showProgressAll = user.MangasConfigurations.ShowProgressForChaptersWithDecimals;
            }

            var mangas = await _fusionCache.GetOrSetAsync($"manga_tag_{tag.ToLower()}_{includesNSFW}", async _ => await _mangaRepo.GetMangasByTag(tag , includesNSFW));

            if (!showProgressAll)
                mangas.ForEach(m => m.Chapters = m.Chapters.Where(c => (c.Name % 1) == 0).ToList());

            return _mapper.Map<List<MangaInfoDTO>>(mangas);
        }
        public async Task<List<string>> GetMangaTags()
        {
            var tags = await _fusionCache.GetOrSetAsync("manga_tags", async _ => await _mangaTagRepo.GetAll()) ?? new List<MangaTag>();
            return tags.Select(t => t.Name).ToList();
        }
        public async Task<MangaWriterDTO> GetMangaWriterById(int id)
        {
            var writer = await _mangaWriterRepo.GetById(id);
            return _mapper.Map<MangaWriterDTO>(writer);
        }

        public async Task<List<MangaMetadataDTO>> SearchMangaMetadata(MangaMetadataSourceEnumDTO source, string name)
        {
            switch (source)
            {
                case MangaMetadataSourceEnumDTO.ANILIST:
                    var aniMangas = await AniListHelper.SearchMangaByName(name);
                    var alDtos = new List<MangaMetadataDTO>();
                    foreach(var m in aniMangas)
                    {
                        alDtos.Add(await ConvertALMangaToDTO(m));
                    }
                    return alDtos;

                case MangaMetadataSourceEnumDTO.MYANIMELIST:
                    var malMangas = (await _jikan.SearchMangaAsync(name)).Data;

                    // Filter only allowed types
                    malMangas = malMangas.FilterCorrectTypes();

                    var malDtos = new List<MangaMetadataDTO>();
                    foreach(var m in malMangas)
                    {
                        malDtos.Add(await ConvertMALMangaToDTO(m));
                    }
                    return malDtos;
                   
                default:
                    return new List<MangaMetadataDTO>();
            }
        }
        private async Task<MangaMetadataDTO> ConvertALMangaToDTO(AniListHelper.ALManga m, bool mayExist = true)
        {
            var titles = new List<string>();
            if (m.Title.UserPreferred != null)
                titles.Add(m.Title.UserPreferred);

            if (m.Title.English != null)
                titles.Add(m.Title.English);

            if (m.Title.Native != null)
                titles.Add(m.Title.Native);

            if (m.Title.Romaji != null)
                titles.Add(m.Title.Romaji);

            return new MangaMetadataDTO
            {
                Id = m.Id,
                Titles = titles,
                AlreadyExists = mayExist ? await _mangaRepo.MangaExistsByAL(m.Id) : false,
                Description = m.Description ?? "",
                Tags = m.Genres.ToList(),
                Status = m.Status,
                Score = (m.AverageScore is not null ? m.AverageScore / (decimal)10 : m.MeanScore / (decimal)10) ?? 0,
                FinishedAt = m.EndDate.Year != null ? new DateTime(m.EndDate.Year.Value, m.EndDate.Month ?? 1, m.EndDate.Day ?? 1) : null,
                ImageUrl = m.CoverImage.Large,
                StartedAt = m.StartDate.Year != null ? new DateTime(m.StartDate.Year.Value, m.StartDate.Month ?? 1, m.StartDate.Day ?? 1) : null,
                Type = _mapper.Map<MangaTypeEnumDTO>(ConvertMALMangaType(m.CountryOfOrigin)),
                Source = MangaMetadataSourceEnumDTO.ANILIST
            };
        }
        private async Task<MangaMetadataDTO> ConvertMALMangaToDTO(JikanDotNet.Manga m, bool mayExist = true)
        {
            return new MangaMetadataDTO
            {
                Id = m.MalId,
                Titles = m.Titles.Select(t => t.Title).ToList(),
                Description = m.Synopsis ?? "",
                FinishedAt = m.Published.To,
                AlreadyExists = mayExist ? await _mangaRepo.MangaExistsByMAL(m.MalId) : false,
                Type = m.Type == "manhwa" ? MangaTypeEnumDTO.MANWHA : (m.Type == "manhua" ? MangaTypeEnumDTO.MANHUA : MangaTypeEnumDTO.MANGA),
                Status = m.Status,
                StartedAt = m.Published.From,
                ImageUrl = m.Images.JPG.ImageUrl,
                Score = m.Score ?? 0,
                Tags = m.Genres.Select(g => g.Name).ToList(),
                Source = MangaMetadataSourceEnumDTO.MYANIMELIST
            };
        }
        
        public async Task<List<MangaInfoDTO>> GetMangaRecommendations(int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
                throw new EntityNotFoundException("User");

            var userData = await _mangaUserDataRepo.GetByUser(userId);
            if (userData is null)
                throw new EntityNotFoundException("UserData");

            var userSeenMangas = userData.Where(m => m.Status == MangaUserStatusEnum.COMPLETED ||
                m.Status == MangaUserStatusEnum.READING || m.Status == MangaUserStatusEnum.DROPPED ||
                m.Status == MangaUserStatusEnum.ON_HOLD);

            // If no readings yet -> Popular
            if (!userData.Any())
            {
                return _mapper.Map<List<MangaInfoDTO>>(await _mangaRepo.GetPopularMangas());
            }

            // If readings search the most common tags
            var tagsScores = new Dictionary<string, double>();
            var typesScores = new Dictionary<MangaTypeEnum, double>();
            foreach (var manga in userSeenMangas)
            {
                //Types
                if (typesScores.ContainsKey(manga.Manga.Type))
                {
                    typesScores[manga.Manga.Type] += manga.Rating ?? 2.5;
                }
                else typesScores[manga.Manga.Type] = manga.Rating ?? 2.5;

                //Tags
                foreach (var tag in manga.Manga.Tags)
                {
                    if (tagsScores.ContainsKey(tag.Name))
                    {
                        tagsScores[tag.Name] += manga.Rating ?? 2.5;
                    }
                    else tagsScores[tag.Name] = manga.Rating ?? 2.5;
                }
            }

            // Search the mangas with OK rating which have most of those tags (not read before) and not on ignore !
            var allMangas = await _mangaRepo.GetAll();
            var allMangasNotSeen = allMangas.Where(m => !userSeenMangas.Any(usm => usm.Manga.Id == m.Id)).OrderByDescending(a => ((a.ALScore ?? a.MALScore) + (a.MALScore ?? a.ALScore)) / 2);

            var mangasScores = new Dictionary<int, double>();
            foreach (var manga in allMangasNotSeen)
            {
                mangasScores[manga.Id] = 0;
                foreach (var tag in manga.Tags)
                {
                    if (tagsScores.ContainsKey(tag.Name))
                        mangasScores[manga.Id] += tagsScores[tag.Name];
                }

                if (typesScores.ContainsKey(manga.Type))
                    mangasScores[manga.Id] += typesScores[manga.Type];
            }

            // TODO: Maybe put a "where value" bigger than something
            // TODO: Take points from Ignored

            var mangaIds = mangasScores.Where(ms => ms.Value > 0).OrderByDescending(ms => ms.Value).Select(ms => ms.Key).Take(20).ToList();
            return _mapper.Map<List<MangaInfoDTO>>(allMangasNotSeen.Where(m => mangaIds.Contains(m.Id)));
        }
        public async Task<List<MangaMetadataDTO>> SearchMangaRecommendations(int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
                throw new EntityNotFoundException("User");

            var userData = await _mangaUserDataRepo.GetByUser(userId);
            if (userData is null)
                throw new EntityNotFoundException("UserData");

            var userSeenMangas = userData.Where(m => m.Status == MangaUserStatusEnum.COMPLETED ||
                m.Status == MangaUserStatusEnum.READING || m.Status == MangaUserStatusEnum.DROPPED ||
                m.Status == MangaUserStatusEnum.ON_HOLD);

            var allDTOs = new List<MangaMetadataDTO>();

            // If no readings yet -> Popular
            if (!userData.Any())
            {
                var malMangas = (await _jikan.GetTopMangaAsync()).Data;
                malMangas = malMangas.FilterCorrectTypes();

                foreach (var m in malMangas)
                {
                    allDTOs.Add(await ConvertMALMangaToDTO(m));
                }

                var alMangas = await AniListHelper.GetPopularMangas();
                foreach (var m in alMangas)
                {
                    if (m.IdMal is not null && allDTOs.Any(a => a.Id == m.IdMal && a.Source == MangaMetadataSourceEnumDTO.MYANIMELIST))
                        continue;

                    allDTOs.Add(await ConvertALMangaToDTO(m));
                }
                return allDTOs.OrderByDescending(m => m.Score).ToList();
            }

            // Get most used Tags & Types
            var tagsScores = new Dictionary<string, double>();
            var typesScores = new Dictionary<MangaTypeEnumDTO, double>();
            foreach (var manga in userSeenMangas)
            {
                //Types
                var dtoType = _mapper.Map<MangaTypeEnumDTO>(manga.Manga.Type);
                if (typesScores.ContainsKey(dtoType))
                {
                    typesScores[dtoType] += manga.Rating ?? 2.5;
                }
                else typesScores[dtoType] = manga.Rating ?? 2.5;

                //Tags
                foreach (var tag in manga.Manga.Tags)
                {
                    if (tagsScores.ContainsKey(tag.Name))
                    {
                        tagsScores[tag.Name] += manga.Rating ?? 2.5;
                    }
                    else tagsScores[tag.Name] = manga.Rating ?? 2.5;
                }
            }

            var allMangas = await _mangaRepo.GetAll();

            // Fetch Mangas
            var mostSeenTags = tagsScores.OrderByDescending(ms => ms.Value).Select(ms => ms.Key).Take(5).ToList();
            int page = 1;
            while (allDTOs.Count < 80)
            {

                var maxMalPages = 3;
                for (int i = 0; i < maxMalPages; i++)
                {
                    var searchedMALMangas = await _fusionCache.GetOrSetAsync($"searched_mal_{i}_{page}", async _ => (await _jikan.SearchMangaAsync(new MangaSearchConfig { Page = (page * maxMalPages) - (maxMalPages - 1) + i, OrderBy = MangaSearchOrderBy.Popularity })).Data, new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(12) });
                    searchedMALMangas = searchedMALMangas?.FilterCorrectTypes();
                    var searchedMALMangasNotAdded = searchedMALMangas?.Where(sm => !allMangas.Any(am => am.MangaMyAnimeListID == sm.MalId) && !allDTOs.Any(dtos => dtos.Source == MangaMetadataSourceEnumDTO.MYANIMELIST && dtos.Id == sm.MalId)).ToList();
                    searchedMALMangasNotAdded?.ForEach(async s => allDTOs.Add(await ConvertMALMangaToDTO(s, false)));
                }

                // Closer to Favorites
                var top3Tags = mostSeenTags.Take(3).ToList();
                var topType = typesScores.OrderByDescending(ts => ts.Value).Select(ts => ts.Key).FirstOrDefault();
                var searchedTopALMangas = await _fusionCache.GetOrSetAsync($"searched_al_{top3Tags}_{topType}", async _ =>
                {
                    if (topType == MangaTypeEnumDTO.MANHUA)
                        return await AniListHelper.SearchManhuaByTags(top3Tags, page);
                    else if (topType == MangaTypeEnumDTO.MANWHA)
                        return await AniListHelper.SearchManwhaByTags(top3Tags, page);
                    else if (topType == MangaTypeEnumDTO.MANGA)
                        return await AniListHelper.SearchMangaByTags(top3Tags, page);

                    return await AniListHelper.SearchAllMangaTypeByTags(top3Tags, page);
                }, new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(12) });
                var searchedTopALMangasNotAdded = searchedTopALMangas?.Where(sm => !allMangas.Any(am => am.MangaAniListID == sm.Id) && !allDTOs.Any(dtos => dtos.Source == MangaMetadataSourceEnumDTO.ANILIST && dtos.Id == sm.Id) && !allDTOs.Any(dtos => dtos.Source == MangaMetadataSourceEnumDTO.MYANIMELIST && dtos.Id == sm.IdMal)).ToList();
                searchedTopALMangasNotAdded?.ForEach(async s => allDTOs.Add(await ConvertALMangaToDTO(s, false)));

                // From Single Tag
                foreach (var tag in mostSeenTags)
                {
                    var searchedALMangas = await _fusionCache.GetOrSetAsync($"searched_al_{tag}_{page}", async _ => await AniListHelper.SearchAllMangaTypeByTags(new List<string>() { tag }, page), new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(12)});
                    var searchedALMangasNotAdded = searchedALMangas?.Where(sm => !allMangas.Any(am => am.MangaAniListID == sm.Id) && !allDTOs.Any(dtos => dtos.Source == MangaMetadataSourceEnumDTO.ANILIST && dtos.Id == sm.Id) && !allDTOs.Any(dtos => dtos.Source == MangaMetadataSourceEnumDTO.MYANIMELIST && dtos.Id == sm.IdMal)).ToList();
                    searchedALMangasNotAdded?.ForEach(async s => allDTOs.Add(await ConvertALMangaToDTO(s, false)));
                }


                page++;
            }

            // Calculate Mangas
            var mangasScores = new Dictionary<int, double>();
            for (int i = 0; i < allDTOs.Count; i++)
            {
                var manga = allDTOs[i];
                mangasScores[i] = 0;
                foreach (var tag in manga.Tags)
                {
                    if (tagsScores.ContainsKey(tag))
                        mangasScores[i] += tagsScores[tag];
                }

                if (typesScores.ContainsKey(manga.Type))
                    mangasScores[i] += typesScores[manga.Type];
            }

            var orderedDTOs = mangasScores.Where(ms => ms.Value > 0).OrderByDescending(ms => ms.Value).Select(ms => allDTOs[ms.Key]).ToList();
            return orderedDTOs;
        }



        public async Task<List<MangaSourceDTO>> SearchMangaSource(string name)
        {
            var treatedName = name.Replace("\"", "")
                .Replace(",", "")
                .Replace(".", "")
                .Replace("!", "")
                .Replace("#", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("'", " ");

            var finalResults = await _fusionCache.GetOrSetAsync($"manga_sources_{treatedName}", async _ =>  {
                List<Task<List<MangaSourceDTO>>> allMangasTasks = new();

                foreach (var scrapper in _scrappers)
                {
                    allMangasTasks.Add(scrapper.SearchManga(treatedName));
                }
                var result = await Task.WhenAll(allMangasTasks.ToArray());

                var listResults = new List<MangaSourceDTO>();
                foreach (var item in result)
                {
                    listResults.AddRange(item);
                }
                return listResults;
            });
           
            return finalResults ?? new List<MangaSourceDTO>();
        }
        public async Task<List<MangaSourceDTO>> LinkSourcesToManga(int mangaId, List<MangaSourceDTO> mangas)
        {
            var manga = await _mangaRepo.ClearSources(mangaId);
            if (manga is null)
                throw new EntityNotFoundException("Manga");

            manga.Sources = mangas.Select(m => new MangaSource { Source = m.Source.ToString(), Url = m.Url , ImageURL = m.ImageURL, Name = m.Name}).ToList();

            var dbManga = await _mangaRepo.Update(manga);

            await _fusionCache.ExpireAsync($"manga_{mangaId}_sources_dto");

            return _fusionCache.GetOrSet($"manga_{mangaId}_sources_dto", _ => _mapper.Map<List<MangaSourceDTO>>(dbManga.Sources))!;
        }


        public async Task<MangaDTO> AddManga(MangaMetadataSourceEnumDTO source, long mangaId, int userId)
        {
            var user = await _userRepository.GetById(userId) ?? throw new EntityNotFoundException("User");

            //TODO: Handle Exceptions (Timeout and wrong id)
            var manga = source == MangaMetadataSourceEnumDTO.MYANIMELIST ? await AddMALManga(mangaId, user.Id) : await AddALManga(mangaId, user.Id);
            if (manga is null)
                throw new Exception("Couldn't create or get the Manga");

            await _fusionCache.ExpireAsync($"recent_mangas");
            return _mapper.Map<MangaDTO>(manga);
        }
        public async Task<MangaDTO> AddManga(MangaMetadataSourceEnumDTO source, long mangaId, string discordId)
        {
            var user = await _userRepository.GetUserByDiscordId(discordId) ?? throw new EntityNotFoundException("User");
            return await AddManga(source, mangaId, user.Id);
        }
        #region Add Manga Private

        /// <summary>
        /// Adds a new MAL Manga to the DB, if already exists just returns it
        /// </summary>
        /// <param name="malId">MyAnimeList Id</param>
        /// <returns></returns>
        private async Task<Core.Entities.Mangas.Manga> AddMALManga(long malId, int? userId)
        {
            // Check if already exists
            var dbManga = await _mangaRepo.GetByMALId(malId);
            if (dbManga != null) return dbManga;

            var mangaResponse = await _jikan.GetMangaAsync(malId);
            var manga = mangaResponse.Data;

            return await AddMALManga(manga, userId);
        }
        private async Task<Core.Entities.Mangas.Manga> AddMALManga(JikanDotNet.Manga manga, int? userId)
        {
            // Check if already exists
            var dbManga = await _mangaRepo.GetByMALId(manga.MalId);
            if (dbManga != null) return dbManga;

            // Get Writer or Create one
            var author = manga.Authors.First();
            var writer = await _mangaWriterRepo.GetWriterByName(author.Name);
            writer ??= new MangaWriter
            {
                Name = author.Name,
            };

            // Get Tags
            var tags = new List<MangaTag>();
            var mangaGenres = manga.Genres;
            foreach (var genre in mangaGenres)
            {
                var tag = await _mangaTagRepo.GetTagByName(genre.Name);
                tag ??= new MangaTag
                {
                    Name = genre.Name,
                };

                tags.Add(tag);
            }

            var images = new List<string>
            {
                manga.Images.JPG.ImageUrl
            };

            long? aniListId = null;
            var synonyms = new List<string>();
            int? anilistPopularity = null;
            int? anilistAverageScore = null;
            // Try to get Anilist Id aswell
            try
            {
                var aniListManga = await AniListHelper.GetMangaByMALId(manga.MalId);
                aniListId = aniListManga.Id;
                synonyms = aniListManga.Synonyms;
                anilistPopularity = aniListManga.Popularity;
                anilistAverageScore = aniListManga.AverageScore ?? aniListManga.MeanScore;

                if (!string.IsNullOrEmpty(aniListManga.CoverImage.Large))
                    images.Add(aniListManga.CoverImage.Large);
            }
            catch { }


            return await SaveMangaInDb(new Core.Entities.Mangas.Manga
            {
                MangaMyAnimeListID = manga.MalId,
                MangaAniListID = aniListId,
                Name = manga.Titles.FirstOrDefault(c => c.Type == "Default")?.Title ?? manga.Title,
                AlternativeNames = manga.Titles.Select(t => new MangaAlternativeName { Language = t.Type, Name = t.Title }).ToList(),
                Synonyms = synonyms.Select(d => new MangaSynonym { Name = d }).ToList(),
                IsReleasing = manga.Publishing,
                Type = ConvertMALMangaType(manga.Type),
                Description = manga.Synopsis,
                Writer = writer,
                Tags = tags,
                ImagesUrls = images,
                IsNSFW = MangasHelper.IsMangaNSFW(tags.Select(t => t.Name)),
                MALPopularity = manga.Members,
                MALScore = manga.Score.HasValue ? decimal.ToDouble(manga.Score.Value) : null,
                ALPopularity = anilistPopularity,
                ALScore = anilistAverageScore / (double)10,
                StartedAt = manga.Published.From is not null ? DateTime.SpecifyKind(manga.Published.From.Value, DateTimeKind.Utc) : null,
                FinishedAt = manga.Published.To is not null ? DateTime.SpecifyKind(manga.Published.To.Value, DateTimeKind.Utc) : null,
            }, userId);
        }


        /// <summary>
        /// Adds a new AL Manga to the DB, if already exists just returns it
        /// </summary>
        /// <param name="mangaId">Anilist Id</param>
        /// <returns></returns>
        private async Task<Core.Entities.Mangas.Manga> AddALManga(long mangaId, int? userId)
        {
            // Check if already exists
            var dbManga = await _mangaRepo.GetByALId(mangaId);
            if (dbManga != null) return dbManga;


            var manga = await AniListHelper.GetMangaById(mangaId);
            return await AddALManga(manga, userId);
        }
        private async Task<Core.Entities.Mangas.Manga> AddALManga(AniListHelper.ALManga manga, int? userId)
        {
            // Check if already exists
            var dbManga = await _mangaRepo.GetByALId(manga.Id);
            if (dbManga != null) return dbManga;

            //If exists in MAL its preferred
            if (manga.IdMal != null)
            {
                return await AddMALManga(manga.IdMal.Value, userId);
            }


            // Get Writer or Create one
            var firstAuthor = manga.Staff.Edges.FirstOrDefault(s => s.Role.ToLower().Contains("original story"));
            var secondAuthor = manga.Staff.Edges.First(s => s.Role.ToLower().Contains("story"));
            var author = firstAuthor ?? secondAuthor;
            var authorData = manga.Staff.Nodes.Where(n => n.Id == author.Node.Id).First();
            var writer = await _mangaWriterRepo.GetWriterByName(authorData.Name.Full);
            writer ??= new MangaWriter
            {
                Name = authorData.Name.Full,
            };

            // Get Tags
            var tags = new List<MangaTag>();
            var mangaGenres = manga.Genres;
            foreach (var genre in mangaGenres)
            {
                var tag = await _mangaTagRepo.GetTagByName(genre);
                tag ??= new MangaTag
                {
                    Name = genre,
                };

                tags.Add(tag);
            }

            var alternativeNames = new List<MangaAlternativeName>();

            if (!string.IsNullOrEmpty(manga.Title.English))
                alternativeNames.Add(new MangaAlternativeName
                {
                    Language = "English",
                    Name = manga.Title.English
                });
            if (!string.IsNullOrEmpty(manga.Title.Native))
                alternativeNames.Add(new MangaAlternativeName
                {
                    Language = "Native",
                    Name = manga.Title.Native

                });
            if (!string.IsNullOrEmpty(manga.Title.Romaji))
                alternativeNames.Add(new MangaAlternativeName
                {
                    Language = "Romaji",
                    Name = manga.Title.Romaji
                });

            var type = ConvertMALMangaType(manga.CountryOfOrigin);
            return await SaveMangaInDb(new Core.Entities.Mangas.Manga
            {
                MangaAniListID = manga.Id,
                MangaMyAnimeListID = manga.IdMal,
                Name = type != MangaTypeEnum.MANGA && !string.IsNullOrEmpty(manga.Title.English) ? manga.Title.English : manga.Title.UserPreferred,
                AlternativeNames = alternativeNames,
                Synonyms = manga.Synonyms.Select(d => new MangaSynonym { Name = d }).ToList(),
                IsReleasing = manga.Status == "RELEASING",
                Type = type,
                Description = manga.Description,
                Writer = writer,
                Tags = tags,
                IsNSFW = MangasHelper.IsMangaNSFW(tags.Select(t => t.Name)),
                ALPopularity = manga.Popularity,
                ALScore = manga.AverageScore is not null ? manga.AverageScore / (double)10 : manga.MeanScore / (double)10,
                ImagesUrls = new List<string> { manga.CoverImage.Large },
                StartedAt = manga.StartDate.Year is not null ? DateTime.SpecifyKind(new DateTime(manga.StartDate.Year.Value, manga.StartDate.Month ?? 1, manga.StartDate.Day ?? 1), DateTimeKind.Utc) : null,
                FinishedAt = manga.EndDate.Year is not null ? DateTime.SpecifyKind(new DateTime(manga.EndDate.Year.Value, manga.EndDate.Month ?? 1, manga.EndDate.Day ?? 1), DateTimeKind.Utc) : null,
            }, userId);
        }

        private async Task<Core.Entities.Mangas.Manga> SaveMangaInDb(Core.Entities.Mangas.Manga manga, int? userId)
        {
            var dbManga = await _mangaRepo.Create(manga);
            Log.Information($"Added new Manga: {manga.Name}");

            if (userId.HasValue)
                await _addedMangaRepo.Create(new AddedMangaAction
                {
                    UserId = userId.Value,
                    Manga = dbManga,
                });

            //TODO: Notify Bot

            try
            {
                // Save Image aswell
                using WebClient webClient = new();
                var mangaNameSimplified = dbManga.Name.NormalizeStringToDirectory();
                webClient.Headers.Add("user-agent", "User Agent");
                var url = manga.ImagesUrls.First();
                var data = webClient.DownloadData(url);
                await _imageService.SaveImage(data, $"mangas/{mangaNameSimplified}/thumbnail.{url.Split(".").Last()}");
            }
            catch
            {
                Log.Error($"Error saving thumbnail image for: {dbManga.Name}");
            }

            return dbManga;
        }
        #endregion

        public async Task StartMangaChaptersUpdate(int mangaId, int userId)
        {
            var user = await _userRepository.GetById(userId) ?? throw new EntityNotFoundException("User");
            var manga = await _mangaRepo.GetById(mangaId) ?? throw new EntityNotFoundException("Manga");
            if (!manga.Sources.Any())
                throw new Exception("Manga has no Sources");

            _queue.AddToPriorityQueue(new QueuedManga { MangaId = manga.Id, QueuedAt = DateTime.UtcNow, QueuedByUser = userId});
        }

        public async Task UpdateMangaChapters(QueuedManga queuedManga)
        {
            var manga = await _mangaRepo.GetById(queuedManga.MangaId);
            Log.Information($"Scrapping {manga.Name}");

            // Reset IsWorking Status
            foreach(var chapter in manga.Chapters)
            {
                foreach(var translation in chapter.Translations)
                {
                    if (!translation.Downloaded)
                        translation.IsWorking = false;
                }
            }

            // Put in Cache
            var dtosQueue = new List<QueuedMangaDTO>();
            var user = queuedManga.QueuedByUser != null ? await _userRepository.GetById(queuedManga.QueuedByUser.Value) : null;
            dtosQueue.Add(new QueuedMangaDTO
            {
                Manga = _mapper.Map<MangaInfoDTO>(manga), 
                Progress = new QueueProgressDTO(),
                QueuedAt = queuedManga.QueuedAt,
                QueuedByUser = user != null ? _mapper.Map<SimpleUserDTO>(user) : null
            });
            _cache.Set(QUEUE_CACHE_KEY, dtosQueue);
            _cache.Set(QUEUE_CACHE_KEY, await RecalculateMangasQueue());

            foreach (var source in manga.Sources)
            {
                await UpdateChaptersFromMangaAndSource(manga, source, manga.Sources.Count);
            }

            // With this here only adds all the chapters at the same time
            await _mangaRepo.Update(manga);
            await _fusionCache.ExpireAsync($"recent_chapters_{true}");
            await _fusionCache.ExpireAsync($"recent_chapters_{false}");

            // Notify it ended
            var cachedQueue = _cache.Get<List<QueuedMangaDTO>>(QUEUE_CACHE_KEY);
            cachedQueue?.RemoveAt(0);
            _cache.Set(QUEUE_CACHE_KEY, cachedQueue);
            cachedQueue = _cache.Set(QUEUE_CACHE_KEY, await RecalculateMangasQueue());
            await _mangasHub.Clients.All.SendMangasQueue(cachedQueue);
        }
        private async Task<List<QueuedMangaDTO>> RecalculateMangasQueue()
        {
            List<QueuedMangaDTO> cachedQueue = _cache.Get<List<QueuedMangaDTO>>(QUEUE_CACHE_KEY)!;
            var updatedQueue = _queue.GetNextInQueue();

            // Get New Ones
            var newQueue = new List<QueuedMangaDTO>();
            if (cachedQueue!.Any())
                newQueue.Add(cachedQueue[0]);
            foreach(var manga in updatedQueue)
            {
                var queuedManga = cachedQueue.FirstOrDefault(cm => cm.Manga.Id == manga.MangaId && cm.QueuedByUser?.Id == manga.QueuedByUser && cm.QueuedAt == manga.QueuedAt);
                if (queuedManga is null)
                {
                    var repeatedManga = cachedQueue.Select(cm => cm.Manga).FirstOrDefault(cm => cm.Id == manga.MangaId);
                    repeatedManga ??= _mapper.Map<MangaInfoDTO>(await _mangaRepo.GetById(manga.MangaId));

                    var repeatedUser = manga.QueuedByUser is null ?  null : cachedQueue.Select(cm => cm.QueuedByUser).FirstOrDefault(cm => cm?.Id == manga.QueuedByUser);
                    repeatedUser ??= manga.QueuedByUser is null ? null : _mapper.Map<SimpleUserDTO>(await _userRepository.GetById(manga.QueuedByUser.Value));

                    queuedManga = new QueuedMangaDTO
                    {
                        QueuedAt = manga.QueuedAt,
                        QueuedByUser = repeatedUser,
                        Manga = repeatedManga
                    };
                }
                newQueue.Add(queuedManga!);
            }

            return newQueue;

        }
        public List<QueuedMangaDTO> GetQueueStatus()
        {
            var takeQueue = _cache.TryGetValue(QUEUE_CACHE_KEY, out List<QueuedMangaDTO>? queue);
            return takeQueue ? queue! : new List<QueuedMangaDTO>();
        }

        #region Jobs
        public async Task DownloadImages()
        {
            var mangas = await _mangaRepo.GetAll();
            foreach (var manga in mangas)
            {
                Log.Information($"Downloading Images for: {manga.Name}");
                foreach (var source in manga.Sources)
                {
                    Log.Information($"Source: {source.Source}");
                    try
                    {
                        var scrapperEnum = (MangaSourceEnumDTO)Enum.Parse(typeof(MangaSourceEnumDTO), source.Source);
                        var scrapper = _scrappers.First(s => s.GetMangaSourceEnumDTO() == scrapperEnum);
                        if (scrapper.GetLanguage() != MangaTranslationEnumDTO.PT) continue;

                        var fetchedManga = await scrapper.GetManga(source.Url);
                        if (fetchedManga.Chapters == null || fetchedManga.Chapters.Count == 0) continue;
                        var chapters = manga.Chapters.Where(c => c.Translations.Any(t => t.Language == TranslationLanguageEnum.PT));
                        foreach (var chapter in chapters)
                        {
                            Log.Information($"Chapter: {chapter.Name}");
                            try
                            {
                                var fetchedChapter = fetchedManga.Chapters
                                    .First(c => c.Name.Split(":").First().Split("-").First().Split(" ").First().Replace(",", ".").Trim() ==
                                    chapter.Name.ToString().Replace(",", "."));

                                var translation = chapter.Translations.First(t => t.Language == TranslationLanguageEnum.PT);
                                if (await SaveImage(scrapper, TranslationLanguageEnum.PT, manga.Name, chapter.Name.ToString(), fetchedChapter.Link))
                                {
                                    Log.Debug($"Added");
                                    await Task.Delay(2000);
                                }
                                else
                                    Log.Debug($"Exists");

                                translation.Downloaded = true;
                                await _mangaRepo.Update(manga);

                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Problem saving Image");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Problem saving Image with Source");
                    }

                }

            }
        }
        public async Task UpdateMangasChapters()
        {
            var mangas = await _mangaRepo.GetAll();

            foreach (var manga in mangas)
            {
                if (manga.Sources.Any())
                    _queue.AddToQueue(new QueuedManga { MangaId  = manga.Id, QueuedAt = DateTime.UtcNow});
            }

        }
        public async Task AddOrUpdateAllMangasMetadata()
        {
            Log.Information($"Started Updating Mangas Metadata");
            // Update Mangas Metadata
            var updatedMangas = await UpdateMangasMetadata();

            Log.Information($"Updated {updatedMangas} mangas metadata");

            Log.Information("Waiting");
            await Task.Delay(60000);

            Log.Information("Started Adding Popular Mangas");

            // Get & Update MAL Mangas
            Log.Information("MyAnimeList Mangas");
            for (int i = 1; i < 7; i++)
            {
                var topMangasMAL = await _jikan.GetTopMangaAsync(i);
                var malList = topMangasMAL.Data.FilterCorrectTypes();
                foreach (var manga in malList)
                {
                    await AddMALManga(manga, null);
                }
            }

            Log.Information("Waiting");
            await Task.Delay(60000);

            //AL
            Log.Information("AniList Mangas");
            var topMangasAL = await AniListHelper.GetPopularMangas();
            foreach (var manga in topMangasAL)
            {
                await AddALManga(manga.Id, null);
            }

            Log.Information("Waiting");
            await Task.Delay(60000);

            Log.Information("AniList Manhwas");
            var topKRMangasAL = await AniListHelper.GetPopularManhwas();
            foreach (var manga in topKRMangasAL)
            {
                await AddALManga(manga, null);
            }

            Log.Information("Waiting");
            await Task.Delay(60000);

            Log.Information("AniList Manhuas");
            var topCHMangasAL = await AniListHelper.GetPopularManhuas();
            foreach (var manga in topCHMangasAL)
            {
                await AddALManga(manga, null);
            }

            Log.Information("Finished Adding Popular Mangas");

        }
        private async Task<int> UpdateMangasMetadata()
        {
            int updatedMangas = 0;

            var mangas = await _mangaRepo.GetAll();
            foreach (var manga in mangas)
            {
                bool changed = false;

                // Get MAL first
                if (manga.MangaMyAnimeListID != null)
                {
                    var mangaMetadata = (await _jikan.GetMangaAsync(manga.MangaMyAnimeListID.Value)).Data;

                    if (!MangasHelper.IsMALMangaCorrectType(mangaMetadata))
                    {
                        Log.Information($"Deleting {manga.Name} because type is: {mangaMetadata.Type}");
                        await _mangaRepo.Delete(manga.Id);
                        continue;
                    }

                    //Update If needed
                    if (mangaMetadata.Synopsis != manga.Description)
                    {
                        manga.Description = mangaMetadata.Synopsis;
                        changed = true;
                    }

                    if (mangaMetadata.Published.To != null && manga.FinishedAt == null)
                    {
                        manga.FinishedAt = DateTime.SpecifyKind(mangaMetadata.Published.To.Value, DateTimeKind.Utc);
                        manga.IsReleasing = false;
                        changed = true;
                    }

                    if (!manga.ImagesUrls.Contains(mangaMetadata.Images.JPG.ImageUrl))
                    {
                        manga.ImagesUrls = new List<string>
                        {
                            mangaMetadata.Images.JPG.ImageUrl
                        };
                        changed = true;
                    }

                    if (manga.MALPopularity != mangaMetadata.Members)
                    {
                        manga.MALPopularity = mangaMetadata.Members;
                        changed = true;
                    }

                    if (mangaMetadata.Score.HasValue)
                    {
                        if (manga.MALScore != decimal.ToDouble(mangaMetadata.Score.Value))
                        {
                            manga.MALScore = decimal.ToDouble(mangaMetadata.Score.Value);
                            changed = true;
                        }
                    }

                    var isNSFW = MangasHelper.IsMangaNSFW(manga.Tags.Select(t => t.Name));
                    if (isNSFW != manga.IsNSFW)
                    {
                        manga.IsNSFW = isNSFW;
                        changed = true;
                    }
                }

                // Get AL second
                if (manga.MangaAniListID != null)
                {
                    var mangaMetadata = await AniListHelper.GetMangaById(manga.MangaAniListID.Value);

                    if (mangaMetadata.Format.ToLowerInvariant() != "manga")
                    {
                        Log.Information($"Deleting {manga.Name} because format is: {mangaMetadata.Format}");
                        await _mangaRepo.Delete(manga.Id);
                        continue;
                    }

                    //Only if MAL not found you update here
                    if (!changed && manga.MangaMyAnimeListID == null)
                    {

                        //Update If needed
                        if (mangaMetadata.Description != manga.Description)
                        {
                            manga.Description = mangaMetadata.Description;
                            changed = true;
                        }

                        if (mangaMetadata.EndDate.Year != null && manga.FinishedAt == null)
                        {
                            manga.FinishedAt = DateTime.SpecifyKind(new DateTime(mangaMetadata.EndDate.Year.Value, mangaMetadata.EndDate.Month ?? 1, mangaMetadata.EndDate.Day ?? 1), DateTimeKind.Utc);
                            manga.IsReleasing = false;
                            changed = true;
                        }

                        if (!manga.ImagesUrls.Contains(mangaMetadata.CoverImage.Large))
                        {
                            manga.ImagesUrls = new List<string>
                            {
                                mangaMetadata.CoverImage.Large
                            };
                            changed = true;
                        }
                    }

                    //Update title when is KR or CN
                    if (manga.Type != MangaTypeEnum.MANGA && !string.IsNullOrEmpty(mangaMetadata.Title.English) && manga.Name != mangaMetadata.Title.English)
                    {
                        manga.Name = mangaMetadata.Title.English;
                        changed = true;
                    }

                    //Get Tags if MAL doesn't have
                    if (manga.Tags == null || !manga.Tags.Any())
                    {
                        // Get Tags
                        var tags = new List<MangaTag>();
                        var mangaGenres = mangaMetadata.Genres;
                        foreach (var genre in mangaGenres)
                        {
                            var tag = await _mangaTagRepo.GetTagByName(genre);
                            tag ??= new MangaTag
                            {
                                Name = genre,
                            };

                            tags.Add(tag);
                        }

                        manga.Tags = tags;
                        changed = true;
                    }

                    var isNSFW = MangasHelper.IsMangaNSFW(manga.Tags.Select(t => t.Name)); 
                    if (isNSFW != manga.IsNSFW)
                    {
                        manga.IsNSFW = isNSFW;
                        changed = true;
                    }

                    if (manga.ALPopularity != mangaMetadata.Popularity)
                    {
                        manga.ALPopularity = mangaMetadata.Popularity;
                        changed = true;
                    }

                    if (mangaMetadata.AverageScore.HasValue)
                    {
                        if (manga.ALScore != mangaMetadata.AverageScore.Value / (double)10)
                        {
                            manga.ALScore = mangaMetadata.AverageScore.Value / (double)10;
                            changed = true;
                        }
                    }
                    else if (mangaMetadata.MeanScore.HasValue)
                    {
                        if (manga.ALScore != mangaMetadata.MeanScore.Value / (double)10)
                        {
                            manga.ALScore = mangaMetadata.MeanScore.Value / (double)10;
                            changed = true;
                        }
                    }
                }

                if (changed)
                {
                    Log.Information($"Manga {manga.Name} just updated metadata");
                    await _mangaRepo.Update(manga);
                    updatedMangas++;
                    await Task.Delay(1000);
                }
            }

            return updatedMangas;
        }
        public async Task FetchSeasonMangas()
        {
            Log.Information("Reseting Season Mangas");

            var allMangas = await _mangaRepo.GetAll();
            foreach (var manga in allMangas)
            {
                if (!manga.IsSeasonManga) continue;
                manga.IsSeasonManga = false;
                await _mangaRepo.Update(manga);
            }
           
            Log.Information("Getting Season Mangas");
            var seasonAnimes = await _jikan.GetCurrentSeasonAsync();
            await Task.Delay(1000);

            foreach (var anime in seasonAnimes.Data)
            {
                try
                {
                    if (anime is null)
                        continue;

                    var relations = await _jikan.GetAnimeRelationsAsync(anime.MalId!.Value);
                    var mangaId = relations.Data.Where(r => r.Relation == "Adaptation" && r.Entry.First().Type == "manga").Select(s => s.Entry.First().MalId).First();

                    var dbManga = allMangas.Where(m => m.MangaMyAnimeListID == mangaId).FirstOrDefault();
                    BaseJikanResponse<JikanDotNet.Manga>? manga = null;
                    if (dbManga is null)
                    {
                        await Task.Delay(350);

                        // Fetch First Degree Relation
                        manga = await _jikan.GetMangaAsync(mangaId);
                        if (manga is null)
                            continue;

                        var isMangaType = MangasHelper.IsMALMangaCorrectType(manga!.Data);
                        if (!isMangaType)
                        {
                            await Task.Delay(350);

                            // Fetch Second Degree Relation
                            var relationsManga = await _jikan.GetMangaRelationsAsync(mangaId);
                            mangaId = relationsManga.Data.Where(r => r.Relation == "Alternative version" && r.Entry.First().Type == "manga").Select(s => s.Entry.First().MalId).First();
                            
                            dbManga = allMangas.Where(m => m.MangaMyAnimeListID == mangaId).FirstOrDefault();

                            // Get the Manga & check the type
                            if (dbManga is null)
                            {
                                await Task.Delay(350);
                                manga = await _jikan.GetMangaAsync(mangaId);
                                isMangaType = MangasHelper.IsMALMangaCorrectType(manga!.Data);

                                if (!isMangaType)
                                    continue;
                            }
                        }
                    }

                    dbManga ??= await AddMALManga(manga!.Data, null);

                    dbManga.IsSeasonManga = true;

                    await _mangaRepo.Update(dbManga);
                    await _fusionCache.ExpireAsync($"manga_{dbManga.Id}");
                    await _fusionCache.ExpireAsync($"manga_tags");
                    foreach (var tag in dbManga.Tags)
                    {
                        await _fusionCache.ExpireAsync($"manga_tag_${tag.Name.ToLower()}_true");
                        await _fusionCache.ExpireAsync($"manga_tag_${tag.Name.ToLower()}_false");
                    }
                    await Task.Delay(1000);
                }
                catch(Exception ex)
                {
                    Log.Error($"Error Fetching {anime.Titles.First().Title}", ex);
                }

            }
            await _fusionCache.ExpireAsync("season_mangas_dto");
            Log.Information("Season Mangas Updated");
        }
        #endregion

        #region Private
        private async Task UpdateChaptersFromMangaAndSource(Core.Entities.Mangas.Manga manga, MangaSource source, int maxSources)
        {
            try
            {
                Log.Information($"Source: {source.Source}");

                // Get Scrapper
                var scrapperEnum = (MangaSourceEnumDTO)Enum.Parse(typeof(MangaSourceEnumDTO), source.Source);
                var scrapper = GetScrapperByEnum(scrapperEnum);
                if (scrapper == null)
                    throw new Exception("No Scrapper Registered");
                var scrapperTranslation = _mapper.Map<TranslationLanguageEnum>(scrapper?.GetLanguage());

                // Get (Cached) Manga Info
                var cachedManga = _cache.TryGetValue(source.Url, out ScrappedManga? mangaInfo);
                if (!cachedManga)
                {
                    mangaInfo = await scrapper!.GetManga(source.Url);
                    _cache.Set(source.Url, mangaInfo, DateTimeOffset.Now.AddMinutes(30));
                }

                int scrapperFailures = 0;

                for (int i = 0; i < mangaInfo.Chapters.Count; i++)
                {
                    var chapter = mangaInfo.Chapters[i];

                    // Send to Queue Hub
                    var cachedQueue = _cache.Get<List<QueuedMangaDTO>>(QUEUE_CACHE_KEY);
                    if (cachedQueue is not null)
                    {
                        cachedQueue![0].Progress!.TotalChapters = mangaInfo.Chapters.Count;
                        cachedQueue![0].Progress!.CurrentSource = scrapperEnum;
                        cachedQueue![0].Progress!.CurrentChapter = i + 1;
                        cachedQueue![0].Progress!.Percentage += ((100 / (double)maxSources) / mangaInfo.Chapters.Count);
                        _cache.Set(QUEUE_CACHE_KEY, cachedQueue);

                        var updatedQueue = _cache.Set(QUEUE_CACHE_KEY, await RecalculateMangasQueue());
                        await _mangasHub.Clients.All.SendMangasQueue(updatedQueue);
                    }


                    // Check if its a valid name
                    var nameScrapped = chapter.Name.Split(":").First().Split("-").First().Split(" ").First().Replace(",",".").Trim();
                    if (string.IsNullOrEmpty(nameScrapped) || !double.TryParse(nameScrapped,NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var number))
                        continue;

                    // Get Chapter Translation from db if exists (change all Translations to not work)
                    var dbChapter = manga.Chapters.FirstOrDefault(c => c.Name == number);
                    dbChapter ??= new MangaChapter
                    {
                        Name = number,
                    };
                    dbChapter.Translations ??= new();
                    var dbTranslation = dbChapter.Translations.FirstOrDefault(t => t.Language == scrapperTranslation);

                    // If doesn't exist create
                    if (dbTranslation is null)
                    {
                        try
                        {
                            var translation = new MangaTranslation
                            {
                                ReleasedDate = chapter.ReleasedAt is not null ? DateTime.SpecifyKind(chapter.ReleasedAt.Value, DateTimeKind.Utc) : null,
                                Language = scrapperTranslation,
                                IsWorking = true,
                                Downloaded = false
                            };

                            dbChapter.Translations.Add(translation);
                            manga.Chapters.Add(dbChapter);
                            scrapperFailures = 0;

                            Log.Information($"Added Chapter: {dbChapter.Name} for {scrapperTranslation}");

                        }
                        catch(SavingImageException sie)
                        {
                            Log.Error(sie, $"Chapter {nameScrapped} failed saving image");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Chapter {nameScrapped} failed saving");

                            var sourceFailedTooManyTimesInARow = ++scrapperFailures == 5;
                            if (sourceFailedTooManyTimesInARow)
                            {
                                Log.Error($"Something might be wrong for the {source.Source} and {source.Name}");
                                break;
                            }
                        }
                        finally
                        {
                            await Task.Delay(1500);
                        }
                    }
                    else if (dbTranslation.IsWorking || dbTranslation.Downloaded)
                    {
                        //Avoid rewriting a chapter already working
                        continue;
                    }
                    // If exists update to work
                    else
                    {
                        dbTranslation.IsWorking = true;
                        if (chapter.ReleasedAt.HasValue && (dbTranslation.ReleasedDate is null || DateTime.SpecifyKind(chapter.ReleasedAt.Value, DateTimeKind.Utc) < dbTranslation.ReleasedDate))
                            dbTranslation.ReleasedDate = DateTime.SpecifyKind(chapter.ReleasedAt.Value, DateTimeKind.Utc);
                       
                        Log.Debug($"Chapter {nameScrapped} already in DB");
                    }


                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error Updating {manga.Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }
        private MangaTranslationDTO MapMangaTranslation(MangaTranslation mangaTranslation, string source, List<string> pages, bool changeTranslation)
        {
            var dto = new MangaTranslationDTO
            {
                MangaName = mangaTranslation.MangaChapter.Manga.Name,
                MangaType = _mapper.Map<MangaTypeEnumDTO>(mangaTranslation.MangaChapter.Manga.Type),
                Language = _mapper.Map<MangaTranslationEnumDTO>(mangaTranslation.Language),
                ChapterId = mangaTranslation.MangaChapter.Id,
                ChapterNumber = mangaTranslation.MangaChapter.Name,
                ReleasedAt = mangaTranslation.ReleasedDate,
                Source = source,
                Pages = pages,
                PageHeaders = string.IsNullOrEmpty(source) ?  new Dictionary<string, string>() : GetScrapperByEnum(Enum.Parse<MangaSourceEnumDTO>(source))?.GetImageHeaders(),
                CreatedAt = mangaTranslation.CreatedAt,
            };
            var previousChapter = mangaTranslation.MangaChapter.Manga.Chapters.OrderByDescending(o => o.Name).SkipWhile(s => s.Id != mangaTranslation.MangaChapter.Id).Skip(1).Take(1).FirstOrDefault();
            dto.PreviousChapterId = (changeTranslation || (previousChapter?.Translations.Any(t => t.Language == mangaTranslation.Language) ?? false)) ? previousChapter?.Id : null;

            var nextChapter = mangaTranslation.MangaChapter.Manga.Chapters.OrderBy(o => o.Name).SkipWhile(s => s.Id != mangaTranslation.MangaChapter.Id).Skip(1).Take(1).FirstOrDefault();
            dto.NextChapterId = (changeTranslation || (nextChapter?.Translations.Any(t => t.Language == mangaTranslation.Language) ?? false)) ? nextChapter?.Id : null;
            return dto;
        }
        private static string BuildTranslationFolderPath(string mangaNameNormalized, string translation, string chapterNameNormalized)
        {
            return $"mangas/{mangaNameNormalized}/chapters/{translation}/{chapterNameNormalized}";
        }
        private async Task<bool> SaveImage(IBaseMangaScrapper scrapper, TranslationLanguageEnum scrapperTranslation, string mangaName, string chapterName, string chapterLink, bool replace = false)
        {
            try
            {                
                var mangaNameSimplified = mangaName.NormalizeStringToDirectory();
                var folderPath = BuildTranslationFolderPath(mangaNameSimplified, scrapperTranslation.ToString().ToLower(), chapterName.NormalizeStringToDirectory());
                if (Directory.Exists(folderPath) && !replace)
                    return false;
               

                var chapterPages = await scrapper.GetChapterImages(chapterLink);
                for (int i = 0; i < chapterPages.Count; i++)
                {
                    using WebClient webClient = new();
                    webClient.Headers.Add("user-agent", "User Agent");
                    var page = chapterPages[i];
                    webClient.Headers.Add("referer", $"{scrapper.GetBaseURLForManga()}/{chapterLink}");

                    Log.Debug($"{i}: {page}");
                    var data = webClient.DownloadData(page);
                    await _imageService.SaveImage(data, $"{folderPath}/{i}.{page.Split(".").Last()}");
                    Log.Debug("Saved");
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new SavingImageException(ex.Message);
            }           
        }
        private static MangaTypeEnum ConvertMALMangaType(string mangaType)
        {
            return mangaType.ToLower() switch
            {
                "manga" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANGA,
                "manhua" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANHUA,
                "manhwa" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANHWA,
                "cn" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANHUA,
                "kr" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANHWA,
                "jp" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANGA,
                _ => Core.Entities.Mangas.Enums.MangaTypeEnum.MANGA,
            };
        }
        private IBaseMangaScrapper? GetScrapperByEnum(MangaSourceEnumDTO source)
        {
            return _scrappers.FirstOrDefault(s => s.GetMangaSourceEnumDTO() == source);
        }
        #endregion
    }
}
