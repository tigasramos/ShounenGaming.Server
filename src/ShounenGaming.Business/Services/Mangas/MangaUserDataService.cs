using AutoMapper;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Models.Mangas;
using ShounenGaming.Business.Models.Mangas.Enums;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Services.Mangas
{
    public class MangaUserDataService : IMangaUserDataService
    {
        private readonly IMangaRepository _mangaRepository;
        private readonly IMangaUserDataRepository _mangaUserDataRepo;

        private readonly IMapper _mapper;

        public MangaUserDataService(IMangaUserDataRepository mangaUserDataRepo, IMapper mapper, IMangaRepository mangaRepository)
        {
            _mangaUserDataRepo = mangaUserDataRepo;
            _mapper = mapper;
            _mangaRepository = mangaRepository;
        }

        public async Task<MangaUserDataDTO?> GetMangaDataByMangaByUser(int userId, int mangaId)
        {
            var info = await _mangaUserDataRepo.GetByUserAndManga(userId, mangaId);
            return _mapper.Map<MangaUserDataDTO?>(info);
        }

        public async Task<List<MangaInfoDTO>> GetMangasByStatusByUser(int userId, MangaUserStatusEnumDTO status)
        {
            var mangas = await _mangaUserDataRepo.GetMangasByStatusByUser(_mapper.Map<MangaUserStatusEnum>(status), userId);
            return _mapper.Map<List<MangaInfoDTO>>(mangas);
        }        

        public async Task MarkChapterRead(int userId, int chapterId)
        {
            var manga = await _mangaRepository.GetByChapter(chapterId) ?? throw new Exception("Chapter not Found");
            var mangaUserInfo = await _mangaUserDataRepo.GetByUserAndManga(userId, manga.Id);
            var chapter = manga.Chapters.First(c => c.Id == chapterId);

            if (mangaUserInfo is null)
            {
                mangaUserInfo = new Core.Entities.Mangas.MangaUserData
                {
                    MangaId = manga.Id,
                    UserId = userId,
                    Status = MangaUserStatusEnum.READING
                };
                await _mangaUserDataRepo.Create(mangaUserInfo);
            }

            if (!mangaUserInfo.ChaptersRead.Any(c => c.Id == chapterId))
            {
                mangaUserInfo.ChaptersRead.Add(chapter);
                await _mangaUserDataRepo.Update(mangaUserInfo);
            }
        }

        public async Task UnmarkChapterRead(int userId, int chapterId)
        {
            var manga = await _mangaRepository.GetByChapter(chapterId) ?? throw new Exception("Chapter not Found");
            var mangaUserInfo = await _mangaUserDataRepo.GetByUserAndManga(userId, manga.Id);
            var chapter = manga.Chapters.First(c => c.Id == chapterId);

            if (mangaUserInfo is null) return;

            if (mangaUserInfo.ChaptersRead.Any(c => c.Id == chapterId))
            {
                mangaUserInfo.ChaptersRead.Remove(chapter);
                await _mangaUserDataRepo.Update(mangaUserInfo);
            }
        }

        public async Task UpdateMangaStatusByUser(int userId, int mangaId, MangaUserStatusEnumDTO status)
        {
            var manga = await _mangaRepository.GetById(mangaId) ?? throw new Exception("Manga not Found");
            var mangaUserInfo = await _mangaUserDataRepo.GetByUserAndManga(userId, manga.Id);

            if (mangaUserInfo is null)
            {
                mangaUserInfo = new Core.Entities.Mangas.MangaUserData
                {
                    MangaId = manga.Id,
                    UserId = userId,
                    Status = _mapper.Map<MangaUserStatusEnum>(status)
                };
                await _mangaUserDataRepo.Create(mangaUserInfo);
            } else
            {
                mangaUserInfo.Status = _mapper.Map<MangaUserStatusEnum>(status);
                await _mangaUserDataRepo.Update(mangaUserInfo);
            }
        }
    }
}
