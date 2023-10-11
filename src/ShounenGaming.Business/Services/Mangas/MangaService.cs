using AutoMapper;
using JikanDotNet;
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

        public MangaService(IMangaRepository mangaRepo, IMangaWriterRepository mangaWriterRepo, IMangaTagRepository mangaTagRepo, IMapper mapper, IImageService imageService, IJikan jikan, IAddedMangaActionRepository addedMangaRepo, IUserRepository userRepository, IFetchMangasQueue queue, IMemoryCache cache, IHubContext<MangasHub, IMangasHubClient> mangasHub, IFusionCache fusionCache, IEnumerable<IBaseMangaScrapper> scrappers)
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
        }


        public async Task<MangaDTO> GetMangaById(int id)
        {
            var manga = await _fusionCache.GetOrSetAsync($"manga_{id}", async _ =>  await _mangaRepo.GetById(id));
            if (manga is null)
                throw new EntityNotFoundException("Manga");

            return _mapper.Map<MangaDTO>(manga);
        }
        public async Task<List<MangaSourceDTO>> GetMangaSourcesById(int id)
        {
            var manga = await _fusionCache.GetOrSetAsync($"manga_{id}", async _ => await _mangaRepo.GetById(id));
            return manga != null ? _mapper.Map<List<MangaSourceDTO>>(manga.Sources) : throw new EntityNotFoundException("Manga");
        }
        public async Task<MangaTranslationDTO?> GetMangaTranslation(int userId, int mangaId, int chapterId, MangaTranslationEnumDTO translation)
        {
            var user = await _fusionCache.GetOrSetAsync($"user_{userId}", async _ => await _userRepository.GetById(userId));
            if (user is null)
                throw new EntityNotFoundException("User");

            var manga = await _fusionCache.GetOrSetAsync($"manga_{mangaId}", async _ => await _mangaRepo.GetById(mangaId));
            if (manga is null) 
                throw new EntityNotFoundException("Manga");

            var mangaChapter = manga.Chapters.FirstOrDefault(c => c.Id == chapterId) ?? 
                throw new EntityNotFoundException("MangaChapter");

            var mangaTranslation = mangaChapter.Translations.FirstOrDefault(t => t.Language == _mapper.Map<TranslationLanguageEnum>(translation) && t.IsWorking);
            mangaTranslation ??= mangaChapter.Translations.FirstOrDefault(t => t.IsWorking) ?? throw new EntityNotFoundException("MangaTranslation");

            var selectedSource = string.Empty;
            var pages = new List<string>();

            //Fetch pages runtime
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
                catch { }
            }

            return null;
        }
        public async Task<PaginatedResponse<MangaInfoDTO>> SearchMangas(SearchMangaQueryDTO query, int page, int? userId = null)
        {
            var includeNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _fusionCache.GetOrSetAsync($"user_{userId.Value}", async _ => await _userRepository.GetById(userId.Value));
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
            var seasonMangas = await _fusionCache.GetOrSetAsync($"season_mangas", async _ => await _mangaRepo.GetSeasonMangas());
            return _mapper.Map<List<MangaInfoDTO>>(seasonMangas);
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
        public async Task<List<MangaInfoDTO>> GetFeaturedMangas(int? userId = null)
        {
            var includesNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _userRepository.GetById(userId.Value) ?? throw new EntityNotFoundException("User");
                includesNSFW = user.MangasConfigurations.NSFWBehaviour != NSFWBehaviourEnum.HIDE_ALL;
                showProgressAll = user.MangasConfigurations.ShowProgressForChaptersWithDecimals;
            }

            var featuredMangas = await _mangaRepo.GetFeaturedMangas(includesNSFW);

            if (!showProgressAll)
                featuredMangas.ForEach(m => m.Chapters = m.Chapters.Where(c => (c.Name % 1) == 0).ToList());

            return _mapper.Map<List<MangaInfoDTO>>(featuredMangas);
        }
        public async Task<List<MangaInfoDTO>> GetRecentlyAddedMangas()
        {
            var mangas = await _fusionCache.GetOrSetAsync($"recent_mangas", async _ => await _mangaRepo.GetRecentlyAddedMangas());
            return _mapper.Map<List<MangaInfoDTO>>(mangas);
        }
        public async Task<List<LatestReleaseMangaDTO>> GetRecentlyReleasedChapters(int? userId = null)
        {
            var includeNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _fusionCache.GetOrSetAsync($"user_{userId}", async _ => await _userRepository.GetById(userId.Value));
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
            await _fusionCache.SetAsync($"recent_chapters_{includeNSFW}", dto, options: new FusionCacheEntryOptions { Duration = TimeSpan.FromMinutes(5)});
            return dto;
        }
        public async Task<List<MangaWriterDTO>> GetMangaWriters()
        {
            var writers = await _fusionCache.GetOrSetAsync($"manga_writers", async _ => await _mangaWriterRepo.GetAll());
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

        public async Task<MangaInfoDTO> ChangeMangaFeaturedStatus(int mangaId)
        {
            var manga = await _mangaRepo.GetById(mangaId) ?? throw new EntityNotFoundException("Manga");
            manga.IsFeatured = !manga.IsFeatured;
            manga = await _mangaRepo.Update(manga);
            return _mapper.Map<MangaInfoDTO>(manga);
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
                        var titles = new List<string>();
                        if (m.Title.UserPreferred != null)
                            titles.Add(m.Title.UserPreferred);

                        if (m.Title.English != null)
                            titles.Add(m.Title.English);

                        if (m.Title.Native != null)
                            titles.Add(m.Title.Native);

                        if (m.Title.Romaji != null)
                            titles.Add(m.Title.Romaji);


                        alDtos.Add(new MangaMetadataDTO
                        {
                            Id = m.Id,
                            Titles = titles,
                            AlreadyExists = await _mangaRepo.MangaExistsByAL(m.Id),
                            Description = m.Description,
                            Tags = m.Genres.ToList(),
                            Status = m.Status,
                            Score = m.AverageScore,
                            FinishedAt = m.EndDate.Year != null ? new DateTime(m.EndDate.Year.Value, m.EndDate.Month ?? 1, m.EndDate.Day ?? 1) : null,
                            ImageUrl = m.CoverImage.Large,
                            StartedAt = m.StartDate.Year != null ? new DateTime(m.StartDate.Year.Value, m.StartDate.Month ?? 1, m.StartDate.Day ?? 1) : null,
                            Type = Enum.GetName(typeof(MangaTypeEnum), ConvertMALMangaType(m.CountryOfOrigin)) ?? "",
                            Source = MangaMetadataSourceEnumDTO.ANILIST
                        });
                    }
                    return alDtos;

                case MangaMetadataSourceEnumDTO.MYANIMELIST:
                    var malMangas = (await _jikan.SearchMangaAsync(name)).Data;

                    // Filter only allowed types
                    malMangas = malMangas.FilterCorrectTypes();

                    var malDtos = new List<MangaMetadataDTO>();
                    foreach(var m in malMangas)
                    {
                        malDtos.Add(new MangaMetadataDTO
                        {
                            Id = m.MalId,
                            Titles = m.Titles.Select(t => t.Title).ToList(),
                            Description = m.Synopsis,
                            FinishedAt = m.Published.To,
                            AlreadyExists = await _mangaRepo.MangaExistsByMAL(m.MalId),
                            Type = m.Type,
                            Status = m.Status,
                            StartedAt = m.Published.From,
                            ImageUrl = m.Images.JPG.ImageUrl,
                            Score = m.Score,
                            Tags = m.Genres.Select(g => g.Name).ToList(),
                            Source = MangaMetadataSourceEnumDTO.MYANIMELIST
                        });
                    }
                    return malDtos;
                   
                default:
                    return new List<MangaMetadataDTO>();
            }
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

            List<Task<List<MangaSourceDTO>>> allMangasTasks = new();

            foreach(var scrapper in _scrappers)
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
        }

        public async Task<List<MangaSourceDTO>> LinkSourcesToManga(int mangaId, List<MangaSourceDTO> mangas)
        {
            var manga = await _mangaRepo.ClearSources(mangaId);
            if (manga is null)
                throw new EntityNotFoundException("Manga");

            manga.Sources = mangas.Select(m => new MangaSource { Source = m.Source.ToString(), Url = m.Url , ImageURL = m.ImageURL, Name = m.Name}).ToList();

            var dbManga = await _mangaRepo.Update(manga);
            return _mapper.Map<List<MangaSourceDTO>>(dbManga.Sources);
        }

        public async Task<MangaDTO> AddManga(MangaMetadataSourceEnumDTO source, long mangaId, int userId)
        {
            var user = await _userRepository.GetById(userId) ?? throw new EntityNotFoundException("User");

            //TODO: Handle Exceptions (Timeout and wrong id)
            var manga = source == MangaMetadataSourceEnumDTO.MYANIMELIST ? await AddMALManga(mangaId, user.Id) : await AddALManga(mangaId, user.Id);
            if (manga is null)
                throw new Exception("Couldn't create or get the Manga");

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
                anilistAverageScore = aniListManga.AverageScore;

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
                ALScore = manga.AverageScore / (double)10,
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
            var updatedQueue = _queue.GetNextInQueue().Take(20);

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



        public async Task DownloadImages()
        {
            var mangas = await _mangaRepo.GetAll();
            foreach (var manga in mangas)
            {
                Log.Information($"Downloading Images for: {manga.Name}");
                foreach (var source in manga.Sources)
                {
                    Log.Information($"Source: {source}");

                    var scrapperEnum = (MangaSourceEnumDTO)Enum.Parse(typeof(MangaSourceEnumDTO), source.Source);
                    var scrapper = _scrappers.First(s => s.GetMangaSourceEnumDTO() == scrapperEnum);

                    var fetchedManga = await scrapper.GetManga(source.Url);
                    var chapters = manga.Chapters.Where(c => c.Translations.Any(t => !t.Downloaded && t.Language == TranslationLanguageEnum.PT));
                    foreach (var chapter in chapters)
                    {
                        Log.Information($"Chapter: {chapter.Name}");
                        try
                        {
                            var fetchedChapter = fetchedManga.Chapters
                                .First(c => c.Name.Split(":").First().Split("-").First().Split(" ").First().Replace(",", ".").Trim() ==
                                chapter.Name.ToString().Replace(",", "."));

                            var translation = chapter.Translations.First(t => t.Language == TranslationLanguageEnum.PT);
                            await SaveImage(scrapper, TranslationLanguageEnum.PT, manga.Name, chapter.Name.ToString(), fetchedChapter.Link);

                            translation.Downloaded = true;
                            await _mangaRepo.Update(manga);

                            await Task.Delay(2000);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                        }
                    }
                }

            }
        }


        #region Jobs
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
                        manga.ImagesUrls.Add(mangaMetadata.Images.JPG.ImageUrl);
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
                            manga.ImagesUrls.Add(mangaMetadata.CoverImage.Large);
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
                    await _fusionCache.ExpireAsync($"manga_writers");
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
            await _fusionCache.ExpireAsync("recent_mangas");
            await _fusionCache.ExpireAsync("recent_chapters_true");
            await _fusionCache.ExpireAsync("recent_chapters_false");
            await _fusionCache.ExpireAsync("season_mangas");
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

                mangaInfo!.Chapters.Reverse();

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

                            //TODO: Send to Discord Bot to notify new Chapter Release
                            manga.Chapters.Add(dbChapter);
                            scrapperFailures = 0;

                            Log.Information($"Added Chapter: {dbChapter.Name} for {scrapperTranslation}");
                            await Task.Delay(1500);
                            // TODO : Remove Later
#if DEBUG && false
                            if (scrapperTranslation == TranslationLanguageEnum.PT)
                            {
                                await SaveImage(scrapper, scrapperTranslation, manga.Name, dbChapter.Name.ToString(), chapter.Link);
                                translation.Downloaded = true;
                                await Task.Delay(2000);
                            }
#endif

                        }
                        catch(SavingImageException sie)
                        {
                            Log.Error($"Chapter {nameScrapped} failed saving image: {sie.Message}\n{sie.StackTrace}");
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Chapter {nameScrapped} failed saving: {ex.Message}\n{ex.StackTrace}");

                            var sourceFailedTooManyTimesInARow = ++scrapperFailures == 5;
                            if (sourceFailedTooManyTimesInARow)
                            {
                                Log.Error($"Something might be wrong for the {source.Source} and {source.Name}");
                                break;
                            }
                        }
                    }
                    else if (dbTranslation.IsWorking)
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
                       
                        Log.Debug($"Chapter {nameScrapped} Already in DB");
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
                PageHeaders = GetScrapperByEnum(Enum.Parse<MangaSourceEnumDTO>(source))?.GetImageHeaders(),
                CreatedAt = mangaTranslation.CreatedAt,
            };
            var previousChapter = mangaTranslation.MangaChapter.Manga.Chapters.OrderByDescending(o => o.Name).SkipWhile(s => s.Id != mangaTranslation.MangaChapter.Id).Skip(1).Take(1).FirstOrDefault();
            dto.PreviousChapterId = (changeTranslation || (previousChapter?.Translations.Any(t => t.Language == mangaTranslation.Language) ?? false)) ? previousChapter?.Id : null;

            var nextChapter = mangaTranslation.MangaChapter.Manga.Chapters.OrderBy(o => o.Name).SkipWhile(s => s.Id != mangaTranslation.MangaChapter.Id).Skip(1).Take(1).FirstOrDefault();
            dto.NextChapterId = (changeTranslation || (nextChapter?.Translations.Any(t => t.Language == mangaTranslation.Language) ?? false)) ? nextChapter?.Id : null;
            return dto;
        }
        private async Task SaveImage(IBaseMangaScrapper scrapper, TranslationLanguageEnum scrapperTranslation, string mangaName, string chapterName, string chapterLink)
        {
            // TODO : Verify folder before adding
            try
            {
                var chapterPages = await scrapper.GetChapterImages(chapterLink);

                //Check if images are working, if so, add them to filedata
                
                var mangaNameSimplified = mangaName.NormalizeStringToDirectory();
                
                for (int i = 0; i < chapterPages.Count; i++)
                {
                    using WebClient webClient = new();
                    webClient.Headers.Add("user-agent", "User Agent");
                    var page = chapterPages[i];
                    webClient.Headers.Add("referer", $"{scrapper.GetBaseURLForManga()}/{chapterLink}");
                    Log.Information($"{i}: {page}");
                    var data = webClient.DownloadData(page);
                    await _imageService.SaveImage(data, $"mangas/{mangaNameSimplified}/chapters/{scrapperTranslation.ToString().ToLower()}/{chapterName.NormalizeStringToDirectory()}/{i}.{page.Split(".").Last()}");
                }
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
