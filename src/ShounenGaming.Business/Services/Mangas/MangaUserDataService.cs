﻿using AutoMapper;
using Serilog;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DataAccess.Interfaces.Base;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.DTOs.Models.Base;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.Business.Services.Mangas
{
    public class MangaUserDataService : IMangaUserDataService
    {
        private readonly IUserRepository _userRepo;
        private readonly IMangaRepository _mangaRepository;
        private readonly IMangaUserDataRepository _mangaUserDataRepo;
        private readonly IChangedChapterStateActionRepository _mangaChangedChapterStateActionRepo;
        private readonly IChangedMangaStatusActionRepository _mangaChangedStatusActionRepo;

        private readonly IMapper _mapper;

        public MangaUserDataService(IMangaUserDataRepository mangaUserDataRepo, IMapper mapper, IMangaRepository mangaRepository, IChangedChapterStateActionRepository mangaChangedChapterStateActionRepo, IChangedMangaStatusActionRepository mangaStatusActionRepo, IUserRepository userRepo)
        {
            _mangaUserDataRepo = mangaUserDataRepo;
            _mapper = mapper;
            _mangaRepository = mangaRepository;
            _mangaChangedChapterStateActionRepo = mangaChangedChapterStateActionRepo;
            _mangaChangedStatusActionRepo = mangaStatusActionRepo;
            _userRepo = userRepo;
        }

        public async Task<MangaUserDataDTO?> GetMangaDataByMangaByUser(int userId, int mangaId)
        {
            var user = await _userRepo.GetById(userId) ?? throw new EntityNotFoundException("User");
            var info = await _mangaUserDataRepo.GetByUserAndManga(userId, mangaId);
            if (info is null) return null;
            return await MapMangaUserData(info);
        }

        public async Task<List<MangaUserDataDTO>> GetMangasByStatusByUser(int userId, MangaUserStatusEnumDTO status)
        {
            var user = await _userRepo.GetById(userId) ?? throw new EntityNotFoundException("User");
            var mangas = await _mangaUserDataRepo.GetMangasByStatusByUser(_mapper.Map<MangaUserStatusEnum>(status), userId);

            var dtoMangas = new List<MangaUserDataDTO>();
            foreach (var manga in mangas)
            {
                dtoMangas.Add(await MapMangaUserData(manga));
            }
            return dtoMangas;
        }        

        public async Task<MangaUserDataDTO> MarkChaptersRead(int userId, List<int> chaptersIds)
        {
            var user = await _userRepo.GetById(userId) ?? throw new EntityNotFoundException("User");
            var manga = await _mangaRepository.GetByChapters(chaptersIds) ?? throw new Exception("Manga not Found");
            var mangaUserInfo = await _mangaUserDataRepo.GetByUserAndManga(userId, manga.Id);

            //First Chapter Seen
            if (mangaUserInfo is null)
            {
                mangaUserInfo = new Core.Entities.Mangas.MangaUserData
                {
                    MangaId = manga.Id,
                    UserId = userId,
                    User = user,
                    Status = MangaUserStatusEnum.READING,
                    IsPrivate = false,
                    ChaptersRead = new List<Core.Entities.Mangas.MangaChapter>(),
                };

                mangaUserInfo = await _mangaUserDataRepo.Create(mangaUserInfo);
                await _mangaChangedStatusActionRepo.Create(new Core.Entities.Mangas.ChangedMangaStatusAction
                {
                    UserId = userId,
                    MangaId = manga.Id,
                    PreviousState = null,
                    NewState = mangaUserInfo.Status
                });
            }

            var actions = new List<Core.Entities.Mangas.ChangedChapterStateAction>();
            foreach (var chapterId in chaptersIds.Distinct())
            {
                var chapter = manga.Chapters.First(c => c.Id == chapterId);

                //Chapter Not Seen Yet
                if (!mangaUserInfo.ChaptersRead.Any(c => c.Id == chapterId))
                {
                    mangaUserInfo.ChaptersRead.Add(chapter);
                    actions.Add(new Core.Entities.Mangas.ChangedChapterStateAction
                    {
                        ChapterId = chapterId,
                        UserId = userId,
                        Read = true,
                    });
                }

            }

            mangaUserInfo = await _mangaUserDataRepo.Update(mangaUserInfo);
            await _mangaChangedChapterStateActionRepo.CreateBulk(actions);

            return await MapMangaUserData(mangaUserInfo);
        }

        public async Task<MangaUserDataDTO?> UnmarkChaptersRead(int userId, List<int> chaptersIds)
        {
            var user = await _userRepo.GetById(userId) ?? throw new EntityNotFoundException("User");
            var manga = await _mangaRepository.GetByChapters(chaptersIds) ?? throw new Exception("Manga not Found");
            var mangaUserInfo = await _mangaUserDataRepo.GetByUserAndManga(userId, manga.Id);
            if (mangaUserInfo is null) return null;

            var actions = new List<Core.Entities.Mangas.ChangedChapterStateAction>();
            foreach (var chapterId in chaptersIds)
            {
                var chapter = manga.Chapters.First(c => c.Id == chapterId);

                if (mangaUserInfo.ChaptersRead.Any(c => c.Id == chapterId))
                {
                    mangaUserInfo.ChaptersRead.Remove(chapter);
                    actions.Add(new Core.Entities.Mangas.ChangedChapterStateAction
                    {
                        ChapterId = chapterId,
                        UserId = userId,
                        Read = false,
                    });
                }
            }

            mangaUserInfo = await _mangaUserDataRepo.Update(mangaUserInfo);
            await _mangaChangedChapterStateActionRepo.CreateBulk(actions);

            return await MapMangaUserData(mangaUserInfo);
        }

        public async Task<MangaUserDataDTO?> UpdateMangaStatusByUser(int userId, int mangaId, MangaUserStatusEnumDTO? status)
        {
            var user = await _userRepo.GetById(userId) ?? throw new EntityNotFoundException("User");
            var manga = await _mangaRepository.GetById(mangaId) ?? throw new EntityNotFoundException("Manga");
            var mangaUserInfo = await _mangaUserDataRepo.GetByUserAndManga(userId, manga.Id);

            //If first time
            if (mangaUserInfo is null)
            {
                if (status == null) return null;

                mangaUserInfo = new Core.Entities.Mangas.MangaUserData
                {
                    MangaId = manga.Id,
                    UserId = userId,
                    User = user,
                    IsPrivate = false,
                    Status = _mapper.Map<MangaUserStatusEnum>(status)
                };
                var dbMangaUserInfo = await _mangaUserDataRepo.Create(mangaUserInfo);

                await _mangaChangedStatusActionRepo.Create(new Core.Entities.Mangas.ChangedMangaStatusAction
                {
                    UserId = userId,
                    MangaId = mangaId,
                    PreviousState = null,
                    NewState = mangaUserInfo.Status
                });

                return await MapMangaUserData(dbMangaUserInfo);
            } 
            else
            {
                if (status == null)
                {
                    if (mangaUserInfo.ChaptersRead.Count == 0)
                    {
                        //Delete 
                        await _mangaChangedStatusActionRepo.Create(new Core.Entities.Mangas.ChangedMangaStatusAction
                        {
                            UserId = userId,
                            MangaId = mangaId,
                            PreviousState = mangaUserInfo.Status,
                            NewState = null
                        });

                        await _mangaUserDataRepo.Delete(mangaUserInfo.Id);

                        return null;
                    } 
                    else throw new Exception("Invalid Status");
                }

                //Change
                var previousState = mangaUserInfo.Status;
                mangaUserInfo.Status = _mapper.Map<MangaUserStatusEnum>(status);
                var dbMangaUserInfo = await _mangaUserDataRepo.Update(mangaUserInfo);

                await _mangaChangedStatusActionRepo.Create(new Core.Entities.Mangas.ChangedMangaStatusAction
                {
                    UserId = userId,
                    MangaId = mangaId,
                    PreviousState = previousState,
                    NewState = mangaUserInfo.Status
                });

                return await MapMangaUserData(dbMangaUserInfo);
            }
        }
        public async Task<MangaUserDataDTO?> UpdateMangaRatingByUser(int userId, int mangaId, double? rating)
        {
            if (rating < 0 || rating > 5) throw new Exception($"Invalid Rating ({rating}), only values allowed are between 0 and 5");

            var user = await _userRepo.GetById(userId) ?? throw new EntityNotFoundException("User");
            var manga = await _mangaRepository.GetById(mangaId) ?? throw new EntityNotFoundException("Manga");
            var mangaUserInfo = await _mangaUserDataRepo.GetByUserAndManga(userId, manga.Id);

            //If first time
            if (mangaUserInfo is null)
            {
                if (rating == null) return null;

                mangaUserInfo = new Core.Entities.Mangas.MangaUserData
                {
                    MangaId = manga.Id,
                    UserId = userId,
                    User = user,
                    Rating = rating
                };

                var dbMangaUserInfo = await _mangaUserDataRepo.Create(mangaUserInfo);
                return await MapMangaUserData(dbMangaUserInfo);
            }
            else
            {
                //Change
                mangaUserInfo.Rating = rating;
                var dbMangaUserInfo = await _mangaUserDataRepo.Update(mangaUserInfo);
                return await MapMangaUserData(dbMangaUserInfo);
            }
        }



        private async Task<MangaUserDataDTO> MapMangaUserData(Core.Entities.Mangas.MangaUserData mangaUserData)
        {
            var lastStatusUpdate = await _mangaChangedStatusActionRepo.GetLastMangaUserStatusUpdate(mangaUserData.UserId, mangaUserData.MangaId);
            var lastChapterReadUpdate = await _mangaChangedChapterStateActionRepo.GetLastChapterUserReadFromManga(mangaUserData.UserId, mangaUserData.MangaId);
            var firstChapterReadUpdate = await _mangaChangedChapterStateActionRepo.GetFirstChapterUserReadFromManga(mangaUserData.UserId, mangaUserData.MangaId);
            var manga = mangaUserData.Manga;

            var filteredReadChapters = mangaUserData?.ChaptersRead?.Count ?? 0;
            var filteredTotalChapters = manga.Chapters.Count;
            if (!mangaUserData?.User.MangasConfigurations.ShowProgressForChaptersWithDecimals ?? false)
            {
                filteredTotalChapters = manga.Chapters.Where(c => (c.Name % 1) == 0).Count();
                filteredReadChapters = mangaUserData?.ChaptersRead?.Where(cr => (cr.Name % 1) == 0).Count() ?? 0;

            }

            return new MangaUserDataDTO
            {
                AddedToStatusDate = lastStatusUpdate?.CreatedAt,
                FilteredReadChapters = filteredReadChapters,
                FilteredTotalChapters = filteredTotalChapters,
                Rating =  mangaUserData?.Rating,
                ChaptersRead = mangaUserData?.ChaptersRead is not null ? mangaUserData.ChaptersRead.Select(c => c.Id).ToList() : new List<int>(),
                FinishedReadingDate = lastChapterReadUpdate?.CreatedAt,
                Manga = _mapper.Map<MangaInfoDTO>(manga),
                StartedReadingDate = firstChapterReadUpdate?.CreatedAt,
                Status = _mapper.Map<MangaUserStatusEnumDTO>(mangaUserData?.Status),
                UserId = mangaUserData?.UserId ?? -1
            };
        }
    }
}
