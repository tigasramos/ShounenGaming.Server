using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.Business.Interfaces.Base;

namespace ShounenGaming.API.Controllers.Base
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {

        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Receive form data containing a file, save file locally with a unique id as the name, and return the unique id
        /// </summary>
        /// <param name="file">Received IFormFile file</param>
        /// <returns>IAction Result</returns>
        [HttpPost("upload"), DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var id = await _fileService.UploadFile(file);
            return Ok(id);
        }

        /// <summary>
        /// Return a locally stored file based on id to the requesting client
        /// </summary>
        /// <param name="id">unique identifier for the requested file</param>
        /// <returns>an IAction Result</returns>
        [HttpGet("download/{id}"), DisableRequestSizeLimit]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _fileService.DownloadFile(id);
            return file.Extension switch
            {
                "png" => File(file.Data, "image/png", file.Name),
                "jpg" => File(file.Data, "image/jpg", file.Name),
                "jpeg" => File(file.Data, "image/jpeg", file.Name),
                _ => File(file.Data, "application/octet-stream", file.Name),
            };
        }
    }
}
