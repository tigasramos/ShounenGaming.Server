using AutoMapper;
using HtmlAgilityPack;
using JikanDotNet;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers;
using ShounenGaming.Business.Models.Base;
using ShounenGaming.Business.Models.Mangas;
using ShounenGaming.Business.Models.Mangas.Enums;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Services.Mangas
{
    public class MangaService : IMangaService
    {
        private readonly IMangaRepository _mangaRepo;
        private readonly IMangaUserDataRepository _mangaUserDataRepo;
        private readonly IMangaWriterRepository _mangaWriterRepo;
        private readonly IMangaTagRepository _mangaTagRepo;

        private readonly IMapper _mapper;

        public MangaService(IMangaRepository mangaRepo, IMangaUserDataRepository mangaUserDataRepo, IMangaWriterRepository mangaWriterRepo, IMangaTagRepository mangaTagRepo, IMapper mapper)
        {
            _mangaRepo = mangaRepo;
            _mangaUserDataRepo = mangaUserDataRepo;
            _mangaWriterRepo = mangaWriterRepo;
            _mangaTagRepo = mangaTagRepo;
            _mapper = mapper;
        }


        public async Task<MangaDTO> GetMangaById(int id)
        {
            var manga = await _mangaRepo.GetById(id);
            if (manga == null)
                throw new EntityNotFoundException("Manga");
            return _mapper.Map<MangaDTO>(manga);
        }

        public async Task<List<MangaDTO>> SearchMangaByName(string name)
        {
            var mangas = await _mangaRepo.SearchMangaByName(name);
            return _mapper.Map<List<MangaDTO>>(mangas);
        }


        public async Task<List<MangaInfoDTO>> GetPopularMangas()
        {
            var popularMangas = await _mangaRepo.GetPopularMangas(5);
            if (popularMangas == null)
                throw new Exception("Error Fetching Popular Mangas");

            if (popularMangas.Count < 5)
            {
                //TODO: Think about it
            }

            return _mapper.Map<List<MangaInfoDTO>>(popularMangas);
        }

        public async Task<List<MangaInfoDTO>> GetMangasByStatusByUser(int userId, MangaUserStatusEnumDTO status)
        {
            var mangas = await _mangaUserDataRepo.GetByStatusByUser(_mapper.Map<MangaUserStatusEnum>(status), userId);
            return _mapper.Map<List<MangaInfoDTO>>(mangas);
        }


        public async Task<List<MangaInfoDTO>> GetRecentlyAddedMangas()
        {
            var mangas = await _mangaRepo.GetRecentlyAddedMangas();
            return _mapper.Map<List<MangaInfoDTO>>(mangas);
        }

        public async Task<List<ChapterReleaseDTO>> GetRecentlyReleasedChapters()
        {
            var chapters = await _mangaRepo.GetRecentlyReleasedChapters();
            return _mapper.Map<List<ChapterReleaseDTO>>(chapters);
        }

        public async Task<List<MangaWriterDTO>> GetMangaWriters()
        {
            var writers = await _mangaWriterRepo.GetAll();
            return _mapper.Map<List<MangaWriterDTO>>(writers);
        }

        public async Task<List<string>> GetMangaTags()
        {
            var tags = await _mangaTagRepo.GetAll();
            return tags.Select(t => t.Name).ToList();
        }

        public async Task FetchMangas()
        {
            var scrappers = new List<IBaseMangaScrapper>() { new FireMangasScrapper(), new GekkouScansScrapper(),
                new NeoXScansScrapper(), new ManganatoScrapper(), new SilenceScansScrapper(), new HuntersScansScrapper(), 
                new BRMangasScrapper() };

            var scrapper = scrappers[4];

            var mangas = await scrapper.GetAllMangas();
            var manga = await scrapper.GetManga(mangas[3].Link);
            var chapterPages = await scrapper.GetChapterImages(manga.Chapters[0].Link);

            IJikan jikan = new Jikan();
            var onePiece = await jikan.SearchMangaAsync("One Piece");



            





           


            



            //TODO: 
            //LerManga ??
            //PrismaScans - Weird
            //ImperioScans - Weird


            List<string> wikiLink = new List<string>();

            
        }
    }
}
