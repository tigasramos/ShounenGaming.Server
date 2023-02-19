using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.DataAccess.Interfaces.Mangas;

namespace ShounenGaming.API.Controllers.Base
{
    [Route("api/[controller]")]
    [ApiController]
    public class MangasController : ControllerBase
    {
        private readonly IMangaRepository _repository;
        private readonly IMangaWriterRepository _Wrepository;

        public MangasController(IMangaRepository repository, IMangaWriterRepository wrepository)
        {
            _repository = repository;
            _Wrepository = wrepository;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            //Create Manga
            var manga = await _repository.Create(new Core.Entities.Mangas.Manga
            {
                Name = "test",
                AlternativeNames = new List<Core.Entities.Mangas.MangaAlternativeName> { new Core.Entities.Mangas.MangaAlternativeName { Name = "Alternative"}  },
                Type = Core.Entities.Mangas.Enums.MangaType.MANGA,
                Description = "Description",
                Chapters = new List<Core.Entities.Mangas.MangaChapter>(),
                Writer = new Core.Entities.Mangas.MangaWriter { Name = "Manel"},
                IsReleasing = true,
                Tags = new List<Core.Entities.Mangas.MangaTag> { new Core.Entities.Mangas.MangaTag { Name = "Test"} },
                
            });
            return Ok(manga);
        }


        [HttpGet("test2/{id}")]
        public async Task<IActionResult> Test2(int id)
        {
            //Delete Manga
            var manga = await _repository.Delete(id);
            return Ok(manga);
        }

        [HttpGet("test3")]
        public async Task<IActionResult> Test3()
        {
            //Get Mangas
            var mangas = await _repository.GetAll();
            return Ok(mangas);
        }

        [HttpGet("test4")]
        public async Task<IActionResult> Test4()
        {
            //Get Writers
            var writers = await _Wrepository.GetAll();
            return Ok(writers);
        }


    }
}
