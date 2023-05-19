using ShounenGaming.Business.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    }
}
