using Microsoft.AspNetCore.Http;
using ShounenGaming.Business.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Base
{
    public interface IFileService
    {
        Task<int> UploadFile(IFormFile file);
        Task<InternFileData> DownloadFile(int id);
    }
}
