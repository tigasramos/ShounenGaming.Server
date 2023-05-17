using AutoMapper;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Models.Mangas;
using ShounenGaming.Business.Models.Mangas.Enums;
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
        private readonly IMangaUserDataRepository _mangaUserDataRepo;

        private readonly IMapper _mapper;

        public MangaUserDataService(IMangaUserDataRepository mangaUserDataRepo, IMapper mapper)
        {
            _mangaUserDataRepo = mangaUserDataRepo;
            _mapper = mapper;
        }

        public async Task<MangaUserDataDTO?> GetMangaDataByUserAndManga(int userId, int mangaId)
        {
            return _mapper.Map<MangaUserDataDTO?>(await _mangaUserDataRepo.GetByUserAndManga(userId, mangaId));
        }

        public Task<List<MangaInfoDTO>> GetMangasByStatusByUser(int userId, MangaUserStatusEnumDTO status)
        {
            throw new NotImplementedException();
        }

        public async Task<List<MangaUserDataDTO>> GetMangasDataByUser(int userId)
        {
            return _mapper.Map<List<MangaUserDataDTO>>(await _mangaUserDataRepo.GetByUser(userId));
        }

        

        public Task MarkChapterRead(int userId, int chapterId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMangaStatusByUser(int userId, int mangaId, MangaUserStatusEnumDTO status)
        {
            throw new NotImplementedException();
        }
    }
}
