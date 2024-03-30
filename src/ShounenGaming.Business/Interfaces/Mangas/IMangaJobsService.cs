using ShounenGaming.Business.Helpers;
using ShounenGaming.DTOs.Models.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas
{
    public interface IMangaJobsService
    {
        Task AddOrUpdateAllMangasMetadata();

        Task AddNewSeasonMangas();

        Task<int> UpdateAllMangasMetadata();

        Task DownloadImagesAndUpdateChapters();

        Task AddAllMangasToChaptersQueue(); 
        Task UpdateMangaChapters(QueuedManga queuedManga);
    }
}
