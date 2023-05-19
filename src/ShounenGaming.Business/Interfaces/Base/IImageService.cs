using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Base
{
    public interface IImageService
    {
        Task SaveImage(byte[] image, string pathToSave);
        Task<byte[]> GetImage(string imagePath);
    }
}

/*
    mangas/
        one-piece/
            thumbnail.jpeg
            chapters/
                en/
                    1/
                        1-page.jpeg
                        2-page.jpeg
                pt/
                    1/
                        {order}-page.jpeg
                        2-page.jpeg



 */