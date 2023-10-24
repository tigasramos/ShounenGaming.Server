using ShounenGaming.Business.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Services.Base
{
    public class ImageService : IImageService
    {
        public async Task<byte[]> GetImage(string imagePath)
        {
            return await File.ReadAllBytesAsync(imagePath);
        }

        public async Task SaveImage(byte[] image, string pathToSave)
        {
            new FileInfo(pathToSave).Directory?.Create();
            await File.WriteAllBytesAsync(pathToSave, image);
        }

        public List<string> GetFilesFromFolder(string folderPath)
        {
            var files = new DirectoryInfo(folderPath).GetFiles();
            if (files is null) return new List<string>();
            return files
                .OrderBy(f => Convert.ToInt16(f.Name.Split(".").First()))
                .Select(f => $"{folderPath}/{f.Name}")
                .ToList();
        }
    }
}
