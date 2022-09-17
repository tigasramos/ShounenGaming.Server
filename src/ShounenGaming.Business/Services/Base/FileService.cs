using Microsoft.AspNetCore.Http;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Models.Base;
using ShounenGaming.DataAccess.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Services.Base
{
    public class FileService : IFileService
    {
        private const string FILE_PATH = "./Files";
        private readonly IFileDataRepository _fileDataRepo;

        public FileService(IFileDataRepository fileDataRepo)
        {
            _fileDataRepo = fileDataRepo;
        }

        public async Task<InternFileData> DownloadFile(int id)
        {
            var file = await _fileDataRepo.GetById(id);
            if (file == null)
                throw new EntityNotFoundException("File");

            string path = Path.Combine(FILE_PATH, file.FileId);
            if (File.Exists(path))
            {
                byte[] b = await File.ReadAllBytesAsync(path);

                return new InternFileData { Data=  b, Extension = file.Extension, Name = file.FileName};
            }

            throw new Exception("Not Found");
        }

        public async Task<int> UploadFile(IFormFile file)
        { 
            string newFileName = DateTime.Now.Ticks + "_" + Guid.NewGuid().ToString();

            Directory.CreateDirectory(FILE_PATH);
            var filePath = Path.Combine(FILE_PATH, newFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var dbFile = await _fileDataRepo.Create(new Core.Entities.Base.FileData
            {
                FileName = file.FileName,
                Extension = file.Name.Split(".").Last(),
                FileId = newFileName
            }); 

            return dbFile.Id;
        }
    }
}
